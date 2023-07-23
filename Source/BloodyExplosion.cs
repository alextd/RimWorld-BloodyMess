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

		const float explosionRadius = 2.5f;
		static readonly DamageDef damageDef = TD_BloodSplatterDamage;
		static readonly SoundDef soundDef = SoundDefOf.Hive_Spawn;
		static readonly ThingDef bloodFilthDef = ThingDefOf.Filth_Blood;
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
			//todo: cover pawns in blood

			base.Impact(hitThing, blockedByShield);
		}
	}
}
