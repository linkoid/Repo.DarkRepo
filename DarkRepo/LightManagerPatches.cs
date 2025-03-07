using HarmonyLib;
using Linkoid.Repo.DarkRepo;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace Linkoid.Repo.DarkRepo;

[HarmonyPatch(typeof(LightManager))]
internal class LightManagerPatches
{
    //internal static bool KeepHalos = true;
    internal static float LightFactor = 0.25f;
    internal static float EmissiveFactor = 0.75f;

    [HarmonyPrefix, HarmonyPatch(nameof(LightManager.FadeLightIntensity))]
    static void FadeLightIntensity_Prefix(ref float targetIntensity)
    {
        targetIntensity *= LightFactor;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(LightManager.FadeEmissionIntensity))]
    static void FadeEmissionIntensity_Prefix(ref Color targetColor)
    {
        targetColor *= EmissiveFactor;
    }
}
