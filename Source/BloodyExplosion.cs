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

		const float explosionRadius = 2.5f;
		const float chance = 0.5f;
		const int count = 2;
		const float propagationSpeed = 0.25f;
		const int numProjectiles = 4;

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
														explosionSound: SoundDefOf.Hive_Spawn,
														propagationSpeed: propagationSpeed,
														preExplosionSpawnThingDef: ThingDefOf.Filth_Blood,
														preExplosionSpawnChance: chance,
														preExplosionSpawnThingCount: count,
														postExplosionSpawnThingDef: ThingDefOf.Filth_Blood,
														postExplosionSpawnChance: chance/2,
														postExplosionSpawnThingCount: count);


			for (int i = 0; i < numProjectiles; i++)
			{
				Projectile projectile = (Projectile)GenSpawn.Spawn(TD_ProjectileBlood, pawn.Position, map);

				IntVec3 targetPos = origin + GenRadial.RadialPattern[Rand.Range(GenRadial.NumCellsInRadius(explosionRadius*1.5f), GenRadial.NumCellsInRadius(explosionRadius * 2.5f))];
				projectile.Launch(pawn, pawn.DrawPos, targetPos, targetPos, ProjectileHitFlags.All);
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
			//todo: cover pawns in blood? heduff like a wound that shows bloody mark?

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
