using HarmonyLib;
using Linkoid.Repo.DarkRepo;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace Linkoid.Repo.DarkRepo;

[HarmonyPatch(typeof(FlashlightController))]
internal class FlashlightControllerPatches
{
    internal static float FlashlightFactor = 2f;

    [HarmonyPostfix, HarmonyPatch(nameof(FlashlightController.LightOn))]
    static void LightOn_Prefix(FlashlightController __instance)
    {
        __instance.baseIntensity *= FlashlightFactor;
    }
}
