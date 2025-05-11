using Godot;
using System.Linq;
using System;

public partial class Game : Node3D
{
    [Export] PackedScene PlayerScene;
    private Node3D _players;

    public override void _Ready()
    {
        _players = GetTree().GetNodesInGroup("Players").First() as Node3D;
        SpawnPlayers();
    }

    public override void _Process(double delta)
    {
        Menu Menu = GetTree().Root.GetNode<Menu>("F2V/Menu");
        if (Menu.Visible)
        {
            return;
        }

        float resetBall = Input.GetActionStrength("reset_ball");
        if (resetBall > 0.0f
            && Multiplayer.GetUniqueId() == 1)
        {
            // Reset the ball
            ResetBall();
            ReSpawnPlayers();
        }

    }

    public override void _ExitTree()
    {
        // Supprimez toutes les connexions
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        base._ExitTree();
    }

    public void SpawnPlayers()
    {
        for (int i = 0; i < GameManager.Players.Count; i++)
        {
            Player currentPlayer = PlayerScene.Instantiate<Player>();
            currentPlayer.Name = GameManager.Players[i].Id.ToString();
            _players.AddChild(currentPlayer);
            foreach (Node3D spawnPoint in GetTree().GetNodesInGroup("PlayerSpawnPoints"))
            {
                if (int.Parse(spawnPoint.Name) == i)
                {
                    currentPlayer.GlobalPosition = spawnPoint.GlobalPosition;
                    currentPlayer.GlobalRotation = spawnPoint.GlobalRotation;
                    break;
                }
            }
        }
    }

    private void ReSpawnPlayers()
    {
        foreach (Node child in _players.GetChildren())
        {
            child.QueueFree();
            _players.RemoveChild(child);
        }

        SpawnPlayers();
    }

    private void ResetBall()
    {
        // Reset the ball position
        RigidBody3D Ball = GetNode<RigidBody3D>("Ball");
        Ball.GlobalPosition = new Vector3(0, 2, 0);
        Ball.GlobalRotation = new Vector3(0, 0, 0);
        Ball.LinearVelocity = new Vector3(0, 0, 0);
    }
}
