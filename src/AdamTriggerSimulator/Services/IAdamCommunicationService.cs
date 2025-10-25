using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.Services;

/// <summary>
/// Service for communicating with ADAM 6060 devices via UDP.
/// </summary>
public interface IAdamCommunicationService
{
    /// <summary>
    /// Sets a specific digital input to the specified state.
    /// </summary>
    /// <param name="profile">The device profile to send the command to.</param>
    /// <param name="inputChannel">The input channel (0-5 for DI0-DI5).</param>
    /// <param name="state">The desired state (HIGH or LOW).</param>
    /// <returns>A command log entry with the result of the operation.</returns>
    Task<CommandLogEntry> SetInputStateAsync(DeviceProfile profile, int inputChannel, DigitalInputState state);

    /// <summary>
    /// Sets multiple digital inputs to the specified states.
    /// </summary>
    /// <param name="profile">The device profile to send the command to.</param>
    /// <param name="inputStates">Dictionary mapping input channels to their desired states.</param>
    /// <returns>A command log entry with the result of the operation.</returns>
    Task<CommandLogEntry> SetMultipleInputStatesAsync(DeviceProfile profile, Dictionary<int, DigitalInputState> inputStates);

    /// <summary>
    /// Executes a complete trigger sequence on the target device.
    /// </summary>
    /// <param name="profile">The device profile to send commands to.</param>
    /// <param name="sequence">The sequence to execute.</param>
    /// <param name="cancellationToken">Token to cancel the sequence execution.</param>
    /// <returns>List of command log entries for each action executed.</returns>
    Task<List<CommandLogEntry>> ExecuteSequenceAsync(DeviceProfile profile, TriggerSequence sequence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connectivity to an ADAM 6060 device.
    /// </summary>
    /// <param name="profile">The device profile to test.</param>
    /// <returns>A command log entry indicating success or failure.</returns>
    Task<CommandLogEntry> TestConnectionAsync(DeviceProfile profile);

    /// <summary>
    /// Gets the model number from the ADAM device.
    /// </summary>
    /// <param name="profile">The device profile to query.</param>
    /// <returns>A command log entry with the model number.</returns>
    Task<CommandLogEntry> GetModelNumberAsync(DeviceProfile profile);

    /// <summary>
    /// Gets the device name from the ADAM device.
    /// </summary>
    /// <param name="profile">The device profile to query.</param>
    /// <returns>A command log entry with the device name.</returns>
    Task<CommandLogEntry> GetDeviceNameAsync(DeviceProfile profile);

    /// <summary>
    /// Sets all digital outputs (DI0-DI5) to LOW state.
    /// </summary>
    /// <param name="profile">The device profile to send the command to.</param>
    /// <returns>A command log entry with the result of the operation.</returns>
    Task<CommandLogEntry> SetAllLowAsync(DeviceProfile profile);

    /// <summary>
    /// Sets all digital outputs (DI0-DI5) to HIGH state.
    /// </summary>
    /// <param name="profile">The device profile to send the command to.</param>
    /// <returns>A command log entry with the result of the operation.</returns>
    Task<CommandLogEntry> SetAllHighAsync(DeviceProfile profile);

    /// <summary>
    /// Reads the current status of all digital outputs from the device.
    /// </summary>
    /// <param name="profile">The device profile to query.</param>
    /// <returns>A command log entry with the result and current states.</returns>
    Task<CommandLogEntry> GetCurrentStatusAsync(DeviceProfile profile);

    /// <summary>
    /// Gets the current internal state of all digital outputs.
    /// </summary>
    /// <returns>Dictionary mapping output channel to its current state.</returns>
    Dictionary<int, DigitalInputState> GetCurrentStates();

    /// <summary>
    /// Sends a custom UDP command and receives the response.
    /// </summary>
    /// <param name="profile">The device profile to send the command to.</param>
    /// <param name="command">The raw command string to send.</param>
    /// <returns>A command log entry with the result and response.</returns>
    Task<CommandLogEntry> SendCommandAsync(DeviceProfile profile, string command);
}
