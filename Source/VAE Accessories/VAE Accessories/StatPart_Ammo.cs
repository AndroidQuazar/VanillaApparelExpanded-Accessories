using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace VAE_Accessories
{
    [StaticConstructorOnStartup]
    public class StatPart_Ammo : StatPart
    {
        private static StatDef RangedCooldownFactor;

        static StatPart_Ammo()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
                RangedCooldownFactor = StatDef.Named("VAEA_RangedCooldownFactor"));
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing.ParentHolder is Pawn_EquipmentTracker eq) ||
                eq.pawn?.apparel == null) return;
            foreach (var apparel in eq.pawn.apparel.WornApparel)
                if (apparel.def.GetModExtension<EquipmentOffsetConditions>() is EquipmentOffsetConditions conds &&
                    conds.IsValid(eq.pawn, apparel.def))
                    val *= apparel.GetStatValue(RangedCooldownFactor);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing.ParentHolder is Pawn_EquipmentTracker eq) || eq.pawn?.apparel == null)
                return "";
            var builder = new StringBuilder();
            foreach (var (apparel, conds) in from apparel in eq.pawn.apparel.WornApparel
                let conds = apparel.def
                    .GetModExtension<EquipmentOffsetConditions>()
                where conds != null
                select (apparel, conds))
                builder.AppendLine(
                    conds.IsValid(req.Thing, apparel.def)
                        ? $"{apparel.def.LabelCap}: {RangedCooldownFactor.Worker.ValueToString(apparel.GetStatValue(RangedCooldownFactor), true, ToStringNumberSense.Factor)}"
                        : $"{apparel.def.LabelCap}: {req.Thing.def.LabelCap} {"VAEA.IsToo".Translate()} {(req.Thing.def.techLevel > conds.techLevels.Max() ? "VAEA.Advanced".Translate() : "VAEA.Primitive".Translate())}");
            return builder.ToString();
        }
    }
}