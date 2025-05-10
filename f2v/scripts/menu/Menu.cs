using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Menu : Control
{
    private Button _startButton;
    private Button _lanButton;

    // Called when the node enters the scene tree for the first time.
    private List<Container> _navigationContainers = new();

    public override void _Ready()
    {
        _startButton = GetNode<Button>("CenterContainer/MainMenu/Game");
        _lanButton = GetNode<Button>("CenterContainer/MainMenu/Lan");
        // Select the first button nevermind which one is the 1st
        _navigationContainers.Add(GetNode<Container>("CenterContainer/MainMenu"));
        InitButtons();
    }

    // Modifie InitButtons
    public void InitButtons()
    {
        _navigationContainers.Last().GetChildren().OfType<Button>().FirstOrDefault().GrabFocus();
    }
    private bool IsGameLoaded()
    {
        return GetTree().Root.GetChildren().OfType<Game>().Count() != 0;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("game_menu") && IsGameLoaded())
        {
            Visible = !Visible;
            InitButtons();
        }
        // Check if the user is pressing the back button
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (_navigationContainers.Count == 1)
            {
                if (IsGameLoaded())
                    Visible = false;
            }
            else
                OnBackButtonPressed();
        }

        if (!Visible)
        {
            return;
        }

        if (!IsGameLoaded())
        {
            _startButton.Text = "Start";
            _lanButton.Visible = false;
        }
        else
        {
            _startButton.Text = "Continue";
            _lanButton.Visible = true;
        }
    }

    // Get All Children Recursively of a specific type
    private List<T> GetChildren<T>()
    {
        List<T> children = new List<T>();
        GetChildrenRecursive<T>(this, children);
        return children;
    }

    private void GetChildrenRecursive<T>(Node node, List<T> children)
    {
        foreach (Control child in node.GetChildren().Cast<Control>())
        {
            if (child is T)
            {
                children.Add((T)(object)child);
            }
            if (child.Visible)
                GetChildrenRecursive<T>(child, children);
        }
    }

    private void _on_game_pressed()
    {
        Rpc("StartGame");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void StartGame()
    {
        if (!IsGameLoaded())
        {
            var game = ResourceLoader.Load<PackedScene>("res://scenes/Game.tscn").Instantiate<Node3D>();
            GetTree().Root.AddChild(game);
        }

        this.Hide();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnControlsPressed()
    {
        _navigationContainers.Last().Visible = false;
        _navigationContainers.Add(GetNode<Container>("CenterContainer/ControlsMenu"));
        _navigationContainers.Last().Visible = true;

        InitButtons();
    }

    private void OnLanPressed()
    {
        _navigationContainers.Last().Visible = false;
        _navigationContainers.Add(GetNode<Container>("CenterContainer/LanMenu"));
        _navigationContainers.Last().Visible = true;

        InitButtons();
    }

    private void OnBackButtonPressed()
    {
        _navigationContainers.Last().Visible = false;
        _navigationContainers.RemoveAt(_navigationContainers.Count - 1);
        _navigationContainers.Last().Visible = true;

        InitButtons();
    }
}
