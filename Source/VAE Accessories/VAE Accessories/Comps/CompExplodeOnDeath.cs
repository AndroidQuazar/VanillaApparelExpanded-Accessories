using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace VAE_Accessories
{
    public class CompExplodeOnDeath : ThingComp
    {
        public CompProperties_ExplodeOnDeath Props => props as CompProperties_ExplodeOnDeath;

        public void ExplodeOnDeath(Pawn pawn)
        {
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, Props.explosionRadius, Props.damageDef, pawn);
        }
    }
}
