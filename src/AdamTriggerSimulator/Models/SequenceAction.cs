using System.Text.Json.Serialization;

namespace AdamTriggerSimulator.Models;

/// <summary>
/// Base class for all sequence actions. Provides extensibility for future action types.
/// </summary>
[JsonDerivedType(typeof(SetInputHighAction), typeDiscriminator: "setHigh")]
[JsonDerivedType(typeof(SetInputLowAction), typeDiscriminator: "setLow")]
[JsonDerivedType(typeof(DelayAction), typeDiscriminator: "delay")]
public abstract class SequenceAction
{
    /// <summary>
    /// Unique identifier for this action.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets a user-friendly description of this action for display purposes.
    /// </summary>
    public abstract string GetDescription();

    /// <summary>
    /// Gets the action type name for UI display.
    /// </summary>
    public abstract string ActionType { get; }
}
