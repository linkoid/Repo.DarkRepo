using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Linkoid.Repo.DarkRepo;

internal record class ConfigModel(ConfigFile ConfigFile)
{
    public readonly ConfigEntry<float> AmbientFactor    = ConfigFile.Bind("DarkRepoConfig", "AmbientFactor"   , 0.20f);
    public readonly ConfigEntry<float> LightFactor      = ConfigFile.Bind("DarkRepoConfig", "LightFactor"     , 0.25f);
    public readonly ConfigEntry<float> EmissiveFactor   = ConfigFile.Bind("DarkRepoConfig", "EmissiveFactor"  , 0.75f);
    public readonly ConfigEntry<float> FlashlightFactor = ConfigFile.Bind("DarkRepoConfig", "FlashlightFactor", 2.00f);
}
