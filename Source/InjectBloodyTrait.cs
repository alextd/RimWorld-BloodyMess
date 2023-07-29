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

	[DefOf]
	[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GenerateTraits))]
	public static class BloodyTrait
	{
		public static TraitDef TD_BloodyMess;
		//private static void GenerateTraits(Pawn pawn, PawnGenerationRequest request)
		public static void Postfix(Pawn pawn)
		{
			if(Rand.Chance(Mod.settings.traitChance))
			{
				Trait trait = new(TD_BloodyMess);
				pawn.story.traits.GainTrait(trait);
			}
		}
	}
}
