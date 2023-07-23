using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Bloody_Mess
{
	[DefOf]
	public static class BloodyExplosion
	{
		public static DamageDef TD_BloodSplatterDamage;
		public static ThingDef TD_ProjectileBlood;

		[DebugAction("General", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DoBloodyExplosion(Pawn pawn)
		{
			Map map = pawn.Map;
			IntVec3 origin = pawn.Position;

			float explosionRadius = 2.5f;
			var damageDef = TD_BloodSplatterDamage;
			var soundDef = SoundDefOf.Hive_Spawn;
			var bloodFilthDef = ThingDefOf.Filth_Blood;
			var chance = 0.5f;
			var count = 2;
			var propagationSpeed = 0.25f;

			GenExplosion.DoExplosion(origin,
														map,
														explosionRadius,
														damageDef,
														null,//attacker?
														doVisualEffects: false,
														explosionSound: soundDef,
														propagationSpeed: propagationSpeed,
														preExplosionSpawnThingDef: bloodFilthDef,
														preExplosionSpawnChance: chance,
														preExplosionSpawnThingCount: count,
														postExplosionSpawnThingDef: bloodFilthDef,
														postExplosionSpawnChance: chance/2,
														postExplosionSpawnThingCount: count);


			for (int i = 0; i < 4; i++)
			{
				Projectile projectile = (Projectile)GenSpawn.Spawn(TD_ProjectileBlood, pawn.Position, map);

				IntVec3 targetPos = origin + GenRadial.RadialPattern[Rand.Range(GenRadial.NumCellsInRadius(explosionRadius*1.5f), GenRadial.NumCellsInRadius(explosionRadius * 2.5f))];
				projectile.Launch(pawn, pawn.DrawPos, targetPos, targetPos, ProjectileHitFlags.All);
			}
		}
	}

	public class ProjectileBlood : Projectile
	{
		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			Log.Message($"ProjectileBlood impacted ({hitThing}) at {Position}");
			FilthMaker.TryMakeFilth(Position, Map, ThingDefOf.Filth_Blood, 4);
			//todo: cover pawns in blood

			base.Impact(hitThing, blockedByShield);
		}
	}
}
