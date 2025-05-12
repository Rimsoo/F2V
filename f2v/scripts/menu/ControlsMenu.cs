using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static SettingsManager;

public partial class ControlsMenu : Control
{
    private bool isWaitingForKey;
    private string currentAction;
    private InputType currentInputType;
    private Button _pressedButton;
    private SettingsManager _settings => SettingsManager.Instance;

    public override void _Ready()
    {
        LoadBindings();
        InitializeBindings();
        SetupButtons();
    }

    public override void _Process(double delta)
    {
    }

    private void SetupButtons()
    {
        foreach (Button button in GetTree().GetNodesInGroup("bind_buttons"))
        {
            button.Pressed += () => StartRebinding(button);
            UpdateButtonText(button);
        }
    }

    private void InitializeBindings()
    {
        foreach (string action in InputMap.GetActions())
        {
            var keyboardEvents = new Godot.Collections.Array<InputEvent>();
            var gamepadEvents = new Godot.Collections.Array<InputEvent>();

            foreach (InputEvent @event in InputMap.ActionGetEvents(action))
            {
                if (@event is InputEventKey || @event is InputEventMouse)
                {
                    keyboardEvents.Add(@event.Duplicate() as InputEvent);
                }
                else if (@event is InputEventJoypadButton || @event is InputEventJoypadMotion)
                {
                    gamepadEvents.Add(@event.Duplicate() as InputEvent);
                }
            }

            _settings.SetControlEvents(action, InputType.KeyboardMouse, keyboardEvents);
            _settings.SetControlEvents(action, InputType.Gamepad, gamepadEvents);
        }

        _settings.SaveAllSettings();
    }

    private void StartRebinding(Button button)
    {
        currentAction = (string)button.GetMeta("action_name");
        currentInputType = ((string)button.GetMeta("input_type")).Equals("keyboard") ? InputType.KeyboardMouse : InputType.Gamepad;
        isWaitingForKey = true;
        _pressedButton = button;
        _pressedButton.Text = "Press input...";
    }

    public override void _Input(InputEvent @event)
    {
        if (!isWaitingForKey) return;

        GetViewport().SetInputAsHandled();

        if (@event.IsActionPressed("ui_cancel"))
        {
            CancelRebinding();
            return;
        }

        if (IsValidForType(@event, currentInputType))
        {
            HandleValidEvent(@event);
        }
    }

    private void CancelRebinding()
    {
        if (!isWaitingForKey) return;

        isWaitingForKey = false;

        if (_pressedButton != null && _pressedButton.HasMeta("action_name"))
        {
            string action = (string)_pressedButton.GetMeta("action_name");
            string inputTypeMeta = (string)_pressedButton.GetMeta("input_type");
            InputType inputType = inputTypeMeta == "keyboard"
                ? InputType.KeyboardMouse
                : InputType.Gamepad;

            // Récupérer uniquement les événements du type concerné
            var events = _settings.GetControlEvents(action, inputType);

            // Filtrer les événements invalides
            var validEvents = events
                .Where(e => e != null && IsValidForType(e, inputType))
                .ToArray();

            // Mettre à jour le texte
            _pressedButton.Text = validEvents.Length > 0
                ? string.Join(", ", validEvents.Select(GetEventText))
                : "Not set";
        }

        _pressedButton = null;
    }

    private bool IsValidForType(InputEvent @event, InputType type)
    {
        return type switch
        {
            InputType.KeyboardMouse => @event is InputEventKey or InputEventMouseButton,
            InputType.Gamepad => @event is InputEventJoypadButton or InputEventJoypadMotion,
            _ => false
        };
    }

    private void HandleValidEvent(InputEvent @event)
    {
        SaveNewBinding(@event);
        UpdateButtonText(_pressedButton);
        FinalizeRebinding();
    }

    private void FinalizeRebinding()
    {
        isWaitingForKey = false;

        // Jouer un son de confirmation
        // GetNode<AudioStreamPlayer>("ConfirmSound")?.Play();

        // Petite animation visuelle
        var tween = CreateTween();
        tween.TweenProperty(_pressedButton, "modulate", Colors.Green, 0.15f);
        tween.TweenProperty(_pressedButton, "modulate", Colors.White, 0.15f);

        _pressedButton = null;
        currentAction = null;
    }

    private string GetMouseButtonName(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => "Left Click",
            MouseButton.Right => "Right Click",
            MouseButton.Middle => "Middle Click",
            _ => $"Button {(int)button}"
        };
    }

    private void SaveNewBinding(InputEvent @event)
    {
        // Supprimer tous les événements du même type
        var existing = _settings.GetControlEvents(currentAction, currentInputType);
        existing.Clear();

        // Ajouter le nouvel événement
        existing.Add(@event.Duplicate() as InputEvent);

        // Sauvegarder
        _settings.SetControlEvents(currentAction, currentInputType, existing);

        // Mettre à jour l'InputMap
        UpdateInputMapForAction(currentAction);
    }

    private void UpdateInputMapForAction(string action)
    {
        InputMap.ActionEraseEvents(action);

        // Ajouter tous les types
        foreach (InputType type in Enum.GetValues(typeof(InputType)))
        {
            foreach (InputEvent evt in _settings.GetControlEvents(action, type))
            {
                InputMap.ActionAddEvent(action, evt);
            }
        }
    }

    private void UpdateButtonText(Button button)
    {
        string action = (string)button.GetMeta("action_name");
        InputType inputType = ((string)button.GetMeta("input_type")) == "keyboard"
            ? InputType.KeyboardMouse
            : InputType.Gamepad;

        var events = _settings.GetControlEvents(action, inputType);

        button.Text = events.Count switch
        {
            0 => "Not set",
            1 => GetEventText(events[0]),
            _ => string.Join(", ", events.Select(GetEventText))
        };
    }

    private string GetEventText(InputEvent @event)
    {
        return @event switch
        {
            InputEventMouseButton mouseEvent => $"Mouse {GetMouseButtonName(mouseEvent.ButtonIndex)}",
            InputEventKey keyEvent => keyEvent.AsText(),
            InputEventJoypadButton joyButton => $"{joyButton.ButtonIndex}",
            InputEventJoypadMotion motionEvent => GetAxisString(motionEvent),
            _ => "Unknown"
        };
    }

    private void LoadBindings()
    {
        foreach (string action in InputMap.GetActions())
        {
            foreach (InputType inputType in Enum.GetValues(typeof(InputType)))
            {
                var events = _settings.GetControlEvents(action, inputType);
                foreach (InputEvent evt in events)
                {
                    if (IsValidForType(evt, inputType))
                    {
                        InputMap.ActionAddEvent(action, evt);
                    }
                }
            }
        }
    }

    private string GetAxisString(InputEventJoypadMotion motion)
    {
        string axisName = motion.Axis switch
        {
            JoyAxis.LeftX => "Left X",
            JoyAxis.LeftY => "Left Y",
            JoyAxis.RightX => "Right X",
            JoyAxis.RightY => "Right Y",
            _ => motion.Axis.ToString()
        };
        return $"{axisName} {(motion.AxisValue > 0 ? "+" : "-")}";
    }
}
