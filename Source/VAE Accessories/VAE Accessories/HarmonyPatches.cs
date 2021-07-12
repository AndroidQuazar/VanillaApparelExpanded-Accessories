using System;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VAE_Accessories
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("VanillaApparelExpanded.Accessories");

            harmony.Patch(AccessTools.Method(typeof(MassUtility), nameof(MassUtility.Capacity)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches),
                    nameof(ApparelMassCapacity)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)),
                new HarmonyMethod(typeof(HarmonyPatches),
                    nameof(BeltsOnDeath)));
        }

        private static float GetMutiplierForQuality(QualityCategory cat)
        {
            switch (cat)
            {
                case QualityCategory.Awful:
                    return 0.5f;
                case QualityCategory.Poor:
                    return 0.8f;
                case QualityCategory.Normal:
                    return 1f;
                case QualityCategory.Good:
                    return 1.2f;
                case QualityCategory.Excellent:
                    return 1.5f;
                case QualityCategory.Masterwork:
                    return 1.7f;
                case QualityCategory.Legendary:
                    return 2f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cat), cat, null);
            }
        }

        public static void ApparelMassCapacity(Pawn p, StringBuilder explanation, ref float __result)
        {
            if (p?.apparel?.WornApparel.NullOrEmpty() ?? true) return;
            foreach (var apparel in p.apparel.WornApparel)
                if (apparel.def is CaravanCapacityApparelDef def)
                {
                    if (apparel.TryGetQuality(out var cat))
                    {
                        __result += def.carryingCapacity * GetMutiplierForQuality(cat);
                        explanation?.AppendLine(
                            $"{apparel.LabelCapNoCount}: +{def.carryingCapacity} * {GetMutiplierForQuality(cat)} ({cat.GetLabel()})");
                    }
                    else
                    {
                        __result += def.carryingCapacity;
                        explanation?.AppendLine($"{apparel.LabelCapNoCount}: +{def.carryingCapacity}");
                    }
                }
        }

        public static bool BeltsOnDeath(DamageInfo? dinfo, Hediff exactCulprit, Pawn __instance)
        {
            if (__instance?.apparel?.WornApparel.NullOrEmpty() ?? true) return true;
            for (var i = __instance.apparel.WornApparel.Count - 1; i >= 0; --i)
            {
                var apparel = __instance.apparel.WornApparel[i];
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

            return true;
        }
    }
}