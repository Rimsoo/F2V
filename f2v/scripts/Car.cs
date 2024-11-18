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
    [Export] public float DriftFactor = 0.3f; // Facteur de drift
    [Export] public float DriftTurnSpeed = 1.5f; // Multiplicateur de vitesse de direction pendant le drift
    [Export] public float WheelFrictionDefault = 10.5f; // Multiplicateur de puissance pendant le drift

    private VehicleWheel3D FrontLeftWheel;
    private VehicleWheel3D FrontRightWheel;
    private VehicleWheel3D backLeftWheel;
    private VehicleWheel3D BackRightWheel;

    private bool CanDoubleJump = false;

    public override void _Ready()
    {
        // Récupère chaque roue
        FrontLeftWheel = GetNode<VehicleWheel3D>("WheelFrontLeft");
        FrontRightWheel = GetNode<VehicleWheel3D>("WheelFrontRight");
        backLeftWheel = GetNode<VehicleWheel3D>("WheelBackLeft");
        BackRightWheel = GetNode<VehicleWheel3D>("WheelBackRight");
    }

    public bool AreAllWheelsTouching()
    {
        // Vérifie si toutes les roues touchent une surface
        return FrontLeftWheel.IsInContact() &&
               FrontRightWheel.IsInContact() &&
               backLeftWheel.IsInContact() &&
               BackRightWheel.IsInContact();
    }

    public bool AreAllWheeNotTouching()
    {
        // Vérifie si toutes les roues touchent une surface
        return ! FrontLeftWheel.IsInContact() &&
               ! FrontRightWheel.IsInContact() &&
               ! backLeftWheel.IsInContact() &&
               ! BackRightWheel.IsInContact();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Gestion de la direction et de la force moteur
        Steering = Mathf.MoveToward(Steering, (Input.GetActionStrength("turn_left") - Input.GetActionStrength("turn_right")) * MaxSteer, (float)delta * 10);
        EngineForce = (Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward")) * Power;

        // Si on est en train de freiner
        bool IsBraking = Input.IsActionPressed("brake");
        if(IsBraking)
        {
            Brake = BreakForce;
            backLeftWheel.WheelFrictionSlip = DriftFactor;
            BackRightWheel.WheelFrictionSlip = DriftFactor;
        }
        else
        {
            Brake = 0;
            backLeftWheel.WheelFrictionSlip = WheelFrictionDefault;
            BackRightWheel.WheelFrictionSlip = WheelFrictionDefault;
        }
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
        float yawInput = Input.GetActionStrength("turn_left") - Input.GetActionStrength("turn_right");

        if(pitchInput != 0 || rollInput != 0 || yawInput != 0)
        {
            //AngularVelocity = Vector3.Zero;
        }
        RotateObjectLocal(Vector3.Forward, rollInput * AirRotationSpeed * (float)delta);
        RotateObjectLocal(Vector3.ModelRight, pitchInput * AirRotationSpeed * (float)delta);
        if(AreAllWheeNotTouching())
            RotateObjectLocal(Vector3.Up, yawInput * AirRotationSpeed * (float)delta);
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
