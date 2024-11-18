using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float CameraSensitivity = 0.1f; // Sensibilité de la caméra
    [Export] public float CameraResetSpeed = 10.0f; // Vitesse de retour de la caméra à la position de base
    [Export] public float MaxCameraAngle = 80.0f; // Angle maximal de la caméra
    [Export] public float CamHeihgtY = 3.0f; // Hauteur de la caméra
    [Export] public float CamDistanceZ = 10.0f; // Distance de la caméra
    [Export] public float CamRotationX = -20.0f; // Distance de la caméra
    [Export] public bool IsBallCamLock = false; // Verrouillage de la caméra sur la balle

    private Vector3 CameraBaseRotation; // Rotation de base de la caméra
    private VehicleBody3D Car;
    private Node3D CarPivot;
    private Node3D Ball;
    private Camera3D Camera;

    public override void _Ready()
    {
        // Récupère la caméra enfant
        Car = GetNode<VehicleBody3D>("Car");

        // Récupère le pivot de rotation
        CarPivot = GetNode<Node3D>("Car/CarPivot");
        Camera = CarPivot.GetNode<Camera3D>("Camera");
        Ball = GetTree().GetNodesInGroup("Ball")[0] as Node3D;
    }

    public override void _PhysicsProcess(double delta)
    {
        IsBallCamLock = Input.IsActionJustPressed("camera_lock") ? !IsBallCamLock : IsBallCamLock;
        // Le twist pivo va suivre parfaitement la voiture
        CarPivot.Rotation = new Vector3(CarPivot.Rotation.X, Car.Rotation.Y, CarPivot.Rotation.Z);
        CarPivot.Position = new Vector3(Car.Position.X, Car.Position.Y + CamHeihgtY, Car.Position.Z + CamDistanceZ);

        if(IsBallCamLock)
        {
            CarPivot.LookAt(Ball.GlobalTransform.Origin - new Vector3(0, CamHeihgtY, 0));
            Camera.LookAt(Ball.GlobalTransform.Origin -  CarPivot.GlobalTransform.Origin);
        }
        else
        {
            
            Camera.Rotation = new Vector3(Mathf.DegToRad(CamRotationX), 0, 0);
        }

        // Contrôle de la caméra avec le joystick droit
        float cameraHorizontal = Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left");

        if(cameraHorizontal != 0) 
        {
            // Rotation de la caméra avec limitation de l'angle
            float newCameraRotation = CarPivot.Rotation.Y - cameraHorizontal * CameraSensitivity;
            newCameraRotation = Mathf.Clamp(newCameraRotation, -Mathf.DegToRad(MaxCameraAngle), Mathf.DegToRad(MaxCameraAngle));
            CarPivot.Rotation = new Vector3(CarPivot.Rotation.X, newCameraRotation, CarPivot.Rotation.Z);
        } 
        else
        {
            CarPivot.Rotation = new Vector3(
                CarPivot.Rotation.X,
                Mathf.Lerp(CarPivot.Rotation.Y, Car.Rotation.Y, CameraResetSpeed * (float)delta),
                CarPivot.Rotation.Z
            );
        }
    }
}
