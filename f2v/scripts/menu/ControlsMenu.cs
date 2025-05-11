using System;
using System.Linq;
using Godot;

public partial class ControlsMenu : GridContainer
{
    // Called when the node enters the scene tree for the first time.
    private bool isWaitingForKey;
    private string actionToRebind;
    private Button _pressedButton;

    private SettingsManager _settings => SettingsManager.Instance;

    public override void _Ready()
    {
        LoadBindings();

        foreach (Button button in GetTree().GetNodesInGroup("bind_buttons"))
        {
            button.Pressed += () => ButtonClicked(button);

            if (button.HasMeta("action_name"))
            {
                string action = (string)button.GetMeta("action_name");
                var events = InputMap.ActionGetEvents(action);
                button.Text = events.Count > 0
                    ? GetEventText(events[0])
                    : "Not assigned";
            }
        }
    }

    private string GetEventText(InputEvent @event)
    {
        if (@event is InputEventJoypadMotion motion)
            return GetAxisString(motion);

        if (@event is InputEventJoypadButton joyBtn)
            return $"Gamepad {joyBtn.ButtonIndex}";

        return @event.AsText();
    }

    private void ButtonClicked(Button pressedButton)
    {
        if (!pressedButton.HasMeta("action_name"))
        {
            return;
        }

        // Récupère l'action stockée dans les Metadata
        actionToRebind = (string)pressedButton.GetMeta("action_name");

        isWaitingForKey = true;
        _pressedButton = pressedButton;
        _pressedButton.Text = "Press a key...";
    }

    // Dans ControlsMenu.cs
    public override void _Input(InputEvent @event)
    {
        if (!isWaitingForKey)
            return;

        GetViewport().SetInputAsHandled(); // Bloquer la propagation

        // Annulation avec Échap
        if (@event.IsActionPressed("ui_cancel"))
        {
            isWaitingForKey = false;
            _pressedButton.Text = "Change";
            return;
        }

        bool eventHandled = false;

        // Gestion des touches et manettes
        if (@event is InputEventKey keyEvent && keyEvent.IsPressed())
        {
            RebindAction(keyEvent);
            eventHandled = true;
        }
        else if (@event is InputEventJoypadMotion motionEvent)
        {
            // Seuil pour éviter les faux positifs
            if (Mathf.Abs(motionEvent.AxisValue) > 0.5f)
            {
                // Créer un nouvel événement d'axe avec une valeur neutre
                var newMotionEvent = new InputEventJoypadMotion
                {
                    Axis = motionEvent.Axis,
                    AxisValue = motionEvent.AxisValue > 0 ? 1f : -1f, // Forcer une valeur neutre (1 ou -1)
                };
                RebindAction(newMotionEvent, GetAxisString(newMotionEvent));
                eventHandled = true;

                GD.Print("Joypad motion event");
                GD.Print(motionEvent.AxisValue);
            }
        }
        else if (@event is InputEventJoypadButton joypadButton && joypadButton.IsPressed())
        {
            RebindAction(joypadButton);
            eventHandled = true;
        }

        if (eventHandled)
        {
            isWaitingForKey = false;
        }
    }

    private void RebindAction(InputEvent @event, string eventText)
    {
        InputEvent clonedEvent = (InputEvent)@event.Duplicate();

        InputMap.ActionEraseEvents(actionToRebind);
        InputMap.ActionAddEvent(actionToRebind, clonedEvent);
        _pressedButton.Text = eventText;

        _settings.SetSetting("Controls", actionToRebind, clonedEvent);
    }

    private void RebindAction(InputEvent @event)
    {
        RebindAction(@event, @event.AsText());
    }

    private string GetAxisString(InputEventJoypadMotion motionEvent)
    {
        string axisName = motionEvent.Axis switch
        {
            JoyAxis.LeftX => "Left Stick X",
            JoyAxis.LeftY => "Left Stick Y",
            JoyAxis.RightX => "Right Stick X",
            JoyAxis.RightY => "Right Stick Y",
            JoyAxis.TriggerLeft => "Left Trigger",
            JoyAxis.TriggerRight => "Right Trigger",
            _ => $"Axis {motionEvent.Axis}"
        };

        return $"{axisName} {(motionEvent.AxisValue > 0 ? "+" : "-")}";
    }

    private void LoadBindings()
    {
        foreach (string action in InputMap.GetActions())
        {
            var value = _settings.GetSetting("Controls", action);

            if (value?.VariantType == Variant.Type.Object)
            {
                // Utilisation de As<InputEvent>() avec gestion des null
                InputEvent inputEvent = value?.As<InputEvent>();

                if (inputEvent != null)
                {
                    InputMap.ActionEraseEvents(action);
                    InputMap.ActionAddEvent(action, inputEvent);

                    foreach (Button button in GetTree().GetNodesInGroup("bind_buttons"))
                    {
                        if ((string)button.GetMeta("action_name") == action)
                        {
                            button.Text = GetEventText(inputEvent);
                            break;
                        }
                    }
                }
                else
                {
                    GD.PrintErr($"Invalid input event type for action {action}");
                }
            }
            else if (value?.VariantType != Variant.Type.Nil)
            {
                GD.PrintErr($"Non-object value found for action {action}");
            }
        }
    }
}
