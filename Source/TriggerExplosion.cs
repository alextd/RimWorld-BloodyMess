using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Bloody_Mess
{

	[HarmonyPatch(typeof(Pawn), nameof(Pawn.DoKillSideEffects))]
	public static class NewFeature
	{
		//private void DoKillSideEffects(DamageInfo? dinfo, Hediff exactCulprit, bool spawned)
		public static void Postfix(Pawn __instance, DamageInfo? dinfo)
		{
			if (dinfo.HasValue && dinfo.Value.Instigator is Pawn pawn
				//Todo: pawn trait is bloody mess, but for now, all player colonists 
				&& pawn.Faction == Faction.OfPlayer && Rand.Chance(0.5f))
				BloodyExplosion.DoBloodyExplosion(__instance);
		}
	}
	class TriggerExplosion
	{
	}
}
