using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace VAE_Accessories
{
    public class CompProperties_ExplodeOnDeath : CompProperties
    {
        public float explosionRadius;
        public DamageDef damageDef;

        public CompProperties_ExplodeOnDeath()
        {
            compClass = typeof(CompExplodeOnDeath);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (damageDef is null)
            {
                yield return "<color=teal>damageDef</color> cannot be null.";
            }
            if (explosionRadius <= 0)
            {
                yield return "<color=teal>explosionRadius</color> must be larger than 0.";
            }
        }
    }
}
