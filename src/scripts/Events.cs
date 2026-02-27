using Godot;
using System;

public delegate void GameEvent(EventData data);
public enum EventType
{
    HURT,
    JUMP,
    HIT,
    DEATH,
}

public class EventData(EventType type, CharacterBody3D value = null)
{
    public EventType Type = type;
    public CharacterBody3D Value = value;
}

public interface IEventHandler
{
    public event GameEvent Event;
}

