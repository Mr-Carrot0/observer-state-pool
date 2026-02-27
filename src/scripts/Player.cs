using Godot;
using System;

public partial class Player : CharacterBody3D, IEventHandler
{
    public bool Dead = false;
    public const float MAX_SPEED = 5.0f;
    public const float JUMP_VELOCITY = 4.5f;
    public const float MAX_HEALTH = 3;
    public float Health = MAX_HEALTH;
    [Export] private Camera3D Cam;
    Vector3 Knockback = Vector3.Zero;

    public event GameEvent Event;

    public void BroadcastAction(EventData data)
    {
        Event?.Invoke(data);
    }
    public void BroadcastAction(EventType _type, Enemy mob = null)
    {
        Event?.Invoke(new EventData(_type, mob));
        // GameActions(data:new(_type));
    }

    public void OnPlayerHurt(Enemy attacker = null, float dmg = 1)
    {
        if (Dead) return;

        Health -= dmg;
        // GD.Print(Health);
        if (Health <= 0)
        {
            BroadcastAction(EventType.DEATH);
            Dead = true;
        }
        else
        {
            BroadcastAction(EventType.HURT, attacker);
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        if (Dead)
        {
            if (GlobalPosition.Y < 500)
            {
                Position += 2 * Vector3.Up * (float)delta;
            }
        }
        else
        {
            Vector3 velocity = Velocity;
            // Vector3 velocity = Velocity;

            if (IsOnFloor() && Input.IsActionJustPressed("jump"))
            {
                velocity.Y = JUMP_VELOCITY;
                BroadcastAction(EventType.JUMP);
            }
            else
            {
                velocity += GetGravity() * (float)delta;
            }

            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            Vector2 inputDirJoy = new(Input.GetJoyAxis(0, JoyAxis.LeftX), Input.GetJoyAxis(0, JoyAxis.LeftY));
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
            Knockback = Knockback.Lerp(Vector3.Zero, 0.02f);

            MoveAndSlide();
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                KinematicCollision3D collision = GetSlideCollision(i);

                if (collision.GetCollider() is Enemy mob)
                {
                    if (mob.CurrentState == Enemy.State.DISABLED)
                        continue;

                    if (Vector3.Up.Dot(collision.GetNormal()) > 0.1f)
                    {
                        // _ref = mob;
                        BroadcastAction(EventType.HIT, mob);
                        // GD.Print("squash");
                    }
                    else // always activated by Enemy
                    {
                        OnPlayerHurt(mob);
                    }

                    break;
                }
            }
        }
    }

}