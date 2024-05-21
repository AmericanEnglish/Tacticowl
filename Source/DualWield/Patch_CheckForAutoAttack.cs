using RimWorld;
using System.Reflection.Emit;
using HarmonyLib;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl.DualWield
{
    [HarmonyPatch(typeof(JobDriver_Wait), nameof(JobDriver_Wait.CheckForAutoAttack))]
    public class Patch_CheckForAutoAttack
    {
        static bool Prepare()
        {
            Harmony.DEBUG = true;
            return Settings.dualWieldEnabled;
        }
        
        static void Prefix( )
        {
            Log.Message("CheckForAutoAttack");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var tryStartBothAttacksMethod= AccessTools.Method(typeof(DualWieldUtility), nameof(DualWieldUtility.TryStartBothAttacks));
            var tryStartAttackMethod = AccessTools.Method(typeof(Pawn), nameof(Pawn.TryStartAttack));
            instructions = instructions.MethodReplacer(
                tryStartAttackMethod,
                tryStartBothAttacksMethod
            );
            foreach (var instruction in instructions)  yield return instruction ;
        }

    }
}