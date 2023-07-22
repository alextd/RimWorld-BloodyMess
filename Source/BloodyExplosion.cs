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
		public static DamageDef TD_BloodSplatter;
		[DebugAction("General", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DoBloodyExplosion()
		{
			float radius = 3f;
			var damageDef = TD_BloodSplatter;
			var thingDef = ThingDefOf.Filth_Blood;
			var soundDef = SoundDefOf.Hive_Spawn;
			var thingChance = 0.5f;
			var thingCount = 4;
			var propagationSpeed = 0.25f;

			GenExplosion.DoExplosion(UI.MouseCell(),
														Find.CurrentMap,
														radius,
														damageDef,//BloodSplatter
														null,//attacker?
														doVisualEffects: false,
														explosionSound: soundDef,
														propagationSpeed: propagationSpeed,
														preExplosionSpawnThingDef: thingDef,
														preExplosionSpawnChance: thingChance,
														preExplosionSpawnThingCount: thingCount,
														postExplosionSpawnThingDef: thingDef,
														postExplosionSpawnChance: thingChance/4,
														postExplosionSpawnThingCount: thingCount/4);
		}
	}
}
