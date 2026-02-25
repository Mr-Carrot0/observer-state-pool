using Godot;
// using Godot.Collections;
using System;
using System.Collections.Generic;
using StateFunc = System.Action<Enemy, float>;

public partial class Enemy : CharacterBody3D
{
    static Vector3 _FlattenVec3(Vector3 from)
    {
        return new(from.X, 0, from.Z);
    }
    // public enum State { IDLE, CHASING, DEAD, WAITING }

    float MyTimer = 0;
    [Export] float JumpHieght = 0.3f;
    [Export] float JumpFreq = 5f;
    [Export] float rangeSqrd = 14f;
    [Export] float Speed = 2f;
    [Export] MeshInstance3D Mesh;
    [Export] Player player;
    public Vector3 Direction = Vector3.Zero;
    // ("#555")
    [Export] Color ColorDeath = Colors.Gray;
    [Export] Color ColorAlive = Colors.DarkOrange;

    // public State CurrentState = State.IDLE;
    // public MState CStateTMP;

    public StateFunc CurrentState = State.IDLE;

    private void ResetParams()
    {
        MyTimer = 0;
    }

    public void ChangeState(StateFunc state, bool resetParams = true)
    {
        if (resetParams) ResetParams();
        CurrentState = state;
    }

    Color MeshColor
    {
        get
        {
            return ((StandardMaterial3D)((PrimitiveMesh)Mesh.Mesh)?.Material)?.AlbedoColor ?? Colors.Black;
        }
        set
        {
            StandardMaterial3D m = (StandardMaterial3D)((PrimitiveMesh)Mesh.Mesh)?.Material;
            if (m != null)
            {
                m.AlbedoColor = value;
            }
        }
    }
    public override void _Ready()
    {
        player.GameActions += delegate (EventData data)
        {
            if ((GodotObject)data?.Value == this)
            {
                switch (data.Type)
                {
                    case EventType.P_HURT:
                        ChangeState(State.WAITING);
                        break;
                    case EventType.P_HIT:
                        Squash();
                        break;
                }
            }
        };

    }
    public override void _PhysicsProcess(double delta)
    {
        Update((float)delta);
        // GD.Print(State.GetNameDebug[CurrentState]);
        // GD.Print(MyTimer);

    }
    private void Update(float delta)
    {
        CurrentState(this, delta);
    }
    // public string StateToString(StateFunc state)
    // {
    //     switch (CurrentState)
    //     {
    //         case StateCollection.IDLE: return "IDLE";
    //         default: return "unkown";
    //     }
    // }
    private static void Hop(Enemy mob, float JumpFreq, float delta)
    {
        mob.Position = _FlattenVec3(mob.Position);
        // mob.Mesh.Position = new(0
        // , Mathf.Lerp(mob.Mesh.Position.Y, mob.JumpHieght * 0.9f * Mathf.Abs(Mathf.Sin(JumpFreq * mob.MyTimer)), 0.1f)
        // , 0);
        mob.Mesh.Position = new(0, mob.JumpHieght * Mathf.Abs(Mathf.Sin(JumpFreq * mob.MyTimer)), 0);
        // mob.Mesh.Position.Y = mob.JumpHieght * Mathf.Abs(Mathf.Sin(JumpFreq * mob.MyTimer)
        mob.MyTimer += delta;
        mob.MyTimer %= Mathf.Pi / JumpFreq;
    }

    public static bool IsPlayerInRange(Enemy mob)
    {
        // GD.PrintT(mob.GlobalPosition, mob.player.GlobalPosition);
        // GD.PrintT(mob.GlobalPosition.DistanceSquaredTo(mob.player.GlobalPosition), mob.rangeSqrd);
        // GD.PrintT(mob.GlobalPosition.DistanceSquaredTo(mob.player.GlobalPosition) < mob.rangeSqrd);
        return mob.GlobalPosition.DistanceSquaredTo(mob.player.GlobalPosition) < mob.rangeSqrd;
    }

