using Godot;
using System;
using System.Linq;

public partial class AudioMenu : GridContainer
{
    private HSlider _masterVolumeSlider;
    private HSlider _musicVolumeSlider;
    private HSlider _sfxVolumeSlider;

    private SettingsManager _settings => SettingsManager.Instance;

    public override void _Ready()
    {
        _masterVolumeSlider = GetTree().GetNodesInGroup("audio_settings").First(b => b.Name.Equals("Master")) as HSlider;
        _musicVolumeSlider = GetTree().GetNodesInGroup("audio_settings").First(b => b.Name.Equals("Music")) as HSlider;
        _sfxVolumeSlider = GetTree().GetNodesInGroup("audio_settings").First(b => b.Name.Equals("SFX")) as HSlider;

        _masterVolumeSlider.Value = (float)_settings.GetSetting("Audio", "MasterVolume", 1.0f);
        _musicVolumeSlider.Value = (float)_settings.GetSetting("Audio", "MusicVolume", 1.0f);
        _sfxVolumeSlider.Value = (float)_settings.GetSetting("Audio", "SFXVolume", 1.0f);
    }

    private void _on_master_value_changed(float value)
    {
        SetAudioBusValue("Master", value);
    }

    private void _on_music_value_changed(float value)
    {
        SetAudioBusValue("Music", value);
    }

    private void _on_sfx_value_changed(float value)
    {
        SetAudioBusValue("SFX", value);
    }

    private void SetAudioBusValue(string bus, float value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(bus), Mathf.LinearToDb(value));
        _settings.SetSetting("Audio", $"{bus}Volume", value);
    }
}
