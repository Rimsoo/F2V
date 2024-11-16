using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed = 20.0f; // Vitesse de la voiture
    [Export] public float TurnSpeed = 3.0f; // Vitesse de rotation
    [Export] public float JumpVelocity = 10.0f;
    [Export] public float CameraSensitivity = 0.1f; // Sensibilité de la caméra
    [Export] public float CameraResetSpeed = 10.0f; // Vitesse de retour de la caméra à la position de base
    [Export] public float maxCameraAngle = 80.0f; // Angle maximal de la caméra
    [Export] public float camHeihgtY = 3.0f; // Hauteur de la caméra
    [Export] public float camDistanceZ = 6.0f; // Distance de la caméra
    [Export] public float camRotationX = 15.0f; // Distance de la caméra

    private Node3D camera; // Référence à la caméra
    private Vector3 cameraBaseRotation; // Rotation de base de la caméra

    public override void _Ready()
    {
        // Récupère la caméra enfant
        camera = GetNode<Node3D>("Camera3D");

        // Stocke la rotation de base de la caméra pour le retour automatique
        if (camera != null)
        {
            cameraBaseRotation = camera.Rotation;
            camRotationX = camera.Rotation.X;
            camDistanceZ = camera.Position.Z;
            camHeihgtY = camera.Position.Y;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Ajouter la gravité si la voiture n'est pas au sol
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        } 
        else
        {
            // Remise a plat apres aerial
            if (Rotation.X != 0 || Rotation.Z != 0)
            {
                Vector3 final = new Vector3(0, Rotation.Y, 0);
                // Rendre la rotation plus douce
                Rotation = new Vector3(
                    Mathf.Lerp(Rotation.X, final.X, 10.0f * (float)delta),
                    Mathf.Lerp(Rotation.Y, final.Y, 10.0f * (float)delta),
                    Mathf.Lerp(Rotation.Z, final.Z, 10.0f * (float)delta)
                );
            }
        }

        // Gestion du saut
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Accélération et marche arrière avec `RT` et `LT`
        float forwardInput = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward");

		if(IsOnFloor())
		{
			// Déplace la voiture dans la direction vers laquelle elle est orientée
			Vector3 forwardDirection = -Transform.Basis.Z; // La direction vers l’avant de la voiture
			velocity.X = forwardDirection.X * forwardInput * Speed;
			velocity.Z = forwardDirection.Z * forwardInput * Speed;
		}

        // Rotation de la voiture avec le joystick gauche, inversée
        float leftRightTurnInput = Input.GetActionStrength("turn_right") - Input.GetActionStrength("turn_left");
        float upDownTurnInput = 0;
        float rollInput = 0;
        if (!IsOnFloor())
        {
            upDownTurnInput = Input.GetActionStrength("turn_up") - Input.GetActionStrength("turn_down"); 
            rollInput = Input.GetActionStrength("roll_right") - Input.GetActionStrength("roll_left");
        }
        Rotation = new Vector3(Rotation.X - upDownTurnInput * TurnSpeed * (float)delta, Rotation.Y - leftRightTurnInput * TurnSpeed * (float)delta, Rotation.Z - rollInput * TurnSpeed * (float)delta);
        // Mise à jour de la vélocité en Y et déplacement
		Velocity = velocity;
        MoveAndSlide();

        // Contrôle de la caméra avec le joystick droit
        float cameraHorizontal = Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left");

        if (camera != null)
        {
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
}
