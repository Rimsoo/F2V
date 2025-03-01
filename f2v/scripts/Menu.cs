using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Menu : Control
{
    // Called when the node enters the scene tree for the first time.

    private Button _focusButton;
    private List<Button> _buttons;
    private List<Container> _navigationContainers = new();

    public override void _Ready()
    {
        // Select the first button nevermind which one is the 1st
        _navigationContainers.Add(GetNode<Container>("CenterContainer/MainMenu"));
        InitButtons();
    }

    private void InitButtons()
    {
        _buttons = GetChildren<Button>();
        _focusButton = _buttons[0];
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!Visible)
        {
            return;
        }
        Focus(_focusButton);

        // Check if the user is pressing the up or down button
        ButtonNavigation();

        ButtonSelection();

        // Check if the user is pressing the back button
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            OnBackButtonPressed();
        }
    }

    private void ButtonSelection()
    {
        if (Input.IsActionJustPressed("ui_select"))
        {
            // Emit the signal of the focused button
            _focusButton.EmitSignal("pressed");
        }
    }

    private void ButtonNavigation()
    {
        if (Input.IsActionJustPressed("ui_up") || Input.IsActionJustPressed("ui_down"))
        {
            // Get the current index of the focused button + 1 or -1
            int index =
                _buttons.IndexOf(_focusButton) + (Input.IsActionJustPressed("ui_up") ? -1 : 1);

            // If the next button is null, we are at the end of the list
            if (index < 0 || index >= _buttons.Count)
            {
                return;
            }

            // Set the focus to the next button
            Focus(_buttons[index]);
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

    private void Focus(Button button)
    {
        _focusButton = button;
        button.GrabFocus();
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
