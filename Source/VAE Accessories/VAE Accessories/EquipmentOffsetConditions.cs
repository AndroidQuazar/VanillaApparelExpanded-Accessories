using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VAE_Accessories
{
    public class EquipmentOffsetConditions : DefModExtension
    {
        public List<TechLevel> techLevels;

        public bool IsValid(Pawn pawn, ThingDef apparelDef)
        {
            bool weaponType = pawn.equipment?.Primary?.def.Verbs?.Any(v => v.verbClass == typeof(Verb_Shoot) || v.verbClass.IsSubclassOf(typeof(Verb_Shoot))) ?? false;
            bool techLevel = techLevels?.Contains(apparelDef.techLevel) ?? false;
            Log.Message($"IsValid: {weaponType} | {techLevel}");
            return weaponType && techLevel;
        }
    }
}
