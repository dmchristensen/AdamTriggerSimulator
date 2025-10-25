namespace AdamTriggerSimulator.Models;

/// <summary>
/// Represents a single entry in the command history log.
/// </summary>
public class CommandLogEntry
{
    /// <summary>
    /// Timestamp when the command was sent.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Target IP address.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Target port number.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// The command string that was sent.
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Whether the command was sent successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the command failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// User-friendly description of what the command does.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Response received from the device (if any).
    /// </summary>
    public string? Response { get; set; }

    /// <summary>
    /// Gets a formatted string for display in the log.
    /// </summary>
    public string FormattedEntry
    {
        get
        {
            var status = Success ? "✓ Success" : $"✗ Failed: {ErrorMessage}";
            return $"[{Timestamp:HH:mm:ss}] {IpAddress}:{Port} → {Command} {status}";
        }
    }
}
