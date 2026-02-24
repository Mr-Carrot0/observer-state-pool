using Godot;
using System;
using StateFunc = System.Action<Enemy, float>;

public partial class Enemy : CharacterBody3D
{
    static Vector3 _FlattenVec3(Vector3 from)
    {
        return new(from.X, 0, from.Z);
    }
    public enum State
    {
        IDLE,
        CHASING,
        DEAD,
        WAITING,
    }

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

    public State CurrentState = State.IDLE;
    // public MState CStateTMP;

    public StateFunc CStateTMP = StateCollection.IDLE;

    private void ResetParams()
    {
        MyTimer = 0;
    }

    public void ChangeState(StateFunc state, bool resetParams = true)
    {
        if (resetParams) ResetParams();
        CStateTMP = state;
    }

    Color MeshColor
    {
        get
        {
            return ((StandardMaterial3D)((PrimitiveMesh)Mesh.Mesh)?.Material)?.AlbedoColor ?? Colors.Black;
        }
        set
        {
            Color? m = ((StandardMaterial3D)((PrimitiveMesh)Mesh.Mesh)?.Material)?.AlbedoColor;
            if (m != null)
            {
                ((StandardMaterial3D)((PrimitiveMesh)Mesh.Mesh).Material).AlbedoColor = value;
            }
        }
    }
    public override void _Ready()
    {
        player.GameActions += delegate (EventData data)
        {
            switch (data.Type)
            {
                case EventType.P_HURT:
                    ChangeState(StateCollection.WAITING);
                    break;
                case EventType.P_HIT:
                    ChangeState(StateCollection.DISABLED, false);
                    break;
            }
        };

    }
    public override void _PhysicsProcess(double delta)
    {
        CStateTMP(this, (float)delta);
        GD.Print(MyTimer);

    }
    private static void Hop(Enemy mob, float JumpFreq, float delta)
    {
        mob.Position = _FlattenVec3(mob.Position);
        mob.Mesh.Position = new(0, mob.JumpHieght * Mathf.Abs(Mathf.Sin(JumpFreq * mob.MyTimer)), 0);
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
        CurrentState = State.DEAD;
    }
    public void Revive(Vector3 newPos)
    {
        ProcessMode = ProcessModeEnum.Inherit; // remove after refactoring
        GlobalPosition = newPos;
        // Debug.Assert(CurrentState == State.DEAD);
        CurrentState = State.IDLE;
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

    public static class StateCollection
    {
        public static void IDLE(Enemy mob, float delta)
        {
            GD.Print("IDLE");
            if (IsPlayerInRange(mob))
            {
                mob.ChangeState(CHASE);
                return;
            }
            Hop(mob, mob.JumpFreq / 2, delta);
        }
        public static void CHASE(Enemy mob, float delta)
        {
            GD.Print("CHASE");
            if (!IsPlayerInRange(mob))
            {
                mob.ChangeState(IDLE);
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


            // gets killed -> set to DISABLED/DEAD
        }
        public static void WAITING(Enemy mob, float delta)
        {
            GD.Print("WAIT");
            mob.MyTimer += delta;
            if (mob.MyTimer > 2f)
                mob.ChangeState(IDLE);
            // move()

            // hop(0)
        }
        public static void DISABLED(Enemy mob, float delta)
        {
            GD.Print("DEAD");
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
