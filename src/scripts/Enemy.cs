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
    const float LookRadiusSqrd = 14f;

    // [ExportGroup("Physics")]
    const float JumpHieght = 0.3f;
    const float JumpFreq = 5f;
    const float Speed = 2f;

    [ExportGroup("Nodes")]
    [Export] Player player;
    [ExportSubgroup("Internal Nodes")]
    [Export] MeshInstance3D Mesh;

    [ExportGroup("Customization")]
    [Export] Color ColorDeath = Colors.Gray;
    [Export] Color ColorAlive = Colors.DarkOrange;
    // [Export] Area3D Hurtbox;
    // ("#555")

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
        // AttackRadius = Mesh.Mesh

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
    private void Hop(float delta, float _JumpFreq = JumpFreq)
    {
        Position = _FlattenVec3(Position);
        // mob.Mesh.Position = new(0
        // , Mathf.Lerp(mob.Mesh.Position.Y, mob.JumpHieght * 0.9f * Mathf.Abs(Mathf.Sin(JumpFreq * mob.MyTimer)), 0.1f)
        // , 0);
        Mesh.Position = new(0, JumpHieght * Mathf.Abs(Mathf.Sin(_JumpFreq * MyTimer)), 0);
        // mob.Mesh.Position.Y = mob.JumpHieght * Mathf.Abs(Mathf.Sin(JumpFreq * mob.MyTimer)
        MyTimer += delta;
        MyTimer %= Mathf.Pi / _JumpFreq;
    }

    public bool IsPlayerInRange()
    {
        // float _rangeSqrd = attack ? AttackRadiusSqrd : LookRadiusSqrd;
        // GD.PrintT(mob.GlobalPosition, mob.player.GlobalPosition);
        // GD.PrintT(mob.GlobalPosition.DistanceSquaredTo(mob.player.GlobalPosition), mob.rangeSqrd);
        // GD.PrintT(mob.GlobalPosition.DistanceSquaredTo(mob.player.GlobalPosition) < mob.rangeSqrd);
        return GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < LookRadiusSqrd;
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
            if (!mob.IsPlayerInRange())
            {
                mob.ChangeState(IDLE, false);
                return;
            }
            // if (mob.IsPlayerInRange(true))
            // {
            //     mob.player.OnHit();
            // }
            // Move(mob);
            mob.Direction = _FlattenVec3(mob.GlobalPosition.DirectionTo(mob.player.GlobalPosition)).Normalized();
            // Velocity
            // Vector3 velocity = Velocity;

            // GD.Print(GlobalPosition.DistanceSquaredTo(player.GlobalPosition));
            mob.Velocity = mob.Velocity.MoveToward(mob.Direction * Speed, 0.3f * Speed);
            // GD.Print(Direction);

            // Velocity = velocity;
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
                        p.OnPlayerDamage(mob);
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
            if (mob.MyTimer > 2f)
                mob.ChangeState(IDLE);
        }
        public static void DISABLED(Enemy mob, float delta)
        {
            if (mob.MyTimer < 0.1f)
            {

                mob.MyTimer = Mathf.Min(1, mob.MyTimer + (float)delta * 0.1f);
                mob.Mesh.Position = mob.Mesh.Position.Lerp(Vector3.Zero, mob.MyTimer);

                // Vector3 gPos = G
                if (mob.GlobalPosition.Y > 0f)
                    mob.GlobalPosition = new(mob.GlobalPosition.X
                    , Mathf.Lerp(mob.GlobalPosition.Y, 0f, mob.MyTimer)
                        , mob.GlobalPosition.Z);
                // ((StandardMaterial3D)((PrimitiveMesh)Mesh.Mesh).Material).AlbedoColor = Colors.Red;
                mob.MeshColor = mob.MeshColor.Lerp(mob.ColorDeath, mob.MyTimer * 2);
            }
            else
            {
                mob.Visible = false;
                mob.ProcessMode = ProcessModeEnum.Disabled;
            }
        }
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
    // public interface IState
    // {
    //     public void Update();
    // }
}
