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

    private Node3D Camera; // Référence à la caméra
    private Vector3 CameraBaseRotation; // Rotation de base de la caméra
    private VehicleBody3D Car;

    public override void _Ready()
    {
        // Récupère la caméra enfant
        Camera = GetNode<Node3D>("Camera");
        Car = GetNode<VehicleBody3D>("Car");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Position de la caméra par rapport à la voiture toujours à la même distance
        Camera.Position = new Vector3(Car.Position.X, Car.Position.Y + CamHeihgtY, Car.Position.Z + CamDistanceZ);

        // Contrôle de la caméra avec le joystick droit
        float cameraHorizontal = Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left");

        // Rotation de la caméra avec limitation de l'angle
        float newCameraRotation = Camera.Rotation.Y - cameraHorizontal * CameraSensitivity;

        // Limiter l'angle vertical de la caméra
        newCameraRotation = Mathf.Clamp(newCameraRotation, -Mathf.DegToRad(MaxCameraAngle), Mathf.DegToRad(MaxCameraAngle));

        Camera.Rotation = new Vector3(Camera.Rotation.X, newCameraRotation, Camera.Rotation.Z);

        // Si aucun mouvement n'est détecté, retour progressif de la caméra à sa position de base
        if (cameraHorizontal == 0)
        {
            Camera.Rotation = new Vector3(
                Mathf.Lerp(Camera.Rotation.X, CameraBaseRotation.X, CameraResetSpeed * (float)delta),
                Mathf.Lerp(Camera.Rotation.Y, CameraBaseRotation.Y, CameraResetSpeed * (float)delta),
                Mathf.Lerp(Camera.Rotation.Z, CameraBaseRotation.Z, CameraResetSpeed * (float)delta)
            );
        }

    }
}
