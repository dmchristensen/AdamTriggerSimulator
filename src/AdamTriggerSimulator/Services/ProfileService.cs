using System.IO;
using System.Text.Json;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.Services;

/// <summary>
/// Implementation of profile and sequence persistence service using JSON files.
/// </summary>
public class ProfileService : IProfileService
{
    private const string ProfilesFileName = "profiles.json";
    private const string SequencesFileName = "sequences.json";
    private readonly string _dataDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProfileService()
    {
        // Store data in user's AppData folder
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdamTriggerSimulator"
        );

        // Ensure directory exists
        Directory.CreateDirectory(_dataDirectory);

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc/>
    public string GetDataDirectory() => _dataDirectory;

    /// <inheritdoc/>
    public async Task<List<DeviceProfile>> LoadProfilesAsync()
    {
        string filePath = Path.Combine(_dataDirectory, ProfilesFileName);

        if (!File.Exists(filePath))
        {
            // Return empty list if no saved profiles exist
            return new List<DeviceProfile>();
        }

        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            var profiles = JsonSerializer.Deserialize<List<DeviceProfile>>(json, _jsonOptions);
            return profiles ?? new List<DeviceProfile>();
        }
        catch (Exception ex)
        {
            // Log error and return empty list
            Console.WriteLine($"Error loading profiles: {ex.Message}");
            return new List<DeviceProfile>();
        }
    }

    /// <inheritdoc/>
    public async Task SaveProfilesAsync(List<DeviceProfile> profiles)
    {
        string filePath = Path.Combine(_dataDirectory, ProfilesFileName);

        try
        {
            string json = JsonSerializer.Serialize(profiles, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving profiles: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<TriggerSequence>> LoadSequencesAsync()
    {
        string filePath = Path.Combine(_dataDirectory, SequencesFileName);

        if (!File.Exists(filePath))
        {
            // Return example sequence if no saved sequences exist
            return new List<TriggerSequence>
            {
                CreateExampleSequence()
            };
        }

        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            var sequences = JsonSerializer.Deserialize<List<TriggerSequence>>(json, _jsonOptions);
            return sequences ?? new List<TriggerSequence>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sequences: {ex.Message}");
            return new List<TriggerSequence>();
        }
    }

    /// <inheritdoc/>
    public async Task SaveSequencesAsync(List<TriggerSequence> sequences)
    {
        string filePath = Path.Combine(_dataDirectory, SequencesFileName);

        try
        {
            string json = JsonSerializer.Serialize(sequences, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving sequences: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates an example sequence for first-time users.
    /// </summary>
    private TriggerSequence CreateExampleSequence()
    {
        return new TriggerSequence
        {
            Name = "Example: Door Cycle",
            Description = "Opens and closes a door with delays",
            Actions = new List<SequenceAction>
            {
                new SetInputHighAction { InputChannel = 0 },
                new DelayAction { DurationMs = 2000 },
                new SetInputLowAction { InputChannel = 0 },
                new DelayAction { DurationMs = 1000 }
            },
            LoopCount = 1
        };
    }
}
