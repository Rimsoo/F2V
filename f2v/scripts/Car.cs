using Godot;
using System;

public partial class Car : VehicleBody3D
{
    [Export] public float Power = 100.0f; // Vitesse de la voiture
    [Export] public float MaxSteer = 0.9f; // Vitesse de direction maximale
    [Export] public float TurnSpeed = 3.0f; // Vitesse de rotation
    [Export] public float BreakForce = 10.0f; // Force de freinage
    [Export] public float JumpForce = 300.0f; // Force de saut
    [Export] public float BoostForce = 50.0f; // Force de saut
    [Export] public float FlipForce = 0.1f;
    [Export] public float AirRotationSpeed = 6.0f; // Vitesse de rotation dans les airs
    [Export] public float BackDriftFactor = 0.3f; // Facteur de drift
    [Export] public float FrontDriftFactor = 2.0f; // Facteur de drift
    [Export] public float DriftTurnSpeed = 1.5f; // Multiplicateur de vitesse de direction pendant le drift
    [Export] public float WheelFrictionDefault = 10.5f; // Multiplicateur de puissance pendant le drift

    private VehicleWheel3D FrontLeftWheel;
    private VehicleWheel3D FrontRightWheel;
    private VehicleWheel3D BackLeftWheel;
    private VehicleWheel3D BackRightWheel;

    private float PitchInput = 0.0f;
    private float RollInput = 0.0f;
    private float YawInput = 0.0f;

    private bool CanDoubleJump = false;

    private Vector3 BaseRotation;

    public override void _Ready()
    {
        // Récupère chaque roue
        FrontLeftWheel = GetNode<VehicleWheel3D>("WheelFrontLeft");
        FrontRightWheel = GetNode<VehicleWheel3D>("WheelFrontRight");
        BackLeftWheel = GetNode<VehicleWheel3D>("WheelBackLeft");
        BackRightWheel = GetNode<VehicleWheel3D>("WheelBackRight");

        BaseRotation = RotationDegrees;
    }

    public bool AreAllWheelsTouching()
    {
        // Vérifie si toutes les roues touchent une surface
        return FrontLeftWheel.IsInContact() &&
               FrontRightWheel.IsInContact() &&
               BackLeftWheel.IsInContact() &&
               BackRightWheel.IsInContact();
    }

    public bool AreAllWheelsNotTouching()
    {
        // Vérifie si toutes les roues touchent une surface
        return !FrontLeftWheel.IsInContact() &&
               !FrontRightWheel.IsInContact() &&
               !BackLeftWheel.IsInContact() &&
               !BackRightWheel.IsInContact();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GetTree().GetNodesInGroup("Menu")[0] is Control Menu && Menu.Visible)
        {
            return;
        }

        // Entrées utilisateur pour la rotation
        PitchInput = Input.GetActionStrength("turn_up") - Input.GetActionStrength("turn_down");
        RollInput = Input.GetActionStrength("roll_left") - Input.GetActionStrength("roll_right");
        YawInput = Input.GetActionStrength("turn_left") - Input.GetActionStrength("turn_right");
        // Gestion de la direction et de la force moteur
        Steering = Mathf.MoveToward(Steering, YawInput * MaxSteer, (float)delta * 10);
        EngineForce = (Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward")) * Power;

        // Si on est en train de freiner
        CheckBreak(delta);
        // Contrôle dans les airs
        if (!AreAllWheelsTouching())
        {
            AirControl(delta);
            ApplyCentralForce(Vector3.Down * 5.0f); // Ajuste la valeur selon tes besoins
        }
        else
        {
            // Réinitialise le double saut
            CanDoubleJump = true;
        }

        CheckJumps();

        CheckBoost();
    }

    private void CheckBoost()
    {
        if (Input.IsActionPressed("boost"))
        {
            // 1. Utiliser la direction avant LOCALE (toujours -Z dans l'espace local de la voiture)
            Vector3 localDirection = Vector3.Forward; // Équivaut à new Vector3(0, 0, -1)

            // 2. Convertir en direction globale
            Vector3 globalDirection = GlobalTransform.Basis * localDirection;

            // 3. Normaliser et appliquer
            ApplyImpulse(globalDirection.Normalized() * BoostForce);
        }
    }

    private void CheckBreak(double delta)
    {
        bool IsBraking = Input.IsActionPressed("brake");
        if (IsBraking)
        {
            Brake = BreakForce;
            BackLeftWheel.WheelFrictionSlip = BackDriftFactor;
            BackRightWheel.WheelFrictionSlip = BackDriftFactor;
            FrontLeftWheel.WheelFrictionSlip = FrontDriftFactor;
            FrontRightWheel.WheelFrictionSlip = FrontDriftFactor;
        }
        else
        {
            Brake = 0;
            BackLeftWheel.WheelFrictionSlip = WheelFrictionDefault;
            BackRightWheel.WheelFrictionSlip = WheelFrictionDefault;
            FrontLeftWheel.WheelFrictionSlip = WheelFrictionDefault;
            FrontRightWheel.WheelFrictionSlip = WheelFrictionDefault;
        }
    }

    private void AirControl(double delta)
    {
        float deltaTime = (float)delta;
        // AngularVelocity *= 1.97f;

        if (Mathf.Abs(PitchInput) > 0.1f || Mathf.Abs(RollInput) > 0.1f || Mathf.Abs(YawInput) > 0.1f)
        {
            var yawInput = AreAllWheelsNotTouching() ? YawInput * AirRotationSpeed * deltaTime : 0;
            var pitchInput = AreAllWheelsNotTouching() ? PitchInput * AirRotationSpeed * deltaTime : 0;
            // Calculer le changement souhaité en vitesse angulaire (en espace local)
            Vector3 localDelta = new Vector3(
                pitchInput, // Pitch (X)
                yawInput,  // Yaw (Y)
                RollInput * AirRotationSpeed * deltaTime  // Roll (Z)
            );

            // Convertir le delta local en espace global
            Vector3 globalDelta = Basis * localDelta;

            // Appliquer le changement à la vélocité angulaire
            AngularVelocity += globalDelta;
        }
    }

    // private void AirControl(double delta)
    // {
    //     RotateObjectLocal(Vector3.Forward, RollInput * AirRotationSpeed * (float)delta);
    //     RotateObjectLocal(Vector3.ModelRight, PitchInput * AirRotationSpeed * (float)delta);
    //     if (AreAllWheelsNotTouching())
    //     {
    //         RotateObjectLocal(Vector3.Up, YawInput * AirRotationSpeed * (float)delta);
    //     }
    // }

    private void CheckJumps()
    {
        // Saut simple
        if (Input.IsActionJustPressed("jump") && AreAllWheelsTouching())
        {
            ApplyImpulse(Vector3.Up * JumpForce);
        }
        else if (Input.IsActionJustPressed("jump") && CanDoubleJump)
        {
            Vector3 direction = new(RollInput + YawInput, AngularVelocity.Y, PitchInput);
            // Transformation en global en utilisant la base de la voiture
            Vector3 globalDirection = ToGlobal(direction);

            // Appliquer l'impulsion en fonction de la direction calculée
            if (YawInput != 0 || PitchInput != 0 || RollInput != 0)
                ApplyImpulse(globalDirection.Normalized() * JumpForce);
            else
                ApplyImpulse(Vector3.Up * JumpForce);
            CanDoubleJump = false;
        }
    }
}
