using Godot;
using System;

public partial class Car : VehicleBody3D
{
    [Export] public float Power = 100.0f; // Vitesse de la voiture
    [Export] public float MaxSteer = 0.9f; // Vitesse de direction maximale
    [Export] public float TurnSpeed = 3.0f; // Vitesse de rotation
    [Export] public float BreakForce = 10.0f; // Force de freinage
    [Export] public float JumpForce = 300.0f; // Force de saut
    [Export] public float AirRotationSpeed = 6.0f; // Vitesse de rotation dans les airs

    private VehicleWheel3D frontLeftWheel;
    private VehicleWheel3D frontRightWheel;
    private VehicleWheel3D backLeftWheel;
    private VehicleWheel3D backRightWheel;

    private bool CanDoubleJump = false;

    public override void _Ready()
    {
        // Récupère chaque roue
        frontLeftWheel = GetNode<VehicleWheel3D>("WheelFrontLeft");
        frontRightWheel = GetNode<VehicleWheel3D>("WheelFrontRight");
        backLeftWheel = GetNode<VehicleWheel3D>("WheelBackLeft");
        backRightWheel = GetNode<VehicleWheel3D>("WheelBackRight");
    }

    public bool AreAllWheelsTouching()
    {
        // Vérifie si toutes les roues touchent une surface
        return frontLeftWheel.IsInContact() &&
               frontRightWheel.IsInContact() &&
               backLeftWheel.IsInContact() &&
               backRightWheel.IsInContact();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Gestion de la direction et de la force moteur
        Steering = Mathf.MoveToward(Steering, (Input.GetActionStrength("turn_left") - Input.GetActionStrength("turn_right")) * MaxSteer, (float)delta * 10);
        EngineForce = (Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward")) * Power;
        Brake = Input.GetActionStrength("brake") * BreakForce;

        // Contrôle dans les airs
        if (!AreAllWheelsTouching())
        {
            AirControl(delta);
        }
        else
        {
            // Réinitialise le double saut
            CanDoubleJump = true;
        }

        CheckJumps();
    }

    private void AirControl(double delta)
    {
        // Entrées utilisateur pour la rotation
        float pitchInput = Input.GetActionStrength("turn_up") - Input.GetActionStrength("turn_down");
        float rollInput = Input.GetActionStrength("roll_right") - Input.GetActionStrength("roll_left");

        // Rotation instantanée
        Vector3 angularVelocity = AngularVelocity;

        // Modifie directement la vélocité angulaire
        angularVelocity.X = pitchInput * AirRotationSpeed;
        angularVelocity.Z = rollInput * AirRotationSpeed;

        // Applique la nouvelle vélocité angulaire
        AngularVelocity = angularVelocity;
    }

    private void CheckJumps()
    {
        // Saut simple
        if (Input.IsActionJustPressed("jump") && AreAllWheelsTouching())
        {
            ApplyImpulse(Vector3.Up * JumpForce);
        } 
        else if (Input.IsActionJustPressed("jump") && CanDoubleJump )
        {
            ApplyImpulse(Vector3.Up * JumpForce);
            CanDoubleJump = false;
        }
    }
}
