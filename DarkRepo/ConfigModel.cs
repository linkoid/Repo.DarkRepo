using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Linkoid.Repo.DarkRepo;

internal record class ConfigModel(ConfigFile ConfigFile)
{
    internal const string GeneralSection = "General";
    public readonly ConfigEntry<float> AmbientFactor    = ConfigFile.Bind(GeneralSection, "AmbientFactor"   , 0.40f, new ConfigDescription("General environment brightness", new AcceptableValueRange<float>(0.01f, 2f)));
    public readonly ConfigEntry<float> LightFactor      = ConfigFile.Bind(GeneralSection, "LightFactor"     , 0.45f, new ConfigDescription("Dynamic lighting intensity"    , new AcceptableValueRange<float>(0.01f, 2f)));
    public readonly ConfigEntry<float> EmissiveFactor   = ConfigFile.Bind(GeneralSection, "EmissiveFactor"  , 0.85f, new ConfigDescription("Glowing texture intensity"     , new AcceptableValueRange<float>(0.01f, 2f)));
    public readonly ConfigEntry<float> FlashlightFactor = ConfigFile.Bind(GeneralSection, "FlashlightFactor", 2.00f, new ConfigDescription("Flashlight intensity"          , new AcceptableValueRange<float>(0.01f, 8f)));

    internal const string LevelsSection = "Levels";
    internal static readonly ConfigDescription LevelConfigDescription = new ConfigDescription("<-- Darker | 0 = Use General Settings | Lighter --> | 1 = Game Default", new AcceptableValueRange<float>(-0.99f, 2f));
    public readonly Dictionary<string, ConfigEntry<float>> Levels = new()
    {
        ["MainMenu"         ] = ConfigFile.Bind(LevelsSection, "MainMenu"         , 0f, LevelConfigDescription),
        ["LobbyMenu"        ] = ConfigFile.Bind(LevelsSection, "LobbyMenu"        , 0f, LevelConfigDescription),
        ["ServiceStation"   ] = ConfigFile.Bind(LevelsSection, "ServiceStation"   , 0f, LevelConfigDescription),
        ["Truck"            ] = ConfigFile.Bind(LevelsSection, "Truck"            , 0f, LevelConfigDescription),
        ["DisposalArena"    ] = ConfigFile.Bind(LevelsSection, "DisposalArena"    , 0f, LevelConfigDescription),
        ["HeadmanManor"     ] = ConfigFile.Bind(LevelsSection, "HeadmanManor"     , 0f, LevelConfigDescription),
        ["McJannekStation"  ] = ConfigFile.Bind(LevelsSection, "McJannekStation"  , 0f, LevelConfigDescription),
        ["SwiftbroomAcademy"] = ConfigFile.Bind(LevelsSection, "SwiftbroomAcademy", 0f, LevelConfigDescription),
    };

    public void RunMigrations()
    {
        ConfigFile.SaveOnConfigSet = false;
        try
        {
            Migrate_1_3();
            ConfigFile.Save();
        }
        catch (Exception ex)
        {
            DarkRepo.Logger.LogWarning($"An error occured during config migration. Please delete the config file.\n{ex.Message}\n{ex.StackTrace}");
        }
        ConfigFile.SaveOnConfigSet = true;
    }

    public void LoadExtraLevelConfigs()
    {
        var prop_OrphanedEntries = AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries");
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)prop_OrphanedEntries.Invoke(ConfigFile, []);
        foreach (var definition in new List<ConfigDefinition>(ConfigFile.GetOrphanedDefinitions()))
        {
            if (definition.Section != LevelsSection) continue;
            if (Levels.ContainsKey(definition.Key)) continue;

            Levels[definition.Key] = ConfigFile.Bind(definition, 0f, LevelConfigDescription);
        }
    }

    private bool Migrate_1_3()
    {
        bool hasOldConfig = false;
        hasOldConfig |= ConfigFile.TryGetOrphanedEntry<float>(new ConfigDefinition("DarkRepoConfig", "AmbientFactor"   ), out var oldAmbientFactor   );
        hasOldConfig |= ConfigFile.TryGetOrphanedEntry<float>(new ConfigDefinition("DarkRepoConfig", "LightFactor"     ), out var oldLightFactor     );
        hasOldConfig |= ConfigFile.TryGetOrphanedEntry<float>(new ConfigDefinition("DarkRepoConfig", "EmissiveFactor"  ), out var oldEmissiveFactor  );
        hasOldConfig |= ConfigFile.TryGetOrphanedEntry<float>(new ConfigDefinition("DarkRepoConfig", "FlashlightFactor"), out var oldFlashlightFactor);

        const float oldAmbientFactorDefault    = 0.20f; 
        const float oldLightFactorDefault      = 0.25f; 
        const float oldEmissiveFactorDefault   = 0.75f;
        const float oldFlashlightFactorDefault = 2.00f;

        if (!hasOldConfig) return false;

        DarkRepo.Logger.LogMessage("Migrating config from v1.3 to v1.4");

        if (   (oldAmbientFactor != null && oldAmbientFactor.Value != oldAmbientFactorDefault)
            || (oldLightFactor   != null && oldLightFactor  .Value != oldLightFactorDefault  ))
        {
            AmbientFactor.Value = oldAmbientFactor?.Value ?? oldAmbientFactorDefault;
            LightFactor.Value   = oldLightFactor  ?.Value ?? oldLightFactorDefault;
        }

        if (oldEmissiveFactor != null && oldEmissiveFactor.Value != oldEmissiveFactorDefault)
        {
            EmissiveFactor.Value = oldEmissiveFactor.Value;
        }

        if (oldFlashlightFactor != null && oldFlashlightFactor.Value != oldFlashlightFactorDefault)
        {
            FlashlightFactor.Value = oldFlashlightFactor.Value;
        }

        ConfigFile.Remove(oldAmbientFactor   ?.Definition);
        ConfigFile.Remove(oldLightFactor     ?.Definition);
        ConfigFile.Remove(oldEmissiveFactor  ?.Definition);
        ConfigFile.Remove(oldFlashlightFactor?.Definition);

        return true;
    }
}
