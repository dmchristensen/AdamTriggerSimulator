namespace AdamTriggerSimulator.Models;

/// <summary>
/// Represents a saved device configuration profile for an ADAM 6060 unit.
/// </summary>
public class DeviceProfile
{
    /// <summary>
    /// Unique identifier for the profile.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-friendly name for the profile (e.g., "Main Door", "Cabinet 1").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the ADAM 6060 device.
    /// Example: 192.168.1.100
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Port number for UDP communication (default: 1025).
    /// </summary>
    public int Port { get; set; } = 1025;

    /// <summary>
    /// Optional description or notes about this device.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this profile was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Timestamp when this profile was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.Now;
}
