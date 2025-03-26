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
    internal static float LightFactor    => LightFactorCurve   .Evaluate(LevelAdjustment.CurrentValue);
    internal static float EmissiveFactor => EmissiveFactorCurve.Evaluate(LevelAdjustment.CurrentValue);

    internal static AdjustmentCurve LightFactorCurve    = new(0.25f);
    internal static AdjustmentCurve EmissiveFactorCurve = new(0.75f);

    [HarmonyPrefix, HarmonyPatch(nameof(LightManager.FadeLightIntensity))]
    static void FadeLightIntensity_Prefix(PropLight propLight, ref float targetIntensity)
    {
        targetIntensity *= LightFactor;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(LightManager.FadeEmissionIntensity))]
    static void FadeEmissionIntensity_Prefix(ref Color targetColor)
    {
        targetColor *= EmissiveFactor;
    }
}
