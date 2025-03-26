using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Linkoid.Repo.DarkRepo;

internal static class LevelAdjustment
{
    internal static Dictionary<string, ConfigEntry<float>> ConfigDictionary;

    public static float CurrentValue => CurrentConfig?.Value ?? 0f;

    internal static bool BindingNewConfig { get; private set; }

    public static ConfigEntry<float>? CurrentConfig
    {
        get
        {
            if (LevelGenerator.Instance?.Level?.NarrativeName == null)
                return null;

            string key = LevelGenerator.Instance.Level.NarrativeName.Replace(" ", "");
            if (string.IsNullOrWhiteSpace(key))
                return null;

            if (!ConfigDictionary.TryGetValue(key, out var config))
            {
                BindingNewConfig = true;
                config = DarkRepo.Instance.Config.Bind(ConfigModel.LevelsSection, key, 0f, ConfigModel.LevelConfigDescription);
                ConfigDictionary.Add(key, config);
                BindingNewConfig = false;
            }

            return config;
        }
    }
}
