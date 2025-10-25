namespace AdamTriggerSimulator.Models;

/// <summary>
/// Action to delay/pause sequence execution for a specified duration.
/// </summary>
public class DelayAction : SequenceAction
{
    /// <summary>
    /// Delay duration in milliseconds.
    /// </summary>
    public int DurationMs { get; set; } = 1000;

    public override string ActionType => "Delay";

    public override string GetDescription()
    {
        if (DurationMs < 1000)
        {
            return $"Delay {DurationMs}ms";
        }
        else if (DurationMs % 1000 == 0)
        {
            return $"Delay {DurationMs / 1000}s";
        }
        else
        {
            return $"Delay {DurationMs / 1000.0:F1}s";
        }
    }
}
