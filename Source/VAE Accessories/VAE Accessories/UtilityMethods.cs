using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;

namespace VAE_Accessories
{
    public static class UtilityMethods
    {
        public static void ResurrectBeforeDeath(Pawn pawn)
        {
			pawn.health.Notify_Resurrected();
			if (pawn.Faction != null && pawn.Faction.IsPlayer)
			{
				if (pawn.workSettings != null)
				{
					pawn.workSettings.EnableAndInitialize();
				}
				Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);
			}
			for (int i = 0; i < 10; i++)
			{
				MoteMaker.ThrowAirPuffUp(pawn.DrawPos, pawn.Map);
			}
			if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
			{
				LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction, true, true, false, false, true), pawn.Map, Gen.YieldSingle(pawn));
			}
			if (pawn.apparel != null)
			{
				List<Apparel> wornApparel = pawn.apparel.WornApparel;
				for (int j = 0; j < wornApparel.Count; j++)
				{
					wornApparel[j].Notify_PawnResurrected();
				}
			}
			PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(pawn);
			if (pawn.royalty != null)
			{
				pawn.royalty.Notify_Resurrected();
			}
        }
    }
}
