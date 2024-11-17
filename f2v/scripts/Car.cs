using Godot;
using System;

public partial class Car : VehicleBody3D
{
    [Export] public float Power = 100.0f; // Vitesse de la voiture
    [Export] public float MaxSteer = 0.9f; // Vitesse de la voiture
    [Export] public float TurnSpeed = 3.0f; // Vitesse de rotation
    [Export] public float JumpVelocity = 10.0f;
    [Export] public float BreakForce = 10.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Steering = Mathf.MoveToward(Steering, (Input.GetActionStrength("turn_left") - Input.GetActionStrength("turn_right")) * MaxSteer, (float)delta * 10);
		EngineForce = (Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward")) * Power;
		Brake = Input.GetActionStrength("brake") * BreakForce;
		GD.Print(Brake);

        // Gestion du saut
		// if (Input.IsActionJustPressed("jump") && IsOnFloor())
        // {
        //     velocity.Y = JumpVelocity;
        // }
		
        // // Accélération et marche arrière avec `RT` et `LT`
        // float forwardInput = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward");
		
        // // Rotation de la voiture avec le joystick gauche, inversée
        // float leftRightTurnInput = Input.GetActionStrength("turn_right") - Input.GetActionStrength("turn_left");
        // float upDownTurnInput = 0;
        // float rollInput = 0;
        // if (!IsOnFloor())
        // {
        //     upDownTurnInput = Input.GetActionStrength("turn_up") - Input.GetActionStrength("turn_down"); 
        //     rollInput = Input.GetActionStrength("roll_right") - Input.GetActionStrength("roll_left");
        // }

        // // Rotation de la voiture avec le joystick gauche, inversée
        // float leftRightTurnInput = Input.GetActionStrength("turn_right") - Input.GetActionStrength("turn_left");
        // float upDownTurnInput = 0;
        // float rollInput = 0;
        // if (!IsOnFloor())
        // {
        //     upDownTurnInput = Input.GetActionStrength("turn_up") - Input.GetActionStrength("turn_down"); 
        //     rollInput = Input.GetActionStrength("roll_right") - Input.GetActionStrength("roll_left");
        // }
        // Rotation = new Vector3(Rotation.X - upDownTurnInput * TurnSpeed * (float)delta, Rotation.Y - leftRightTurnInput * TurnSpeed * (float)delta, Rotation.Z - rollInput * TurnSpeed * (float)delta);
        // // Mise à jour de la vélocité en Y et déplacement
		// Velocity = velocity;
        // MoveAndSlide();
	}
}
