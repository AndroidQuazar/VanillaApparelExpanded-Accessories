using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace VAE_Accessories
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("VanillaApparelExpanded.Accessories");

            harmony.Patch(original: AccessTools.Method(typeof(MassUtility), nameof(MassUtility.Capacity)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches),
                nameof(ApparelMassCapacity)));
            harmony.Patch(original: AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches),
                nameof(BeltsOnDeath)));
            harmony.Patch(original: AccessTools.Method(typeof(StatWorker), "StatOffsetFromGear"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches),
                nameof(ValidateEquipmentOffset)));
        }

        public static void ApparelMassCapacity(Pawn p, StringBuilder explanation, ref float __result)
        {
            if (!p.apparel.WornApparel.NullOrEmpty() && p.apparel.WornApparel.FirstOrDefault(a => a.def is CaravanCapacityApparelDef)?.def is CaravanCapacityApparelDef def)
            {
                __result += def.carryingCapacity;
            }
        }

        public static bool BeltsOnDeath(DamageInfo? dinfo, Hediff exactCulprit, Pawn __instance)
        {
            if (!__instance.apparel.WornApparel.NullOrEmpty())
            {
                for (int i = __instance.apparel.WornApparel.Count - 1; i >= 0; --i)
                {
                    Apparel apparel = __instance.apparel.WornApparel[i];
                    if (apparel.TryGetComp<CompExplodeOnDeath>() is CompExplodeOnDeath comp)
                    {
                        comp.ExplodeOnDeath(__instance);
                    }
                    else if (apparel is ResurrectorBelt)
                    {
                        UtilityMethods.ResurrectBeforeDeath(__instance);
                        __instance.apparel.Remove(apparel);
                        return false;
                    }
                }
            }
            return true;
        }

        public static void ValidateEquipmentOffset(Thing gear, StatDef stat, ref float __result)
        {
            if (gear.ParentHolder is Pawn pawn)
            {
                //gear.def.GetModExtension<EquipmentOffsetConditions>() is EquipmentOffsetConditions conditions
                //Log.Message($"Checking {gear.Label}");
                //if (!conditions.IsValid(pawn, gear.def))
                //{
                //    __result = 0;
                //}
            }
        }
    }
}
