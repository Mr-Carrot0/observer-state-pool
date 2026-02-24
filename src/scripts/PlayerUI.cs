using Godot;
using System;


public partial class PlayerUI : ObserverNode
{
    [Export] Player player;
    [Export] ProgressBar HealthBar;
    [Export] Camera3D Cam;

    public override void Action(EventData data)
    {
        if (data.Type == EventType.P_HURT)
        {
            GD.Print("HEALTH: ", player.Health);
            HealthBar.Value = player.Health / Player.MAX_HEALTH * 100;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 size = HealthBar.Size;
        HealthBar.GlobalPosition = Cam.UnprojectPosition(player.GlobalPosition) - new Vector2(size.X / 2, size.Y * 5f);
        // + Vector3.Up * HealthBar.Size.Y / 15.4f)
        // + new Vector2(HealthBar.Size.X / -2.2f, 0);
    }
    public override void _Ready()
    {
        player.GameActions += this.Action;
    }
    public override void _ExitTree()
    {
        player.GameActions -= this.Action;
    }
}
