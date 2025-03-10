using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using BepInEx.Configuration;
using System;

namespace Linkoid.Repo.DarkRepo;

[BepInPlugin("Linkoid.Repo.DarkRepo", "Dark REPO", "1.1")]
public class DarkRepo : BaseUnityPlugin
{
    internal static DarkRepo Instance;

    internal static new ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;

    private ConfigModel ConfigModel;

    void Awake()
    {
        Instance = this;
        new Harmony(Info.Metadata.GUID).PatchAll();

        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        ConfigModel = new ConfigModel(this.Config);
        Config.SettingChanged += OnSettingChanged;

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    private void OnSettingChanged(object sender, SettingChangedEventArgs args)
    {
        if (args.ChangedSetting == ConfigModel.AmbientFactor)
        {
            EnvironmentDirectorPatches.FogColorFactor = ConfigModel.AmbientFactor.Value;
            EnvironmentDirectorPatches.AmbientColorFactor = ConfigModel.AmbientFactor.Value;
            EnvironmentDirectorPatches.AmbientColorAdaptationFactor = ConfigModel.AmbientFactor.Value;
            EnvironmentDirector.Instance.Setup();
        }
        else if (args.ChangedSetting == ConfigModel.LightFactor)
        {
            LightManagerPatches.LightFactor = ConfigModel.LightFactor.Value;
            LightManager.instance.UpdateLights();
        }
        else if (args.ChangedSetting == ConfigModel.EmissiveFactor)
        {
            LightManagerPatches.EmissiveFactor = ConfigModel.EmissiveFactor.Value;
            LightManager.instance.UpdateLights();
        }
        else if (args.ChangedSetting == ConfigModel.FlashlightFactor)
        {
            FlashlightControllerPatches.FlashlightFactor = ConfigModel.FlashlightFactor.Value;
            if (FlashlightController.Instance?.currentState == FlashlightController.State.LightOn)
            {
                FlashlightController.Instance.LightOn();
            }
        }
    }
}