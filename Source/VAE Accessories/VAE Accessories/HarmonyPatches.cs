using System.Linq;
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

        public static void ApparelMassCapacity(Pawn p, StringBuilder explanation, ref float __result)
        {
            if (!(p?.apparel?.WornApparel.NullOrEmpty() ?? true) &&
                p.apparel.WornApparel.FirstOrDefault(a => a.def is CaravanCapacityApparelDef)?.def is
                    CaravanCapacityApparelDef def) __result += def.carryingCapacity;
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