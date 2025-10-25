using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.Services;

/// <summary>
/// Implementation of ADAM 6060 communication service using UDP protocol.
/// </summary>
public class AdamCommunicationService : IAdamCommunicationService
{
    private const int TotalOutputs = 6;

    // Semaphore to ensure only one UDP command is sent at a time (prevents response mixing)
    private readonly SemaphoreSlim _commandSemaphore = new(1, 1);

    // Track current state of all outputs (DI0-DI5) - Thread-safe for polling and UI access
    private readonly ConcurrentDictionary<int, DigitalInputState> _currentStates = new(
        new Dictionary<int, DigitalInputState>
        {
            { 0, DigitalInputState.Low },
            { 1, DigitalInputState.Low },
            { 2, DigitalInputState.Low },
            { 3, DigitalInputState.Low },
            { 4, DigitalInputState.Low },
            { 5, DigitalInputState.Low }
        });

    /// <inheritdoc/>
    public async Task<CommandLogEntry> SetInputStateAsync(DeviceProfile profile, int inputChannel, DigitalInputState state)
    {
        if (inputChannel < 0 || inputChannel >= TotalOutputs)
        {
            return CreateErrorLogEntry(profile, "", $"Invalid input channel: {inputChannel}. Must be 0-5.");
        }

        // Update current state
        _currentStates[inputChannel] = state;

        // Convert to Dictionary for API compatibility
        var statesDict = new Dictionary<int, DigitalInputState>(_currentStates);
        return await SetMultipleInputStatesAsync(profile, statesDict);
    }

