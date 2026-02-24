using Godot;
using System;

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

    float Timer = 0;
    [Export] float JumpHieght = 0.3f;
    [Export] float JumpFreq = 5f;
    [Export] float rangeSqrd = 14f;
    [Export] float Speed = 2f;
    [Export] MeshInstance3D Mesh;
    [Export] Player player;
    public Vector3 Direction = Vector3.Zero;
    public State CurrentState = State.IDLE;
    [Export] Color ColorAlive = Colors.DarkOrange;
    // public IState CStateTMP;
    [Export] Color ColorDeath = Colors.Red;

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
                m = value;
            }
        }
    }
    public override void _Ready()
    {
        // player.GameActions += 
    }

    static public bool IsPlayerInRange(Enemy mob)
    {
        return mob.GlobalPosition.DistanceSquaredTo(mob.player.GlobalPosition) < mob.rangeSqrd;
    }

    public void Squash()
    {
        Timer = 0;
        CurrentState = State.DEAD;
    }
    public void ReviveAt(Vector3 newPos)
    {
        ProcessMode = ProcessModeEnum.Inherit; // remove after refactoring
        GlobalPosition = newPos;
        // Debug.Assert(CurrentState == State.DEAD);
        CurrentState = State.IDLE;
        Visible = true;
        MeshColor = ColorAlive;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CurrentState != State.DEAD)
        {
            // GD.Print(CurrentState);
            DebugDraw3D.DrawSphere(GlobalPosition, Mathf.Sqrt(rangeSqrd));
            bool playerInRange = GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < rangeSqrd;
            switch (CurrentState)
            {
                case State.IDLE:
                    if (playerInRange)
                    {
                        CurrentState = State.CHASING;
                        break;
                    }
                    break;

                case State.CHASING:
                    if (!playerInRange)
                    {
                        CurrentState = State.IDLE;
                        break;
                    }
                    Direction = _FlattenVec3(GlobalPosition.DirectionTo(player.GlobalPosition)).Normalized();
                    // Velocity
                    // Vector3 velocity = Velocity;

                    // GD.Print(GlobalPosition.DistanceSquaredTo(player.GlobalPosition));
                    Velocity = Velocity.MoveToward(Direction * Speed, 0.3f * Speed);
                    // GD.Print(Direction);

                    // Velocity = velocity;
                    MoveAndSlide();
                    break;
            }

            Position = new(Position.X, 0, Position.Z);
            Mesh.Position = new(0, JumpHieght * Mathf.Abs(Mathf.Sin(JumpFreq * Timer)), 0);
            Timer += (float)delta;
            Timer %= Mathf.Pi / JumpFreq;
            // GD.Print(SkipTimer);
        }
        else
        {
            // reuses SkipTimer 
            if (Timer < 0.1f)
            {
                // GD.PrintS(SkipTimer, Mesh.Position, (float)delta, (float)delta * 0.1f);

                Timer = Mathf.Min(1, Timer + (float)delta * 0.1f);
                Mesh.Position = Mesh.Position.Lerp(Vector3.Zero, Timer);

                // Vector3 gPos = G
                if (GlobalPosition.Y > 0f)
                    GlobalPosition = new(GlobalPosition.X
                    , Mathf.Lerp(GlobalPosition.Y, 0f, Timer)
                        , GlobalPosition.Z);
                // ((StandardMaterial3D)((PrimitiveMesh)Mesh.Mesh).Material).AlbedoColor = Colors.Red;
                // MeshColor = MeshColor.Lerp(Colors.Red, SkipTimer);
                MeshColor = MeshColor.Lerp(Colors.Red, Timer * 2);
            }
            else
            {
                Visible = false;
                ProcessMode = ProcessModeEnum.Disabled;
            }
        }
    }


    // public interface IState
    // {
    //     public void Update(float delta);
    // }
    // class IdleState : IState
    // {
    //     public void Update(float delta)
    //     {
    //         // if (IsPlayerInRange(mob))
    //         // {
    //         //     mob.CStateTMP = ChaseState;
    //         // }
    //     }
    // }

    // static void ChaseState(Enemy mob)
    // {

    // }
    // static void DState(Enemy mob)
    // {

    // }
}
