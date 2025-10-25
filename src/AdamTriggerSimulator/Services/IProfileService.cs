using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.Services;

/// <summary>
/// Service for managing device profiles and sequences persistence.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Loads all saved device profiles.
    /// </summary>
    Task<List<DeviceProfile>> LoadProfilesAsync();

    /// <summary>
    /// Saves all device profiles.
    /// </summary>
    Task SaveProfilesAsync(List<DeviceProfile> profiles);

    /// <summary>
    /// Loads all saved trigger sequences.
    /// </summary>
    Task<List<TriggerSequence>> LoadSequencesAsync();

    /// <summary>
    /// Saves all trigger sequences.
    /// </summary>
    Task SaveSequencesAsync(List<TriggerSequence> sequences);

    /// <summary>
    /// Gets the application data directory path.
    /// </summary>
    string GetDataDirectory();
}
