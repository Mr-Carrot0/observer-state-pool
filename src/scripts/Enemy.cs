using Godot;
// using Godot.Collections;
using System;
using StateFunc = System.Action<Enemy, float>;

public partial class Enemy : CharacterBody3D
{
    static Vector3 _FlattenVec3(Vector3 from)
    {
        return new(from.X, 0, from.Z);
    }
    // public enum State { IDLE, CHASING, DEAD, WAITING }

    float MyTimer = 0;
    public Vector3 Direction = Vector3.Zero;
    const float LookRadiusSqrd = 16f;

    // [ExportGroup("Physics")]
    const float JumpHieght = 0.3f;
    const float JumpFreq = 5f;
    const float Speed = 2f;

    // [ExportGroup("Nodes")]
    [Export] Player player;
    [ExportGroup("Internal Nodes")]
    [Export] MeshInstance3D Mesh;
    [Export] Node3D Spikes;

    public StateFunc CurrentState = State.IDLE;
    public override void _Ready()
    {
        // AttackRadius = Mesh.Mesh

        player.Event += delegate (EventData data)
        {
            if ((GodotObject)data?.Value == this)
            {
                switch (data.Type)
                {
                    case EventType.HURT:
                    case EventType.DEATH:
                        ChangeState(State.WAITING);
                        break;

                    // gets killed -> set to DISABLED
                    case EventType.HIT:
                        MyTimer = 0;
                        CurrentState = State.DISABLED;
                        break;
                }
            }
        };

    }
    public override void _PhysicsProcess(double delta)
    {
        Update((float)delta);

    }
    private void Update(float delta)
    {
        CurrentState(this, delta);
    }

    public void ChangeState(StateFunc state, bool resetTimer = true)
    {
        if (resetTimer) MyTimer = 0;
        CurrentState = state;
    }

    // }
    private void Hop(float delta, float _JumpFreq = JumpFreq)
    {
        Position = _FlattenVec3(Position);

        Mesh.Position = new(0, JumpHieght * Mathf.Abs(Mathf.Sin(_JumpFreq * MyTimer)), 0);
        MyTimer += delta;
        MyTimer %= Mathf.Pi / _JumpFreq;
    }

    public bool IsPlayerInRange()
    {
        return GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < LookRadiusSqrd;
    }

    public void Squash()
    {
        MyTimer = 0;
        CurrentState = State.DISABLED;
    }
    public void Revive(Vector3 newPos)
    {
        // ProcessMode = ProcessModeEnum.Inherit; // remove after refactoring
        GlobalPosition = newPos;
        Visible = true;
    }

    public static class State
    {
        public static readonly System.Collections.Generic.Dictionary<StateFunc, string> GetNameDebug = new()
        {
            {IDLE, "IDLE"},
            {CHASE, "CHASE"},
            {DISABLED, "DISABLED"},
            {WAITING, "WAITING"},
        };
        public static void CHASE(Enemy mob, float delta)
        {
            if (!mob.IsPlayerInRange() || mob.player.Dead)
            {
                mob.ChangeState(IDLE, false);
                return;
            }

            mob.Direction = _FlattenVec3(mob.GlobalPosition.DirectionTo(mob.player.GlobalPosition)).Normalized();

            mob.Velocity = mob.Velocity.MoveToward(mob.Direction * Speed, 0.3f * Speed);

            mob.Hop(delta);
            mob.MoveAndSlide();
            int ccount = mob.GetSlideCollisionCount();
            for (int i = 0; i < ccount; i++)
            {
                KinematicCollision3D collision = mob.GetSlideCollision(i);

                // GD.Print(ccount);
                if (collision.GetCollider() is Player p)
                {
                    if (Vector3.Up.Dot(collision.GetNormal()) < 0.1f)
                    {
                        p.OnPlayerHurt(mob);
                    }
                    // else // always activated by Player
                    // {
                    //     mob.Squash();
                    //     p.BroadcastAction(EventType.P_HIT, mob);
                    // }
                }
            }

        }
        public static void IDLE(Enemy mob, float delta)
        {
            if (mob.IsPlayerInRange())
            {
                mob.ChangeState(CHASE, false);
                return;
            }
            mob.Hop(delta, JumpFreq / 2);
        }
        public static void WAITING(Enemy mob, float delta)
        {
            mob.MyTimer += delta;
            if (mob.MyTimer > 2f || mob.player.Dead)
                mob.ChangeState(IDLE);
        }
        public static void DISABLED(Enemy mob, float delta)
        {
            if (mob.MyTimer < 0.1f)
            {

                mob.MyTimer = Mathf.Min(1, mob.MyTimer + (float)delta * 0.1f);
                mob.Mesh.Position = mob.Mesh.Position.Lerp(Vector3.Zero, mob.MyTimer);

                if (mob.GlobalPosition.Y > 0f)
                    mob.GlobalPosition = new(mob.GlobalPosition.X
                    , Mathf.Lerp(mob.GlobalPosition.Y, 0f, mob.MyTimer)
                        , mob.GlobalPosition.Z);

                mob.Spikes.Visible = false;
            }
            else
            {
                mob.Visible = false;
                mob.ProcessMode = ProcessModeEnum.Disabled;
            }
        }
    }

}
