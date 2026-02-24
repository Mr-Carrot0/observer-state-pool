using Godot;
using System;

public enum EventType
{
    GENERIC,
    P_HURT,
}

public class EventData(EventType type, Variant? value = null)
{
    public EventType Type = type;
    public Variant? Value = value;
}

public interface IObserver
{
    public abstract void Action(EventData data);
}

public abstract partial class ObserverNode : Node, IObserver
{
    public abstract void Action(EventData data);
}