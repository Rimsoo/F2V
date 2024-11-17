using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float CameraSensitivity = 0.1f; // Sensibilité de la caméra
    [Export] public float CameraResetSpeed = 10.0f; // Vitesse de retour de la caméra à la position de base
    [Export] public float maxCameraAngle = 80.0f; // Angle maximal de la caméra
    [Export] public float camHeihgtY = 3.0f; // Hauteur de la caméra
    [Export] public float camDistanceZ = 10.0f; // Distance de la caméra
    [Export] public float camRotationX = 15.0f; // Distance de la caméra

    private Node3D camera; // Référence à la caméra
    private Vector3 cameraBaseRotation; // Rotation de base de la caméra
    private VehicleBody3D car;

    public override void _Ready()
    {
        // Récupère la caméra enfant
        camera = GetNode<Node3D>("Camera");
        car = GetNode<VehicleBody3D>("Car");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Position de la caméra par rapport à la voiture toujours à la même distance
        camera.Position = new Vector3(car.Position.X, car.Position.Y + camHeihgtY, car.Position.Z + camDistanceZ);

        // Contrôle de la caméra avec le joystick droit
        float cameraHorizontal = Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left");

        // Rotation de la caméra avec limitation de l'angle
        float newCameraRotation = camera.Rotation.Y - cameraHorizontal * CameraSensitivity;

        // Limiter l'angle vertical de la caméra
        newCameraRotation = Mathf.Clamp(newCameraRotation, -Mathf.DegToRad(maxCameraAngle), Mathf.DegToRad(maxCameraAngle));

        camera.Rotation = new Vector3(camera.Rotation.X, newCameraRotation, camera.Rotation.Z);

        // Si aucun mouvement n'est détecté, retour progressif de la caméra à sa position de base
        if (cameraHorizontal == 0)
        {
            camera.Rotation = new Vector3(
                Mathf.Lerp(camera.Rotation.X, cameraBaseRotation.X, CameraResetSpeed * (float)delta),
                Mathf.Lerp(camera.Rotation.Y, cameraBaseRotation.Y, CameraResetSpeed * (float)delta),
                Mathf.Lerp(camera.Rotation.Z, cameraBaseRotation.Z, CameraResetSpeed * (float)delta)
            );
        }

    }
}
