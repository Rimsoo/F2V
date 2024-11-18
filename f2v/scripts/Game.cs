using Godot;
using System;

public partial class Game : Node3D
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float resetBall = Input.GetActionStrength("reset_ball");
		if(resetBall > 0.0f)
		{
			// Reset the ball
			ResetBall();
			ResetPlayer();
		}
	}

    private void ResetBall()
    {
        // Reset the ball position
		RigidBody3D Ball = GetNode<Node3D>("Ball").GetNode<RigidBody3D>("dirty_football_LOD0");
		Ball.Position = new Vector3(0, 2, 0);
		Ball.Rotation = new Vector3(0, 0, 0);
		Ball.LinearVelocity = new Vector3(0, 0, 0);
    }
	
	private void ResetPlayer()
	{
		// Reset the Car position
		VehicleBody3D Player = GetNode<Node3D>("Player").GetNode<VehicleBody3D>("Car");
		Player.Position = new Vector3(0, 2, 10);
		Player.Rotation = new Vector3(0, 0, 0);
		Player.LinearVelocity = new Vector3(0, 0, 0);
	}
}
