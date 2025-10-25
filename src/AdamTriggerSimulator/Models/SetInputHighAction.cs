namespace AdamTriggerSimulator.Models;

/// <summary>
/// Action to set one or more digital inputs to HIGH state.
/// </summary>
public class SetInputHighAction : SequenceAction
{
    /// <summary>
    /// The digital input channel to set HIGH (0-5 for DI0-DI5).
    /// </summary>
    public int InputChannel { get; set; }

    public override string ActionType => "Set HIGH";

    public override string GetDescription()
    {
        return $"DI{InputChannel} → HIGH";
    }
}
