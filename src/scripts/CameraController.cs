using Godot;
using System;
public partial class CameraController : Camera3D
{
    [Export] Node3D Tracker;
    Vector3 Offset;
    public override void _PhysicsProcess(double delta)
    {
        if (Tracker != null)
        {
            GlobalPosition = GlobalPosition.Lerp(Tracker.GlobalPosition + Offset, 0.2f);
        }
    }

    public override void _Ready()
    {
        if (Tracker == null)
            GD.PrintErr("CaneraController: No initial tracker");

        Offset = GlobalPosition - Tracker.GlobalPosition;

        if (Tracker is IEventHandler)
        {
            ((IEventHandler)Tracker).Event += delegate (EventData data)
            {
                if (data.Type == EventType.DEATH)
                    Tracker = null;
            };
        }
    }
}
