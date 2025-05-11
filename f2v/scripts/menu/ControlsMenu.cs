using System;
using Godot;

public partial class ControlsMenu : GridContainer
{
    // Called when the node enters the scene tree for the first time.
    private bool isWaitingForKey;
    private string actionToRebind;
    private Button _pressedButton;

    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void ButtonClicked(NodePath pressedButtonPath)
    {
        GD.Print(pressedButtonPath);
        Button pressedButton = GetNode(pressedButtonPath) as Button;
        if (!pressedButton.HasMeta("action_name"))
        {
            return;
        }

        // Récupère l'action stockée dans les Metadata
        actionToRebind = (string)pressedButton.GetMeta("action_name");

        isWaitingForKey = true;
        _pressedButton.Text = "Press a key...";
        _pressedButton = pressedButton;
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
        InputMap.ActionEraseEvents(actionToRebind);
        InputMap.ActionAddEvent(actionToRebind, @event);
        _pressedButton.Text = eventText;
    }

    private void RebindAction(InputEvent @event)
    {
        RebindAction(@event, @event.AsText());
    }

    // Ajouter cette méthode dans ControlsMenu
    private string GetAxisString(InputEventJoypadMotion motionEvent)
    {
        // Exemple : Convertir l'axe en texte lisible
        return $"Axe {motionEvent.Axis}";
    }
}
