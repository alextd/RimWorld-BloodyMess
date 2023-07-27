using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Bloody_Mess
{
	[DefOf]
	public static class BloodyExplosion
	{
		public static DamageDef TD_BloodSplatterDamage;
		public static ThingDef TD_ProjectileBlood;
		public static ThingDef TD_ProjectileMeat;

		const float explosionRadius = 2.5f;
		const float chance = 0.5f;
		const int count = 2;
		const float propagationSpeed = 0.25f;
		const int numProjectiles = 4;
		const int numProjectilesMeat = 1;

		static List<ThingDefCountClass> potentialProjectileDefs = new();

		[DebugAction("General", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DoBloodyExplosion(Pawn pawn)
		{
			Map map = pawn.Map;
			IntVec3 origin = pawn.Position;
			ThingDef bloodDef = pawn.RaceProps.BloodDef;

			GenExplosion.DoExplosion(origin,
														map,
														explosionRadius,
														TD_BloodSplatterDamage,
														null,//attacker?
														doVisualEffects: false,
														explosionSound: SoundDefOf.Hive_Spawn, // todo: maybe something from https://www.youtube.com/watch?v=vsF4BM1qhok
														propagationSpeed: propagationSpeed,
														preExplosionSpawnThingDef: bloodDef,
														preExplosionSpawnChance: chance,
														preExplosionSpawnThingCount: count,
														postExplosionSpawnThingDef: bloodDef,
														postExplosionSpawnChance: chance/2,
														postExplosionSpawnThingCount: count);


			IntVec3 startPos = pawn.Position;//seems meaningless to projectiles.
			Vector3 launchPos = pawn.DrawPos;
			for (int i = 0; i < numProjectiles; i++)
			{
				ProjectileBlood projectile = (ProjectileBlood)GenSpawn.Spawn(TD_ProjectileBlood, startPos, map);
				projectile.bloodDef = bloodDef;

				IntVec3 targetPos = origin + GenRadial.RadialPattern[Rand.Range(GenRadial.NumCellsInRadius(explosionRadius*1.5f), GenRadial.NumCellsInRadius(explosionRadius * 2.5f))];
				projectile.Launch(pawn, launchPos, targetPos, targetPos, ProjectileHitFlags.All);

				if(i < numProjectilesMeat)
				{
					ProjectileMeat projectileMeat = (ProjectileMeat)GenSpawn.Spawn(TD_ProjectileMeat, startPos, map);

					if (pawn.GetStatValue(StatDefOf.MeatAmount) is float meatCount && meatCount > 0)
					{
						potentialProjectileDefs.Add(new(pawn.def.race.meatDef, (int)meatCount));
					}

					if (pawn.GetStatValue(StatDefOf.LeatherAmount) is float leatherCount && leatherCount > 0)
					{
						potentialProjectileDefs.Add(new(pawn.def.race.leatherDef, (int)leatherCount));
					}

					if(pawn.def.butcherProducts != null)
					{
						potentialProjectileDefs.AddRange(pawn.def.butcherProducts);
					}

					if (!pawn.RaceProps.Humanlike)
					{
						// let's be thorough
						PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
						if (curKindLifeStage.butcherBodyPart != null &&
							pawn.health.hediffSet.GetNotMissingParts().Any(part => part.IsInGroup(curKindLifeStage.butcherBodyPart.bodyPartGroup)) &&
							((pawn.gender == Gender.Male && curKindLifeStage.butcherBodyPart.allowMale) ||
							(pawn.gender == Gender.Female && curKindLifeStage.butcherBodyPart.allowFemale)))
						{
							potentialProjectileDefs.Add(new ThingDefCountClass(curKindLifeStage.butcherBodyPart.thing, 1));
						}
					}

					Log.Message($"ProjectileMeat for {pawn} could launch {potentialProjectileDefs.ToStringSafeEnumerable()}");
					ThingDefCountClass theChosenMeat = potentialProjectileDefs.RandomElementByWeightWithFallback(o => o.count);
					potentialProjectileDefs.Clear();

					projectileMeat.SetMeat(theChosenMeat);

					Log.Message($"ProjectileMeat lauching ({theChosenMeat}) at {targetPos}");
					projectileMeat.Launch(pawn, launchPos, targetPos, targetPos, ProjectileHitFlags.None);
				}
			}
		}
	}

	public class ProjectileBlood : Projectile
	{
		public ThingDef bloodDef;

		//should be protected
		public override void Impact(Thing hitThing, bool blockedByShield = false)
		{
//			Log.Message($"ProjectileBlood impacted ({hitThing}) at {Position}");
			FilthMaker.TryMakeFilth(Position, Map, bloodDef, 4);
			//todo: cover pawns in blood? hediff like a wound that shows bloody mark?

			base.Impact(hitThing, blockedByShield);
		}
	}

	public class ProjectileMeat : Projectile
	{
		private ThingDef meatDef;
		private int meatCount;
		private Material meatMat;

		public void SetMeat(ThingDefCountClass defCount)
		{
			//probably never happens but let's be sure to get no nullrefs
			if (defCount == null)
				defCount = new(ThingDefOf.Gold, 100);

			meatDef = defCount.thingDef;
			meatCount = (defCount.count + 9) / 10;
			meatMat = meatDef.graphic is Graphic_StackCount gr
				? gr.SubGraphicForStackCount(meatCount, def).MatSingle
				: meatDef.DrawMatSingle;
		}

		public override Graphic Graphic
		{
			get
			{
				if (graphicInt == null)
				{
					if (meatDef.graphicData == null)
					{
						return BaseContent.BadGraphic;
					}
					graphicInt = meatDef.graphicData.GraphicColoredFor(this);
				}
				return graphicInt;
			}
		}

		public override void Draw()
		{
			//Same as root but meatDef
			float num = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction);
			Vector3 drawPos = DrawPos;
			Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
			if (def.projectile.shadowSize > 0f)
			{
				DrawShadow(drawPos, num);
			}

			Graphics.DrawMesh(MeshPool.GridPlane(meatDef.graphicData.drawSize), position, ExactRotation, meatMat, 0);
			Comps_PostDraw();
		}

		//should be protected
		public override void Impact(Thing hitThing, bool blockedByShield = false)
		{
//			Log.Message($"ProjectileMeat impacted ({hitThing}) at {Position}");
			Thing impactMeat = ThingMaker.MakeThing(meatDef);
			impactMeat.stackCount = meatCount;
			GenSpawn.Spawn(impactMeat, Position, Map);
			//todo: cover pawns in blood? hediff like a wound that shows bloody mark?

			base.Impact(hitThing, blockedByShield);
		}
	}

	public class DamageWorker_BloodSplatter : DamageWorker
	{
		public override void ExplosionAffectCell(Explosion explosion, IntVec3 cell, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
		{
			//watered down DamageWorker because ThrowExplosionCell also throws dust.

			// FleckMaker.ThrowExplosionCell(c, explosion.Map, def.explosionCellFleck, color);
			// public static void ThrowExplosionCell(IntVec3 cell, Map map, FleckDef fleckDef, Color color)public static void ThrowExplosionCell(IntVec3 cell, Map map, FleckDef fleckDef, Color color)

			Map map = explosion.Map;
			if (cell.ShouldSpawnMotesAt(map))
			{
				float t = Mathf.Clamp01((explosion.Position - cell).LengthHorizontal / explosion.radius);
				Color color = Color.Lerp(def.explosionColorCenter, def.explosionColorEdge, t);
				FleckCreationData dataStatic = FleckMaker.GetDataStatic(cell.ToVector3Shifted(), map, def.explosionCellFleck);
				dataStatic.rotation = 90 * Rand.RangeInclusive(0, 3);
				dataStatic.instanceColor = color;
				map.flecks.CreateFleck(dataStatic);
				/*
				if (Rand.Value < 0.7f)
				{
					ThrowDustPuff(cell, map, 1.2f);
				}
				*/
		}
	}
	}

}
