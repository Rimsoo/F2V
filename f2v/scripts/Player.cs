using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float CameraSensitivity = 0.1f; // Sensibilité de la caméra
    [Export] public float CameraResetSpeed = 10.0f; // Vitesse de retour de la caméra à la position de base
    [Export] public float MaxCameraAngle = 80.0f; // Angle maximal de la caméra
    [Export] public float CamHeihgtY = 3.0f; // Hauteur de la caméra
    [Export] public float CamDistanceZ = 10.0f; // Distance de la caméra
    [Export] public float CamRotationX = 15.0f; // Distance de la caméra

    private Vector3 CameraBaseRotation; // Rotation de base de la caméra
    private VehicleBody3D Car;
    private Node3D TwistPivot;

    public override void _Ready()
    {
        // Récupère la caméra enfant
        Car = GetNode<VehicleBody3D>("Car");

        // Récupère le pivot de rotation
        TwistPivot = GetNode<Node3D>("Car/TwistPivot");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Contrôle de la caméra avec le joystick droit
        float cameraHorizontal = Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left");
        // Le twist pivo va suivre parfaitement la voiture
        TwistPivot.Rotation = new Vector3(TwistPivot.Rotation.X, Car.Rotation.Y, TwistPivot.Rotation.Z);
        TwistPivot.Position = new Vector3(Car.Position.X, Car.Position.Y + CamHeihgtY, Car.Position.Z + CamDistanceZ);

        if(cameraHorizontal != 0) 
        {
            // Rotation de la caméra avec limitation de l'angle
            float newCameraRotation = TwistPivot.Rotation.Y - cameraHorizontal * CameraSensitivity;
            newCameraRotation = Mathf.Clamp(newCameraRotation, -Mathf.DegToRad(MaxCameraAngle), Mathf.DegToRad(MaxCameraAngle));
            TwistPivot.Rotation = new Vector3(TwistPivot.Rotation.X, newCameraRotation, TwistPivot.Rotation.Z);
        }

        // Si aucun mouvement n'est détecté, retour progressif de la caméra à sa position de base
        if (cameraHorizontal == 0)
        {
            TwistPivot.Rotation = new Vector3(
                TwistPivot.Rotation.X,
                Mathf.Lerp(TwistPivot.Rotation.Y, Car.Rotation.Y, CameraResetSpeed * (float)delta),
                TwistPivot.Rotation.Z
            );
        }

    }
}
