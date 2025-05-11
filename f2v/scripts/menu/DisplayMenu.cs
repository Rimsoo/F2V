using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class DisplayMenu : GridContainer
{

    private Button _fullscreenCheckbox;
    private Button _borderlessCheckbox;
    private Button _vsyncCheckBox;

    private SettingsManager _settings => SettingsManager.Instance;

    public override void _Ready()
    {
        _fullscreenCheckbox = GetTree().GetNodesInGroup("display_settings").First(b => b.Name.Equals("Fullscreen")) as Button;
        _borderlessCheckbox = GetTree().GetNodesInGroup("display_settings").First(b => b.Name.Equals("Borderless")) as Button;
        _vsyncCheckBox = GetTree().GetNodesInGroup("display_settings").First(b => b.Name.Equals("VSync")) as Button;

        _fullscreenCheckbox.SetPressed((bool)_settings.GetSetting("Display", "Fullscreen", true));
        _borderlessCheckbox.SetPressed((bool)_settings.GetSetting("Display", "Borderless", false));
        _vsyncCheckBox.SetPressed((bool)_settings.GetSetting("Display", "VSync", true));
    }

    private void _on_bs_toggled(bool toggle_on)
    {
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, toggle_on);
        _settings.SetSetting("Display", "Borderless", toggle_on);
    }

    private void _on_fs_toggled(bool toggle_on)
    {
        if (toggle_on)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }

        _settings.SetSetting("Display", "Fullscreen", toggle_on);
    }

    private void _on_vs_toggled(bool toggle_on)
    {
        if (toggle_on)
        {
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
        }
        else
        {
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
        }

        _settings.SetSetting("Display", "VSync", toggle_on);
    }
}
