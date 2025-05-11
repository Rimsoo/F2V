using Godot;
using System.Collections.Generic;

public partial class SettingsManager : Node
{
    private const string SETTINGS_PATH = "user://settings.cfg";

    private Dictionary<string, Dictionary<string, Variant>> _settings = new()
    {
        {"Audio", new Dictionary<string, Variant>()},
        {"Display", new Dictionary<string, Variant>()},
        {"Controls", new Dictionary<string, Variant>()},
        {"Gameplay", new Dictionary<string, Variant>()}
    };

    // Singleton pattern
    private static SettingsManager _instance;
    public static SettingsManager Instance => _instance;

    public override void _EnterTree()
    {
        if (_instance != null)
        {
            QueueFree();
            return;
        }
        _instance = this;
        LoadAllSettings();
    }

    public void SaveAllSettings()
    {
        var config = new ConfigFile();

        foreach (var section in _settings)
        {
            foreach (var entry in section.Value)
            {
                config.SetValue(section.Key, entry.Key, entry.Value);
            }
        }

        config.Save(SETTINGS_PATH);
    }

    public void LoadAllSettings()
    {
        var config = new ConfigFile();
        if (config.Load(SETTINGS_PATH) != Error.Ok) return;

        foreach (var section in _settings.Keys)
        {
            if (!config.HasSection(section)) continue;

            foreach (string key in config.GetSectionKeys(section))
            {
                _settings[section][key] = config.GetValue(section, key);
            }
        }
    }

    // Méthodes d'accès aux paramètres
    public Variant? GetSetting(string category, string key, Variant? defaultValue = null)
    {
        if (_settings.TryGetValue(category, out var categoryDict) &&
            categoryDict.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }

    public void SetSetting(string category, string key, Variant value)
    {
        if (_settings.ContainsKey(category))
        {
            _settings[category][key] = value;
            SaveAllSettings();
        }
    }

}
