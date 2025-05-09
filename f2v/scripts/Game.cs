using Godot;
using System;

public partial class Game : Node3D
{
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Menu Menu = GetNode<Menu>("Menu");
        if (Menu.Visible)
        {
            return;
        }

        if (Input.IsActionJustPressed("game_menu"))
        {
            Menu.Visible = !Menu.Visible;
            Menu.InitButtons();

        }

        float resetBall = Input.GetActionStrength("reset_ball");
        if (resetBall > 0.0f)
        {
            // Reset the ball
            ResetBall();
            ResetPlayer();
        }
    }

    private void ResetBall()
    {
        // Reset the ball position
        RigidBody3D Ball = GetNode<RigidBody3D>("Ball");
        Ball.GlobalPosition = new Vector3(0, 2, 0);
        Ball.GlobalRotation = new Vector3(0, 0, 0);
        Ball.LinearVelocity = new Vector3(0, 0, 0);
    }

    private void ResetPlayer()
    {
        // Reset the Car position
        VehicleBody3D Player = GetNode<Node3D>("Player").GetNode<VehicleBody3D>("Car");
        Player.GlobalPosition = new Vector3(0, 2, 10);
        Player.GlobalRotation = new Vector3(0, 0, 0);
        Player.LinearVelocity = new Vector3(0, 0, 0);
    }
}