    public void Squash()
    {
        MyTimer = 0;
        CurrentState = State.DISABLED;
    }
    public void Revive(Vector3 newPos)
    {
        ProcessMode = ProcessModeEnum.Inherit; // remove after refactoring
        GlobalPosition = newPos;
        // Debug.Assert(CurrentState == State.DEAD);
        // CurrentState = State.IDLE;
        Visible = true;
        MeshColor = ColorAlive;
    }

    // 
    // static void Move(Enemy mob)
    // {
    //     mob.Direction = _FlattenVec3(mob.GlobalPosition.DirectionTo(mob.player.GlobalPosition)).Normalized();
    //     // Velocity
    //     // Vector3 velocity = Velocity;

    //     // GD.Print(GlobalPosition.DistanceSquaredTo(player.GlobalPosition));
    //     mob.Velocity = mob.Velocity.MoveToward(mob.Direction * mob.Speed, 0.3f * mob.Speed);
    //     // GD.Print(Direction);

    //     // Velocity = velocity;
    //     mob.MoveAndSlide();
    // }
    public interface IState
    {
        public void Update();
    }
    public static class State
    {
        public static readonly Dictionary<StateFunc, string> GetNameDebug = new()
        {
            {IDLE, "IDLE"},
            {CHASE, "CHASE"},
            {DISABLED, "DISABLED"},
            {WAITING, "WAITING"},
        };
        public static void IDLE(Enemy mob, float delta)
        {
            // GD.PrintS(mob.Position, mob.player.Position, IsPlayerInRange(mob));
            if (IsPlayerInRange(mob))
            {
                mob.ChangeState(CHASE, false);
                return;
            }
            Hop(mob, mob.JumpFreq / 2, delta);
        }
        public static void CHASE(Enemy mob, float delta)
        {
            // GD.Print("CHASE");
            if (!IsPlayerInRange(mob))
            {
                mob.ChangeState(IDLE, false);
                return;
            }
            // Move(mob);
            mob.Direction = _FlattenVec3(mob.GlobalPosition.DirectionTo(mob.player.GlobalPosition)).Normalized();
            // Velocity
            // Vector3 velocity = Velocity;

            // GD.Print(GlobalPosition.DistanceSquaredTo(player.GlobalPosition));
            mob.Velocity = mob.Velocity.MoveToward(mob.Direction * mob.Speed, 0.3f * mob.Speed);
            // GD.Print(Direction);

            // Velocity = velocity;
            Hop(mob, mob.JumpFreq, delta);
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
                        p.OnHit(mob);
                    }
                    // else // always activated by Player
                    // {
                    //     mob.Squash();
                    //     p.BroadcastAction(EventType.P_HIT, mob);
                    // }
                }
            }

            // gets killed -> set to DISABLED/DEAD
        }
        public static void WAITING(Enemy mob, float delta)
        {
            // GD.Print("WAIT");
            mob.MyTimer += delta;
            if (mob.MyTimer > 2f)
                mob.ChangeState(IDLE);
        }
        public static void DISABLED(Enemy mob, float delta)
        {
            // GD.Print("DEAD");
            if (mob.MyTimer < 0.1f)
            {
                // GD.PrintS(SkipTimer, Mesh.Position, (float)delta, (float)delta * 0.1f);

                mob.MyTimer = Mathf.Min(1, mob.MyTimer + (float)delta * 0.1f);
                mob.Mesh.Position = mob.Mesh.Position.Lerp(Vector3.Zero, mob.MyTimer);

                // Vector3 gPos = G
                if (mob.GlobalPosition.Y > 0f)
                    mob.GlobalPosition = new(mob.GlobalPosition.X
                    , Mathf.Lerp(mob.GlobalPosition.Y, 0f, mob.MyTimer)
                        , mob.GlobalPosition.Z);
                // ((StandardMaterial3D)((PrimitiveMesh)Mesh.Mesh).Material).AlbedoColor = Colors.Red;
                // MeshColor = MeshColor.Lerp(Colors.Red, SkipTimer);
                mob.MeshColor = mob.MeshColor.Lerp(mob.ColorDeath, mob.MyTimer * 2);
            }
            else
            {
                mob.Visible = false;
                mob.ProcessMode = ProcessModeEnum.Disabled;
            }
        }
    }
}
