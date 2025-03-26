using HarmonyLib;
using Linkoid.Repo.DarkRepo;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace Linkoid.Repo.DarkRepo;

[HarmonyPatch(typeof(EnvironmentDirector))]
internal static class EnvironmentDirectorPatches
{
    internal static float FogColorFactor               => FogColorFactorCurve              .Evaluate(LevelAdjustment.CurrentValue);
    internal static float AmbientColorFactor           => AmbientColorFactorCurve          .Evaluate(LevelAdjustment.CurrentValue);
    internal static float AmbientColorAdaptationFactor => AmbientColorAdaptationFactorCurve.Evaluate(LevelAdjustment.CurrentValue);

    internal static AdjustmentCurve FogColorFactorCurve               = new(0.2f);
    internal static AdjustmentCurve AmbientColorFactorCurve           = new(0.2f);
    internal static AdjustmentCurve AmbientColorAdaptationFactorCurve = new(0.2f);

    [HarmonyPostfix, HarmonyPatch(nameof(EnvironmentDirector.Setup))]
    static void Setup_Postfix(EnvironmentDirector __instance)
    {
        float factor = FogColorFactorCurve.Evaluate(LevelAdjustment.CurrentValue);
        RenderSettings.fogColor *= factor;
        __instance.MainCamera.backgroundColor *= factor;
    }

    [HarmonyTranspiler, HarmonyPatch(nameof(EnvironmentDirector.Update))]
    static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var smeth_Color_op_Multiply_float = AccessTools.Method(typeof(Color), "op_Multiply", new[] { typeof(Color), typeof(float) });
        var field_Level_AmbientColor = AccessTools.Field(typeof(Level), nameof(Level.AmbientColor));
        var field_Level_AmbientColorAdaptation = AccessTools.Field(typeof(Level), nameof(Level.AmbientColorAdaptation));
        var sgetter_AmbientColorFactor = AccessTools.PropertyGetter(typeof(EnvironmentDirectorPatches), nameof(AmbientColorFactor));
        var sgetter_AmbientColorAdaptationFactor = AccessTools.PropertyGetter(typeof(EnvironmentDirectorPatches), nameof(AmbientColorAdaptationFactor));
         
        var head = new CodeMatcher(instructions);

        //head.Advance(7);
        //DarkRepo.Logger.LogDebug(head.Instruction.opcode);
        //DarkRepo.Logger.LogDebug(head.Instruction.operand.GetType());
        //DarkRepo.Logger.LogDebug(field_Level_AmbientColor.GetType());
        //DarkRepo.Logger.LogDebug($"head.Instruction.operand == field_Level_AmbientColor: {head.Instruction.operand == field_Level_AmbientColor}");
        //head.Start();

        // Rewrite:
        //     level.AmbientColor
        // To:
        //     level.AmbientColor * EnvironmentDirectorPatches.AmbientColorFactor;
        CodeMatch[] ambientColorMatch =
        {
            new(OpCodes.Ldfld, field_Level_AmbientColor)
        };
        head.MatchForward(useEnd: true, ambientColorMatch);
        head.ThrowIfInvalid($"Could not match {nameof(ambientColorMatch)}");
        head.Advance(1);
        DarkRepo.Logger.LogDebug(head.Instruction.opcode);
        head.Insert(
            new CodeInstruction(OpCodes.Call, sgetter_AmbientColorFactor),
            new CodeInstruction(OpCodes.Call, smeth_Color_op_Multiply_float)
        );


        // Rewrite:
        //     level.AmbientColorAdaptation
        // To:
        //     level.AmbientColorAdaptation * EnvironmentDirectorPatches.AmbientColorAdaptationFactor;
        CodeMatch[] ambientColorAdaptationMatch =
        {
            new(OpCodes.Ldfld, field_Level_AmbientColorAdaptation)
        };
        head.MatchForward(useEnd: true, ambientColorAdaptationMatch);
        head.ThrowIfInvalid($"Could not match {nameof(ambientColorAdaptationMatch)}");
        head.Advance(1);
        DarkRepo.Logger.LogDebug(head.Instruction.opcode);
        head.Insert(
            new CodeInstruction(OpCodes.Call, sgetter_AmbientColorAdaptationFactor),
            new CodeInstruction(OpCodes.Call, smeth_Color_op_Multiply_float)
        );

        return head.InstructionEnumeration();
    }
}
