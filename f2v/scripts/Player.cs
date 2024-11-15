using Godot;
using System;

public partial class Player : CharacterBody3D
{
    public const float Speed = 5.0f; // Vitesse de la voiture
    public const float TurnSpeed = 3.0f; // Vitesse de rotation
    public const float JumpVelocity = 4.5f;
    public const float CameraSensitivity = 0.1f; // Sensibilité de la caméra
    public const float CameraResetSpeed = 2.0f; // Vitesse de retour de la caméra à la position de base

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

        // Gestion du saut
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Accélération et marche arrière avec `RT` et `LT`
        float forwardInput = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward");

        // Déplace la voiture dans la direction vers laquelle elle est orientée
        Vector3 forwardDirection = -Transform.Basis.Z; // La direction vers l’avant de la voiture
        velocity.X = forwardDirection.X * forwardInput * Speed;
        velocity.Z = forwardDirection.Z * forwardInput * Speed;

        // Rotation de la voiture avec le joystick gauche, inversée
        float turnInput = Input.GetActionStrength("turn_right") - Input.GetActionStrength("turn_left");
        Rotation = new Vector3(Rotation.X, Rotation.Y - turnInput * TurnSpeed * (float)delta, Rotation.Z);

        // Mise à jour de la vélocité en Y et déplacement
        Velocity = velocity;
        MoveAndSlide();

        // Contrôle de la caméra avec le joystick droit
        float cameraHorizontal = Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left");
        float cameraVertical = Input.GetActionStrength("camera_down") - Input.GetActionStrength("camera_up");

        if (camera != null)
        {
            // Si le joystick est utilisé, rotation de la caméra
            if (cameraHorizontal != 0 || cameraVertical != 0)
            {
                camera.RotateY(-cameraHorizontal * CameraSensitivity);
                camera.RotateX(-cameraVertical * CameraSensitivity);
            }
            else
            {
                // Retour progressif de la caméra à sa position de base
                camera.Rotation = new Vector3(
                    Mathf.Lerp(camera.Rotation.X, cameraBaseRotation.X, CameraResetSpeed * (float)delta),
                    Mathf.Lerp(camera.Rotation.Y, cameraBaseRotation.Y, CameraResetSpeed * (float)delta),
                    Mathf.Lerp(camera.Rotation.Z, cameraBaseRotation.Z, CameraResetSpeed * (float)delta)
                );
            }
        }
    }
}
