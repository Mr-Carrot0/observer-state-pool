using Godot;
using System;

public delegate void GameAction(EventData data);
public enum EventType
{
    JUMP,
    P_HURT,
    P_HIT,
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
    // ref delegate IObserver _d(EventData data);
}