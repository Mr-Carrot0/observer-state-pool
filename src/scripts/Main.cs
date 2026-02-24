using Godot;
using Microsoft.VisualBasic.FileIO;
using System;

public partial class Main : Node3D
{
    // Enemy[] Enemies = new Enemy[5];
    // [Export] Node3D EnemyContainerNode;
    public override void _Ready()
    {
        GD.Randomize();

        // for (int i = 0; i < 5; i++)
        // {
        //     Enemies[i] = GD.Load<PackedScene>("res://src/prefabs/enemy.tscn").Instantiate<Enemy>();
        //     EnemyContainerNode.AddChild(Enemies[i]);
        //     Enemies[i].Position = Vector3.Up + 6*Vector3.Right.Rotated(Vector3.Up, Mathf.Tau * i / 5);
        // }
    }

    // public override void _Process(double delta)
    // {
    // }
}
