using Godot;
using System;

public partial class Game : Node3D
{
    [Export] PackedScene PlayerScene;

    public override void _Ready()
    {
        SpawnPlayers();
    }

    public override void _Process(double delta)
    {
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
            AddChild(currentPlayer);

            foreach (Node3D spawnPoint in GetTree().GetNodesInGroup("PlayerSpawnPoints"))
            {
                if (int.Parse(spawnPoint.Name) == i)
                {
                    currentPlayer.GlobalPosition = spawnPoint.GlobalPosition;
                    break;
                }
            }
        }
    }
}
