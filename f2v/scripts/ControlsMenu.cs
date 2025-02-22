using Godot;
using System;

public partial class ControlsMenu : GridContainer
{
	// Called when the node enters the scene tree for the first time.
	private Label _waitForKeyLabel;
	private bool isWaitingForKey;
	private string actionToRebind;
	private Button _pressedButton;

	public override void _Ready()
	{
		_waitForKeyLabel = GetNode<Label>("WaitContainer/WaitInput");
		_waitForKeyLabel.Visible = false;

		foreach (Button button in GetTree().GetNodesInGroup("menu_buttons"))
		{
			button.Pressed += () => ButtonClicked(button);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_waitForKeyLabel.Visible = isWaitingForKey;		
	}

	public void ButtonClicked(Button pressedButton)
	{
		if(!pressedButton.HasMeta("action_name")){
			return;
		}
			
		// Récupère l'action stockée dans les Metadata
		actionToRebind = (string)pressedButton.GetMeta("action_name");

		isWaitingForKey = true;
		_waitForKeyLabel.Visible = true;
		_pressedButton = pressedButton;
	}

	public override void _Input(InputEvent @event)
	{
		if (!isWaitingForKey) return; // Ne bloque que si on attend un input

		// Bloque les autres inputs
		GetViewport().SetInputAsHandled();

		// Annulation avec Échap
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			isWaitingForKey = false;
			_waitForKeyLabel.Visible = false;
		}

		if (@event is InputEvent keyEvent && keyEvent.IsPressed())
		{
			InputMap.ActionEraseEvents(actionToRebind);
			InputMap.ActionAddEvent(actionToRebind, keyEvent);

			_pressedButton.Text = keyEvent.AsText();

			isWaitingForKey = false;
			_waitForKeyLabel.Visible = false;
		}
	}
}
