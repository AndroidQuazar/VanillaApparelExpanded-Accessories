using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VAE_Accessories
{
    [StaticConstructorOnStartup]
    public class RangedShieldBelt : Apparel
    {
        private const float MinDrawSize = 1.2f;
        private const float MaxDrawSize = 1.55f;
        private const float MaxDamagedJitterDist = 0.05f;
        private const int JitterDurationTicks = 8;
        private const int StartingTicksToReset = 3200;
        private const float EnergyOnReset = 0.2f;
        private const float EnergyLossPerDamage = 0.033f;
        private const int KeepDisplayingTicks = 1000;
        private const float ApparelScorePerEnergyMax = 0.25f;

        private static readonly Material BubbleMat =
            MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        private float energy;
        private Vector3 impactAngleVect;
        private int lastAbsorbDamageTick = -9999;
        private int lastKeepDisplayTick = -9999;
        private int ticksToReset = -1;

        private float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax);

        private float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

        public float Energy => energy;

        public ShieldState ShieldState
        {
            get
            {
                if (ticksToReset > 0) return ShieldState.Resetting;
                return ShieldState.Active;
            }
        }

        private bool ShouldDisplay
        {
            get
            {
                var wearer = Wearer;
                return wearer.Spawned && !wearer.Dead && !wearer.Downed &&
                       (wearer.InAggroMentalState || wearer.Drafted ||
                        wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner ||
                        Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
            Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (var gizmo in base.GetWornGizmos()) yield return gizmo;
            if (Find.Selector.SingleSelectedThing == Wearer)
                yield return new Gizmo_RangedShieldStatus
                {
                    shield = this
                };
        }

        public override float GetSpecialApparelScoreOffset()
        {
            return EnergyMax * ApparelScorePerEnergyMax;
        }

        public override void Tick()
        {
            base.Tick();
            if (Wearer == null)
            {
                energy = 0f;
                return;
            }

            if (ShieldState == ShieldState.Resetting)
            {
                ticksToReset--;
                if (ticksToReset <= 0) Reset();
            }
            else if (ShieldState == ShieldState.Active)
            {
                energy += EnergyGainPerTick;
                if (energy > EnergyMax) energy = EnergyMax;
            }
        }

        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (ShieldState != ShieldState.Active) return false;
            if (dinfo.Def == DamageDefOf.EMP)
            {
                energy = 0f;
                Break();
                return false;
            }

            if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
            {
                energy -= dinfo.Amount * EnergyLossPerDamage;
                if (energy < 0f)
                    Break();
                else
                    AbsorbedDamage(dinfo);
                return true;
            }

            return false;
        }

        public void KeepDisplaying()
        {
            lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            var loc = Wearer.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
            var num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, Wearer.Map, FleckDefOf.ExplosionFlash, num);
            var num2 = (int) num;
            for (var i = 0; i < num2; i++) FleckMaker.Static(loc, Wearer.Map, FleckDefOf.ExplosionFlash, num);
            lastAbsorbDamageTick = Find.TickManager.TicksGame;
            KeepDisplaying();
        }

        private void Break()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
            FleckMaker.Static(Wearer.TrueCenter(), Wearer.Map, FleckDefOf.ExplosionFlash, 12f);
            for (var i = 0; i < 6; i++)
                FleckMaker.ThrowDustPuff(
                    Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) *
                    Rand.Range(0.3f, 0.6f), Wearer.Map, Rand.Range(0.8f, 1.2f));
            energy = 0f;
            ticksToReset = StartingTicksToReset;
        }

        private void Reset()
        {
            if (Wearer.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
                FleckMaker.ThrowLightningGlow(Wearer.TrueCenter(), Wearer.Map, 3f);
            }

            ticksToReset = -1;
            energy = EnergyOnReset;
        }

        public override void DrawWornExtras()
        {
            if (ShieldState == ShieldState.Active && ShouldDisplay)
            {
                var num = Mathf.Lerp(MinDrawSize, MaxDrawSize, energy);
                var vector = Wearer.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                var num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (num2 < JitterDurationTicks)
                {
                    var num3 = (JitterDurationTicks - num2) / JitterDurationTicks * MaxDamagedJitterDist;
                    vector += impactAngleVect * num3;
                    num -= num3;
                }

                float angle = Rand.Range(0, 360);
                var s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default;
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }

        public override bool AllowVerbCast(Verb verb)
        {
            return true;
        }
    }
}