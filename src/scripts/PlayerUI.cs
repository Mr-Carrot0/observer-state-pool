using Godot;
using System;


public partial class PlayerUI : ObserverNode
{
    [Export] Player Anchor;
    [Export] ProgressBar HealthBar;
    [Export] Camera3D Cam;

    // update healthbar
    public override void Action(EventData data)
    {
        if (data.Type == EventType.P_HURT)
        {
            GD.Print("HEALTH: ", Anchor.Health);
            // GD.Print("BAR_BEFORE: ", HealthBar.Value);
            HealthBar.Value = (float)Anchor.Health / (float)Player.MAX_HEALTH * 100f;
            // GD.Print("BAR_AFTER: ", HealthBar.Value);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 size = HealthBar.Size;
        HealthBar.GlobalPosition = Cam.UnprojectPosition(Anchor.GlobalPosition)
            - new Vector2(size.X / 2, size.Y * 5f);
        // + Vector3.Up * HealthBar.Size.Y / 15.4f)
        // + new Vector2(HealthBar.Size.X / -2.2f, 0);
    }
}
