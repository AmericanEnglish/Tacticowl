﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl.DualWield
{
    public static class DualWieldUtility
    {
        public static bool TryStartOffHandAttack(Pawn pawn, LocalTargetInfo targ)
        {
            if (!DualWieldUtility.CanAttackOffhand(pawn)) return false;
            if (TryGetOffHandAttackVerb(pawn, targ.Thing, out Verb verb, true))
            {
                return verb.TryStartCastOn(targ);
            }
            return false;
        }
        static bool TryGetOffHandAttackVerb(Pawn instance, Thing target, out Verb verb, bool allowManualCastWeapons = false)
        {
            verb = null;
            if (instance.GetOffHander(out ThingWithComps offHandEquip))
            {
                CompEquippable compEquippable = offHandEquip.GetComp<CompEquippable>();
                
                if (compEquippable != null && compEquippable.PrimaryVerb.Available() && 
                (!compEquippable.PrimaryVerb.verbProps.onlyManualCast || instance.CurJobDef != JobDefOf.Wait_Combat || allowManualCastWeapons))
                {
                    verb = compEquippable.PrimaryVerb;
                }
            }
            else TryGetMeleeVerbOffHand(instance, target, out verb);
            return verb != null;
        }
        public static bool TryGetMeleeVerbOffHand(Pawn instance, Thing target, out Verb verb)
        {
            verb = null;
            if (instance.GetOffHander(out ThingWithComps offHandEquip))
            {              
                List<Verb> allVerbs = offHandEquip.GetComp<CompEquippable>()?.AllVerbs;
                if (allVerbs != null)
                {
                    List<VerbEntry> usableVerbs = new List<VerbEntry>();
                    for (int k = allVerbs.Count; k-- > 0;)
                    {
                        Verb v = allVerbs[k];
                        if (v.IsStillUsableBy(instance)) usableVerbs.Add(new VerbEntry(v, instance, allVerbs, 1));
                    }
                    if (usableVerbs.TryRandomElementByWeight(ve => ve.GetSelectionWeight(target), out VerbEntry result))
                    {
                        verb = result.verb;
                    }
                }
            }
            return verb != null;
        }
        
        public static bool TryStartBothAttacks(Pawn instance, LocalTargetInfo targ)
        {
            Log.Message("TryStartBothAttacks");
            bool attack_one = instance.TryStartAttack(targ);
            bool attack_two = DualWieldUtility.TryStartOffHandAttack(instance, targ);
            return attack_one || attack_two;
        }

        public static bool CanAttackOffhand(Pawn pawn)
        {
            if (pawn.equipment == null || !pawn.GetOffHander(out ThingWithComps offHandEquip))
            {
                return false;
            }
            
            var offHandStance = pawn.GetOffHandStance();
            if (offHandStance is Stance_Warmup_DW || offHandStance is Stance_Cooldown || pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return false;
            }

            return true;
        }

        public static bool CanAttackMainHand(Pawn pawn)
        {
            // Copied from original FulLBodyBusy code
            return pawn.stances.stunner.Stunned || pawn.stances.curStance.StanceBusy;
        }

        public static bool CanAttackAnyHand(Pawn pawn)
        {
            return DualWieldUtility.CanAttackOffhand(pawn) || DualWieldUtility.CanAttackMainHand(pawn);
        }

        public static bool CantAttackBothHands(Pawn pawn)
        {
            return !DualWieldUtility.CanAttackOffhand(pawn) && !DualWieldUtility.CanAttackMainHand(pawn);
        }
    }
}