    /// <inheritdoc/>
    public async Task<CommandLogEntry> SetMultipleInputStatesAsync(DeviceProfile profile, Dictionary<int, DigitalInputState> inputStates)
    {
        try
        {
            // Build the command string based on input states
            string command = BuildOutputCommand(inputStates);

            // Send the command
            await SendCommandAsync(profile.IpAddress, profile.Port, command);

            // Create successful log entry
            var description = string.Join(", ", inputStates.Select(kvp => $"DI{kvp.Key} → {kvp.Value}"));
            return new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = command,
                Success = true,
                Description = description
            };
        }
        catch (Exception ex)
        {
            return CreateErrorLogEntry(profile, "", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<List<CommandLogEntry>> ExecuteSequenceAsync(DeviceProfile profile, TriggerSequence sequence, CancellationToken cancellationToken = default)
    {
        var logEntries = new List<CommandLogEntry>();
        int loopCount = sequence.LoopCount == 0 ? int.MaxValue : sequence.LoopCount;

        try
        {
            for (int loop = 0; loop < loopCount; loop++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                foreach (var action in sequence.Actions)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    CommandLogEntry? logEntry = null;

                    switch (action)
                    {
                        case SetInputHighAction highAction:
                            logEntry = await SetInputStateAsync(profile, highAction.InputChannel, DigitalInputState.High);
                            break;

                        case SetInputLowAction lowAction:
                            logEntry = await SetInputStateAsync(profile, lowAction.InputChannel, DigitalInputState.Low);
                            break;

                        case DelayAction delayAction:
                            await Task.Delay(delayAction.DurationMs, cancellationToken);
                            logEntry = new CommandLogEntry
                            {
                                Timestamp = DateTime.Now,
                                IpAddress = profile.IpAddress,
                                Port = profile.Port,
                                Command = "(delay)",
                                Success = true,
                                Description = delayAction.GetDescription()
                            };
                            break;
                    }

                    if (logEntry != null)
                    {
                        logEntries.Add(logEntry);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Sequence was cancelled, this is expected
            logEntries.Add(new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = "(cancelled)",
                Success = true,
                Description = "Sequence execution cancelled"
            });
        }

        return logEntries;
    }

    /// <inheritdoc/>
    public async Task<CommandLogEntry> TestConnectionAsync(DeviceProfile profile)
    {
        try
        {
            // Get firmware version
            string versionCommand = "$01F\r";
            string versionResponse = await SendCommandAndReceiveAsync(profile.IpAddress, profile.Port, versionCommand);
            string versionInfo = "Unknown";
            if (!string.IsNullOrEmpty(versionResponse) && versionResponse.StartsWith("!01"))
            {
                versionInfo = versionResponse.Substring(3).Trim();
            }

            // Get model number
            string modelCommand = "$01M\r";
            string modelResponse = await SendCommandAndReceiveAsync(profile.IpAddress, profile.Port, modelCommand);
            string modelNumber = "Unknown";
            if (!string.IsNullOrEmpty(modelResponse) && modelResponse.StartsWith("!01"))
            {
                modelNumber = modelResponse.Substring(3).Trim();
            }

            // Get device name
            string nameCommand = "$01N\r";
            string nameResponse = await SendCommandAndReceiveAsync(profile.IpAddress, profile.Port, nameCommand);
            string deviceName = "Unknown";
            if (!string.IsNullOrEmpty(nameResponse) && nameResponse.StartsWith("!01"))
            {
                deviceName = nameResponse.Substring(3).Trim();
            }

            return new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = $"{versionCommand}, {modelCommand}, {nameCommand}",
                Response = $"Version: {versionInfo} | Model: {modelNumber} | Name: {deviceName}",
                Success = true,
                Description = $"Connected - Version: {versionInfo}, Model: {modelNumber}, Name: {deviceName}"
            };
        }
        catch (Exception ex)
        {
            return CreateErrorLogEntry(profile, "$01F", $"Connection failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<CommandLogEntry> GetModelNumberAsync(DeviceProfile profile)
    {
        try
        {
            // Send model query command
            string modelCommand = "$01M\r";
            string response = await SendCommandAndReceiveAsync(profile.IpAddress, profile.Port, modelCommand);

            // Parse model response
            // Expected format: !01{model_number} (e.g., !016060)
            string modelNumber = "Unknown model";
            if (!string.IsNullOrEmpty(response) && response.StartsWith("!01"))
            {
                modelNumber = response.Substring(3).Trim();
            }

            return new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = modelCommand,
                Response = response,
                Success = true,
                Description = $"Model: {modelNumber}"
            };
        }
        catch (Exception ex)
        {
            return CreateErrorLogEntry(profile, "$01M", $"Failed to get model: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<CommandLogEntry> GetDeviceNameAsync(DeviceProfile profile)
    {
        try
        {
            // Send name query command
            string nameCommand = "$01N\r";
            string response = await SendCommandAndReceiveAsync(profile.IpAddress, profile.Port, nameCommand);

            // Parse name response
            // Expected format: !01{device_name}
            string deviceName = "Unknown name";
            if (!string.IsNullOrEmpty(response) && response.StartsWith("!01"))
            {
                deviceName = response.Substring(3).Trim();
            }

            return new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = nameCommand,
                Response = response,
                Success = true,
                Description = $"Name: {deviceName}"
            };
        }
        catch (Exception ex)
        {
            return CreateErrorLogEntry(profile, "$01N", $"Failed to get name: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<CommandLogEntry> SetAllLowAsync(DeviceProfile profile)
    {
        try
        {
            // Set all outputs to LOW
            for (int i = 0; i < TotalOutputs; i++)
            {
                _currentStates[i] = DigitalInputState.Low;
            }

            string command = BuildOutputCommand(_currentStates);
            await SendCommandAsync(profile.IpAddress, profile.Port, command);

            return new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = command,
                Success = true,
                Description = "All outputs set to LOW"
            };
        }
        catch (Exception ex)
        {
            return CreateErrorLogEntry(profile, "", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<CommandLogEntry> SetAllHighAsync(DeviceProfile profile)
    {
        try
        {
            // Set all outputs to HIGH
            for (int i = 0; i < TotalOutputs; i++)
            {
                _currentStates[i] = DigitalInputState.High;
            }

            string command = BuildOutputCommand(_currentStates);
            await SendCommandAsync(profile.IpAddress, profile.Port, command);

            return new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = command,
                Success = true,
                Description = "All outputs set to HIGH"
            };
        }
        catch (Exception ex)
        {
            return CreateErrorLogEntry(profile, "", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<CommandLogEntry> GetCurrentStatusAsync(DeviceProfile profile)
    {
        try
        {
            // Send status query command and receive response
            string queryCommand = "$01C\r";
            string response = await SendCommandAndReceiveAsync(profile.IpAddress, profile.Port, queryCommand);

            // Parse response: !01{DI0}{DI1}{DI2}{DI3}{DI4}{DI5}000000000000
            if (string.IsNullOrEmpty(response) || !response.StartsWith("!01"))
            {
                return CreateErrorLogEntry(profile, queryCommand, $"Invalid response: {response}");
            }

            // Extract status for each input (starts at position 3, each input is 2 chars)
            var statusList = new List<string>();
            for (int i = 0; i < TotalOutputs; i++)
            {
                int position = 3 + (i * 2); // Position in response string
                if (position + 2 <= response.Length)
                {
                    string stateStr = response.Substring(position, 2);
                    DigitalInputState state = stateStr == "80" ? DigitalInputState.High : DigitalInputState.Low;
                    _currentStates[i] = state;
                    statusList.Add($"DI{i}={state}");
                }
            }

            return new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = queryCommand,
                Success = true,
                Description = $"Status read: {string.Join(", ", statusList)}"
            };
        }
        catch (Exception ex)
        {
            return CreateErrorLogEntry(profile, "$01C", ex.Message);
        }
    }

    /// <inheritdoc/>
    public Dictionary<int, DigitalInputState> GetCurrentStates()
    {
        return new Dictionary<int, DigitalInputState>(_currentStates);
    }

    /// <inheritdoc/>
    public async Task<CommandLogEntry> SendCommandAsync(DeviceProfile profile, string command)
    {
        try
        {
            string response = await SendCommandAndReceiveAsync(profile.IpAddress, profile.Port, command);

            return new CommandLogEntry
            {
                Timestamp = DateTime.Now,
                IpAddress = profile.IpAddress,
                Port = profile.Port,
                Command = command,
                Response = response,
                Success = true,
                Description = $"Manual command - Response: {response}"
            };
        }
        catch (Exception ex)
        {
            return CreateErrorLogEntry(profile, command, ex.Message);
        }
    }

    /// <summary>
    /// Builds the ADAM 6060 command string to set digital outputs.
    ///
    /// Protocol Format: $01C{DI0}{DI1}{DI2}{DI3}{DI4}{DI5}000000000000\r
    /// - Each {DIx} is 2 characters: "00" for LOW, "80" for HIGH
    /// - Command includes the state of ALL 6 outputs (DI0-DI5)
    /// </summary>
    private string BuildOutputCommand(IReadOnlyDictionary<int, DigitalInputState> inputStates)
    {
        // Build command by concatenating state of each input (DI0-DI5)
        var commandBuilder = new StringBuilder("$01C");

        for (int i = 0; i < TotalOutputs; i++)
        {
            // Get the state for this input (default to current state if not specified)
            var state = inputStates.ContainsKey(i) ? inputStates[i] : _currentStates[i];

            // Append "80" for HIGH, "00" for LOW
            commandBuilder.Append(state == DigitalInputState.High ? "80" : "00");
        }

        // Append trailing zeros and terminator
        commandBuilder.Append("000000000000\r");

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Sends a UDP command to the ADAM 6060 device.
    /// </summary>
    private async Task SendCommandAsync(string ipAddress, int port, string command)
    {
        await _commandSemaphore.WaitAsync();
        try
        {
            using var client = new UdpClient();
            var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            // Send command
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            await client.SendAsync(commandBytes, commandBytes.Length, endpoint);
        }
        finally
        {
            _commandSemaphore.Release();
        }
    }

    /// <summary>
    /// Sends a UDP command to the ADAM 6060 device and waits for a response.
    /// </summary>
    private async Task<string> SendCommandAndReceiveAsync(string ipAddress, int port, string command)
    {
        await _commandSemaphore.WaitAsync();
        UdpClient? client = null;
        try
        {
            client = new UdpClient();
            var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            // Send command
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            await client.SendAsync(commandBytes, commandBytes.Length, endpoint);

            // Wait for response with timeout using CancellationTokenSource
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            try
            {
                var result = await client.ReceiveAsync(cts.Token);
                string response = Encoding.ASCII.GetString(result.Buffer);
                return response.TrimEnd('\r', '\n');
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("No response received from device within 2 seconds");
            }
        }
        finally
        {
            client?.Dispose();
            _commandSemaphore.Release();
        }
    }

    /// <summary>
    /// Creates an error log entry.
    /// </summary>
    private CommandLogEntry CreateErrorLogEntry(DeviceProfile profile, string command, string errorMessage)
    {
        return new CommandLogEntry
        {
            Timestamp = DateTime.Now,
            IpAddress = profile.IpAddress,
            Port = profile.Port,
            Command = command,
            Success = false,
            ErrorMessage = errorMessage,
            Description = "Command failed"
        };
    }
}
