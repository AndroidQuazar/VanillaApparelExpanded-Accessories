using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VAE_Accessories
{
    public class EquipmentOffsetConditions : DefModExtension
    {
        public List<TechLevel> techLevels;

        public bool IsValid(Thing weapon, ThingDef apparelDef)
        {
            var weaponType = weapon.def.Verbs?.Any(v =>
                v.verbClass == typeof(Verb_Shoot) || v.verbClass.IsSubclassOf(typeof(Verb_Shoot))) ?? false;
            Log.Message(
                $"TechLevel:\n  Weapon: {weapon.def.techLevel}\n  Valid:{techLevels?.Join(tech => tech.ToStringHuman(), "\n  ")}");
            var techLevel = techLevels?.Contains(weapon.def.techLevel) ?? false;
            Log.Message($"IsValid: {weaponType} | {techLevel}");
            return weaponType && techLevel;
        }
    }
}