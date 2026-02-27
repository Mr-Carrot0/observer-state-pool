using Godot;
using System;


public partial class PlayerUI : Control
{
    [Export] Player player;
    [Export] ProgressBar HealthBar;
    [Export] Camera3D Cam;

    Vector3 HOffset;

    // update healthbar
    public void UpdateHealthBar(EventData data)
    {
        switch (data.Type)
        {
            case EventType.HURT:

                HealthBar.Value = Mathf.Max(0, player.Health) / (float)Player.MAX_HEALTH * 100f;
                break;

            case EventType.DEATH:
                HealthBar.Value = 0f;
                break;
        }
    }

    public override void _Process(double delta)
    {
        if (player.Dead)
        {
            Color To = Colors.Transparent;
            // To.A = 0;
            HealthBar.Modulate = HealthBar.Modulate.Lerp(To, Mathf.Min(1f, 3f * (float)delta));

        }
        if (HealthBar.Modulate.A > 0.02f)
            HealthBar.GlobalPosition = Cam.UnprojectPosition(player.GlobalPosition + HOffset);
    }
    public override void _Ready()
    {
        HOffset = new(-HealthBar.Size.X / 160f, HealthBar.Size.Y / 15f, 0);
        player.Event += UpdateHealthBar;

    }
}
