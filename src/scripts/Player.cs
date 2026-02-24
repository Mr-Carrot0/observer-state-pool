using Godot;
using System;


public partial class Player : CharacterBody3D
{
    public delegate void GameAction(EventData data);
    public const float MAX_SPEED = 5.0f;
    public const float JUMP_VELOCITY = 4.5f;
    public const float MAX_HEALTH = 5;
    public float Health = MAX_HEALTH;
    // [Export] public ProgressBar HealthBar;
    [Export] private Camera3D Cam;
    // Enemy _ref;
    // float _Timer = 0;

    public event GameAction GameActions;
    public void BroadcastAction(EventData data)
    {
        GameActions?.Invoke(data);
    }
    public void BroadcastAction(EventType _type, Variant? value = null)
    {
        GameActions?.Invoke(new EventData(_type, value));
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (IsOnFloor() && Input.IsActionJustPressed("jump"))
        {
            velocity.Y = JUMP_VELOCITY;
        }
        else
        {
            velocity += GetGravity() * (float)delta;
        }

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        Vector2 inputDirJoy = new(Input.GetJoyAxis(0, JoyAxis.LeftX), Input.GetJoyAxis(0, JoyAxis.LeftY));
        // GD.PrintS(inputDirJoy, inputDirJoy.LengthSquared());
        if (inputDirJoy.LengthSquared() < 0.003f)
        {
            inputDirJoy = Vector2.Zero;
        }
        if (inputDir == Vector2.Zero) inputDir = inputDirJoy;

        Vector3 direction = (Cam.Basis * Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = Mathf.MoveToward(velocity.X, direction.X * MAX_SPEED, MAX_SPEED * 3 * (float)delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * MAX_SPEED, MAX_SPEED * 3 * (float)delta);
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, MAX_SPEED * 8 * (float)delta);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, MAX_SPEED * 8 * (float)delta);
        }

        Velocity = velocity;
        // MoveAndSlide();

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);

            if (collision.GetCollider() is Enemy mob)
            {
                if (mob.CurrentState == Enemy.State.DEAD)
                    continue;

                if (Vector3.Up.Dot(collision.GetNormal()) > 0.1f)
                {
                    // _ref = mob;
                    BroadcastAction(EventType.P_HIT);
                    // mob.Squash();
                    GD.Print("squash");
                }
                else
                {
                    Health--;
                    BroadcastAction(EventType.P_HURT);
                    // GD.PrintS("hit", Health);

                    // Velocity += (mob.Position - Position).Normalized() * 10 + new Vector3(0, 3, 0);
                }

                break;
            }
        }
        MoveAndSlide();


        // if (_ref != null)
        // {
        //     _Timer += (float)delta;
        //     if (_Timer > 2f)
        //     {
        //         // float r = GD.Randf();
        //         // GD.PrintS(r, r * Mathf.Tau, Vector3.Right.Rotated(Vector3.Up, r * Mathf.Tau));
        //         _ref.Revive(GlobalPosition + 5 * Vector3.Right.Rotated(Vector3.Up, GD.Randf() * Mathf.Tau));
        //         _Timer = 0;
        //         _ref = null;
        //     }
        // }
    }
}
