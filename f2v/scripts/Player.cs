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
	private Node3D Car;
	private Node3D CarPivot;
	private Node3D Ball;
	private Camera3D Camera;
	private float CameraAngle = 0.0f;


	public override void _Ready()
	{
		// Récupère la caméra enfant
		Car = GetNode<Node3D>("Car");

		// Récupère le pivot de rotation
		CarPivot = GetNode<Node3D>("CarPivot");
		Camera = CarPivot.GetNode<Camera3D>("Camera");
		Ball = GetTree().GetNodesInGroup("Ball")[0] as Node3D;
	}

	public override void _PhysicsProcess(double delta)
	{
		Ball.ForceUpdateTransform();

		CameraRotation(delta);
	}

	private void CameraRotation(double delta)
	{
		
		// Contrôle de la caméra avec le joystick droit
		float cameraHorizontal = Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left");
		CarPivot.GlobalPosition = Car.GlobalPosition;

		if(cameraHorizontal != 0) 
		{
			// Rotation de la caméra avec limitation de l'angle
			float newCameraRotation = cameraHorizontal * CameraSensitivity - CameraAngle;
			newCameraRotation = Mathf.Clamp(newCameraRotation, -Mathf.DegToRad(MaxCameraAngle), Mathf.DegToRad(MaxCameraAngle));
			CarPivot.Rotation -= new Vector3(0, newCameraRotation, 0);
			return;
		} 

		IsBallCamLock = Input.IsActionJustPressed("camera_lock") ? !IsBallCamLock : IsBallCamLock;
		// Le twist pivo va suivre parfaitement la voiture

		if (IsBallCamLock)
		{
			Vector3 ballPosition = Ball.GlobalTransform.Origin; // Position actuelle et précise de la balle
			CarPivot.LookAt(ballPosition);
		}
		else
		{
			// Suivi classique de la voiture
			CarPivot.GlobalRotation = new Vector3(
				0,
				Mathf.LerpAngle(CarPivot.GlobalRotation.Y, Car.GlobalRotation.Y, (float)delta * CameraResetSpeed),
				0
			);
		}
		CameraAngle = Mathf.RadToDeg(Camera.Rotation.Y);
	}
}
