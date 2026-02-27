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

public interface IAnchor
{
    public event GameAction GameActions;
}

public abstract partial class ObserverNode : Node, IObserver
{
    IAnchor Anchor;
    public abstract void Action(EventData data);
    // ref delegate IObserver _d(EventData data);
        public override void _Ready()
    {
        Anchor.GameActions += this.Action;
    }
    public override void _ExitTree()
    {
        Anchor.GameActions -= this.Action;
    }
}