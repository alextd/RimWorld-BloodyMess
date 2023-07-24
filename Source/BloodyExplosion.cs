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

		[DebugAction("General", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DoBloodyExplosion(Pawn pawn)
		{
			Map map = pawn.Map;
			IntVec3 origin = pawn.Position;

			GenExplosion.DoExplosion(origin,
														map,
														explosionRadius,
														TD_BloodSplatterDamage,
														null,//attacker?
														doVisualEffects: false,
														explosionSound: SoundDefOf.Hive_Spawn, // todo: maybe something from https://www.youtube.com/watch?v=vsF4BM1qhok
														propagationSpeed: propagationSpeed,
														preExplosionSpawnThingDef: ThingDefOf.Filth_Blood,
														preExplosionSpawnChance: chance,
														preExplosionSpawnThingCount: count,
														postExplosionSpawnThingDef: ThingDefOf.Filth_Blood,
														postExplosionSpawnChance: chance/2,
														postExplosionSpawnThingCount: count);


			IntVec3 startPos = pawn.Position;//seems meaningless to projectiles.
			Vector3 launchPos = pawn.DrawPos;
			for (int i = 0; i < numProjectiles; i++)
			{
				Projectile projectile = (Projectile)GenSpawn.Spawn(TD_ProjectileBlood, startPos, map);

				IntVec3 targetPos = origin + GenRadial.RadialPattern[Rand.Range(GenRadial.NumCellsInRadius(explosionRadius*1.5f), GenRadial.NumCellsInRadius(explosionRadius * 2.5f))];
				projectile.Launch(pawn, launchPos, targetPos, targetPos, ProjectileHitFlags.All);

				if(i < numProjectilesMeat)
				{
					ProjectileMeat projectileMeat = (ProjectileMeat)GenSpawn.Spawn(TD_ProjectileMeat, startPos, map);
					projectileMeat.projectileMeat.thingDef = pawn.def.race.meatDef;
					projectileMeat.projectileMeat.count = (int)(pawn.GetStatValue(StatDefOf.MeatAmount) / 10);

					projectileMeat.Launch(pawn, launchPos, targetPos, targetPos, ProjectileHitFlags.None);
				}
			}
		}
	}

	public class ProjectileBlood : Projectile
	{
		//should be protected
		public override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			Log.Message($"ProjectileBlood impacted ({hitThing}) at {Position}");
			FilthMaker.TryMakeFilth(Position, Map, ThingDefOf.Filth_Blood, 4);
			//todo: cover pawns in blood? hediff like a wound that shows bloody mark?

			base.Impact(hitThing, blockedByShield);
		}
	}

	public class ProjectileMeat : Projectile
	{
		public ThingDefCount projectileMeat;

		public override Graphic Graphic
		{
			get
			{
				if (graphicInt == null)
				{
					if (projectileMeat.thingDef.graphicData == null)
					{
						return BaseContent.BadGraphic;
					}
					graphicInt = projectileMeat.thingDef.graphicData.GraphicColoredFor(this);
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

			Material meatMat = projectileMeat.thingDef.graphic is Graphic_StackCount gr 
				? gr.SubGraphicForStackCount(projectileMeat.count, projectileMeat.thingDef).MatSingle
				: projectileMeat.thingDef.DrawMatSingle;

			Graphics.DrawMesh(MeshPool.GridPlane(projectileMeat.thingDef.graphicData.drawSize), position, ExactRotation, meatMat, 0);
			Comps_PostDraw();
		}

		//should be protected
		public override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			Log.Message($"ProjectileMeat impacted ({hitThing}) at {Position}");
			/*
			 * todo: use random butcher product
					foreach (Thing item in thing3.ButcherProducts(worker, efficiency))
			*/
			Thing impactMeat = ThingMaker.MakeThing(projectileMeat.thingDef);
			impactMeat.stackCount = projectileMeat.count;
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
