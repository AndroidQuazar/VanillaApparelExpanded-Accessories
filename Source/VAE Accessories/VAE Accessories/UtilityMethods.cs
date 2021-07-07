using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VAE_Accessories
{
    public static class UtilityMethods
    {
        public static void ResurrectBeforeDeath(Pawn pawn)
        {
            pawn.health.Notify_Resurrected();
            if (pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                if (pawn.workSettings != null) pawn.workSettings.EnableAndInitialize();
                Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);
            }

            for (var i = 0; i < 10; i++) FleckMaker.ThrowAirPuffUp(pawn.DrawPos, pawn.Map);
            if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
                LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction), pawn.Map,
                    Gen.YieldSingle(pawn));
            if (pawn.apparel != null)
            {
                var wornApparel = pawn.apparel.WornApparel;
                for (var j = 0; j < wornApparel.Count; j++) wornApparel[j].Notify_PawnResurrected();
            }

            PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(pawn);
            if (pawn.royalty != null) pawn.royalty.Notify_Resurrected();
        }
    }
}