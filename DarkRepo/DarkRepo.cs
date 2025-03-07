using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Linkoid.Repo.DarkRepo;

[BepInPlugin("Linkoid.Repo.DarkRepo", "Dark REPO", "1.0")]
public class DarkRepo : BaseUnityPlugin
{
    internal static DarkRepo Instance;

    internal static new ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;


    private bool buttonHeld = false;


    void Awake()
    {
        Instance = this;
        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        new Harmony(Info.Metadata.GUID).PatchAll();

        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;
    }
}