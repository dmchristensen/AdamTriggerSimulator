namespace AdamTriggerSimulator.Models;

/// <summary>
/// Represents a named sequence of actions to execute on an ADAM 6060 device.
/// </summary>
public class TriggerSequence
{
    /// <summary>
    /// Unique identifier for this sequence.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-friendly name for this sequence (e.g., "Door Open/Close Cycle").
    /// </summary>
    public string Name { get; set; } = "New Sequence";

    /// <summary>
    /// Optional description of what this sequence does.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Ordered list of actions to execute.
    /// </summary>
    public List<SequenceAction> Actions { get; set; } = new();

    /// <summary>
    /// Number of times to repeat the sequence (0 = infinite loop).
    /// </summary>
    public int LoopCount { get; set; } = 1;

    /// <summary>
    /// Timestamp when this sequence was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Timestamp when this sequence was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets the total duration of the sequence in milliseconds (delays only).
    /// </summary>
    public int GetTotalDelayDuration()
    {
        return Actions.OfType<DelayAction>().Sum(a => a.DurationMs);
    }

    /// <summary>
    /// Gets the total number of actions in this sequence.
    /// </summary>
    public int ActionCount => Actions.Count;
}
