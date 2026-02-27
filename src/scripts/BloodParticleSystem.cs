using Godot;
using System;

public partial class BloodParticleSystem : GpuParticles3D
{

    [Export]
    public Node EventHandler; // instead of player, use the Interface for Event Registration.


    public override void _Ready()
    {
        // Register on the correct events on "player"
        if (EventHandler is IEventHandler)
        {
            ((IEventHandler)EventHandler).Event += EmitParticles;
        }
        else
        {
            GD.PrintErr("_WARNING: Particles compromised: BloodParticleSystem.Eventhandler is not IEventHandler");
        }
    }

    public void EmitParticles(EventData data)
    {
        // start based on the player logic.
        if (data.Type == EventType.HURT)
        {
            Amount = GD.RandRange(3, 6);
            Emitting = true;
        }
    }
}
