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
		}
	}

    private void ResetBall()
    {
        // Reset the ball position
		RigidBody3D ball = GetNode<Node3D>("ball").GetNode<RigidBody3D>("dirty_football_LOD0");
		ball.Position = new Vector3(0, -10, 0);
		ball.Rotation = new Vector3(0, 0, 0);
		ball.LinearVelocity = new Vector3(0, 0, 0);
    }

}
