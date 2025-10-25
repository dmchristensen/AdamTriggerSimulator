namespace AdamTriggerSimulator.Models;

/// <summary>
/// Represents the state of a digital input on the ADAM 6060.
/// </summary>
public enum DigitalInputState
{
    /// <summary>
    /// Input is in LOW state (0V).
    /// </summary>
    Low = 0,

    /// <summary>
    /// Input is in HIGH state (24V).
    /// </summary>
    High = 1
}
