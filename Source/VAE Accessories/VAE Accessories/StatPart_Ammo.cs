using System;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace VAE_Accessories
{
    [StaticConstructorOnStartup]
    public class StatPart_Ammo : StatPart
    {
        private static StatDef statRangedCooldownFactor;

        static StatPart_Ammo()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
                statRangedCooldownFactor = StatDef.Named("VAEA_RangedCooldownFactor"));
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing.ParentHolder is Pawn_EquipmentTracker eq)) return;
            foreach (var thing in eq.AllEquipmentListForReading.Concat(eq.pawn.apparel.WornApparel))
                if (!(thing.def.GetModExtension<EquipmentOffsetConditions>() is EquipmentOffsetConditions conds) || conds.IsValid(req.Thing, thing.def))
                    val *= thing.GetStatValue(statRangedCooldownFactor);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing.ParentHolder is Pawn_EquipmentTracker eq))
                return "";
            var builder = new StringBuilder();
            foreach (var (thing, conds) in from apparel in eq.AllEquipmentListForReading.Concat(eq.pawn.apparel.WornApparel)
                let conds = apparel.def
                    .GetModExtension<EquipmentOffsetConditions>()
                where conds != null || Math.Abs(apparel.GetStatValue(statRangedCooldownFactor) - 1f) > 0.001f
                select (apparel, conds))
                builder.AppendLine(
                    conds.IsValid(req.Thing, thing.def)
                        ? $"{thing.def.LabelCap}: {statRangedCooldownFactor.Worker.ValueToString(thing.GetStatValue(statRangedCooldownFactor), true, ToStringNumberSense.Factor)}"
                        : $"{thing.def.LabelCap}: {req.Thing.def.LabelCap} {"VAEA.IsToo".Translate()} {(req.Thing.def.techLevel > conds.techLevels.Max() ? "VAEA.Advanced".Translate() : "VAEA.Primitive".Translate())}");
            return builder.ToString();
        }
    }
}