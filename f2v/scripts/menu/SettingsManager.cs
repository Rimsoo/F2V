using Godot;
using System.Collections.Generic;

public partial class SettingsManager : Node
{
    public enum InputType { KeyboardMouse, Gamepad }
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

    public void SetControlEvents(string action, InputType inputType, Godot.Collections.Array<InputEvent> events)
    {
        string key = $"{action}_{inputType}";
        var serialized = new Godot.Collections.Array<Variant>();
        foreach (var evt in events) serialized.Add(evt);
        _settings["Controls"][key] = serialized;
    }

    public Godot.Collections.Array<InputEvent> GetControlEvents(string action, InputType inputType)
    {
        string key = $"{action}_{inputType}";
        var result = new Godot.Collections.Array<InputEvent>();
        if (_settings["Controls"].TryGetValue(key, out var value))
        {
            foreach (Variant item in value.As<Godot.Collections.Array>())
            {
                if (item.As<InputEvent>() is InputEvent evt)
                    result.Add(evt);
            }
        }
        return result;
    }

    // Alternative plus simple pour un seul événement
    public void SetControlEvent(string action, InputEvent @event)
    {
        _settings["Controls"][action] = @event;
    }

    public InputEvent GetControlEvent(string action)
    {
        var value = _settings["Controls"].GetValueOrDefault(action, default(Variant));
        return value.VariantType == Variant.Type.Object ? value.As<InputEvent>() : null;
    }
}
