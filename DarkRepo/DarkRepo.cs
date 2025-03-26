using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using BepInEx.Configuration;

namespace Linkoid.Repo.DarkRepo;

[BepInPlugin("Linkoid.Repo.DarkRepo", "Dark REPO", "1.3")]
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
        ConfigModel.RunMigrations();
        ConfigModel.LoadExtraLevelConfigs();
        LevelAdjustment.ConfigDictionary = ConfigModel.Levels;
        FetchConfigValues();
        Config.SettingChanged += OnSettingChanged;

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    private void OnSettingChanged(object sender, SettingChangedEventArgs args)
    {
        if (args.ChangedSetting == null)
            return;

        if (args.ChangedSetting == ConfigModel.AmbientFactor)
        {
            var newCurve = new AdjustmentCurve(ConfigModel.AmbientFactor.Value);
            EnvironmentDirectorPatches.FogColorFactorCurve               = newCurve;
            EnvironmentDirectorPatches.AmbientColorFactorCurve           = newCurve;
            EnvironmentDirectorPatches.AmbientColorAdaptationFactorCurve = newCurve;
            EnvironmentDirector.Instance?.Setup();
        }
        else if (args.ChangedSetting == ConfigModel.LightFactor)
        {
            LightManagerPatches.LightFactorCurve = new (ConfigModel.LightFactor.Value);
            UpdateLightsInstant();
        }
        else if (args.ChangedSetting == ConfigModel.EmissiveFactor)
        {
            LightManagerPatches.EmissiveFactorCurve = new(ConfigModel.EmissiveFactor.Value);
            UpdateLightsInstant();
        }
        else if (args.ChangedSetting == ConfigModel.FlashlightFactor)
        {
            FlashlightControllerPatches.FlashlightFactor = ConfigModel.FlashlightFactor.Value;
            if (FlashlightController.Instance?.LightActive ?? false)
            {
                FlashlightController.Instance.lightOnLerp = 1f;
                FlashlightController.Instance.LightOn();
            }
        }
        else if (!LevelAdjustment.BindingNewConfig && args.ChangedSetting == LevelAdjustment.CurrentConfig)
        {
            Logger.LogDebug(args.ChangedSetting.Definition.Key);
            EnvironmentDirector.Instance?.Setup();
            UpdateLightsInstant();
        }
    }

    private void FetchConfigValues()
    {
        var ambientCurve = new AdjustmentCurve(ConfigModel.AmbientFactor.Value);
        EnvironmentDirectorPatches.FogColorFactorCurve               = ambientCurve;
        EnvironmentDirectorPatches.AmbientColorFactorCurve           = ambientCurve;
        EnvironmentDirectorPatches.AmbientColorAdaptationFactorCurve = ambientCurve;
        LightManagerPatches.LightFactorCurve                         = new(ConfigModel.LightFactor.Value);
        LightManagerPatches.EmissiveFactorCurve                      = new(ConfigModel.EmissiveFactor.Value);
        FlashlightControllerPatches.FlashlightFactor                 = ConfigModel.FlashlightFactor.Value;
    }

    private void UpdateLightsInstant()
    {
        if (LightManager.instance == null) return;

        var cullTarget = LightManager.instance.lightCullTarget;
        if (cullTarget == null) return;

        this.gameObject.transform.position = cullTarget.position + new Vector3(1, 1, 1) * GraphicsManager.instance.lightDistance * 100;
        LightManager.instance.lightCullTarget = this.gameObject.transform;
        LightManager.instance.UpdateInstant();

        LightManager.instance.lightCullTarget = cullTarget;
        LightManager.instance.UpdateInstant();

        this.gameObject.transform.position = Vector3.zero;
    }
}