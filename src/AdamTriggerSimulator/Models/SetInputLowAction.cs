namespace AdamTriggerSimulator.Models;

/// <summary>
/// Action to set one or more digital inputs to LOW state.
/// </summary>
public class SetInputLowAction : SequenceAction
{
    /// <summary>
    /// The digital input channel to set LOW (0-5 for DI0-DI5).
    /// </summary>
    public int InputChannel { get; set; }

    public override string ActionType => "Set LOW";

    public override string GetDescription()
    {
        return $"DI{InputChannel} → LOW";
    }
}
