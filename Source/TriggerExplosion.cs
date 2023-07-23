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
			if (dinfo.HasValue && dinfo.Value.Instigator is Pawn pawn && (pawn.story?.traits?.HasTrait(BloodyTrait.TD_BloodyMess) ?? false))
				BloodyExplosion.DoBloodyExplosion(__instance);
		}
	}
	class TriggerExplosion
	{
	}
}
