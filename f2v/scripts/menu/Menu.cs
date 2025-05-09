using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Menu : Control
{
    // Called when the node enters the scene tree for the first time.
    private List<Container> _navigationContainers = new();

    public override void _Ready()
    {
        // Select the first button nevermind which one is the 1st
        _navigationContainers.Add(GetNode<Container>("CenterContainer/MainMenu"));
    }

    // Modifie InitButtons
    public void InitButtons()
    {
        _navigationContainers.Last().GetChildren().OfType<Button>().FirstOrDefault().GrabFocus();
    }

    private Button GetFirstButton(Node node)
    {
        // Vérifier si le nœud actuel est un Button
        if (node is Button button)
        {
            return button;
        }

        // Parcourir les enfants récursivement
        foreach (Node child in node.GetChildren())
        {
            Button result = GetFirstButton(child);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!Visible)
        {
            return;
        }

        // Check if the user is pressing the back button
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            OnBackButtonPressed();
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
        if (_navigationContainers.Count == 1)
        {
            Visible = false;
            return;
        }

        _navigationContainers.Last().Visible = false;
        _navigationContainers.RemoveAt(_navigationContainers.Count - 1);
        _navigationContainers.Last().Visible = true;

        InitButtons();
    }
}
