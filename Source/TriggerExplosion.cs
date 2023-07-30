using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Bloody_Mess
{

	[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
	public static class TriggerBloodyMessPatch
	{
		//public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo SetDeadInfo = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.SetDead));

			foreach (var inst in instructions)
			{
				yield return inst;

				if(inst.Calls(SetDeadInfo))
				{
					yield return new(OpCodes.Ldarg_0);//Pawn this
					yield return new(OpCodes.Ldarg_1);//DamageInfo? dinfo
					yield return new(OpCodes.Call, AccessTools.Method(typeof(TriggerBloodyMessPatch), nameof(TriggerBloodyMess)));//TriggerBloodyMess(this, dinfo)
				}
			}
		}
		public static void TriggerBloodyMess(Pawn target, DamageInfo? dinfo)
		{
			bool doMess = false;

			if (Rand.Chance(Mod.settings.alwaysBloodyMess))
				//easy
				doMess = true;
			else
			{
				Pawn instigator = null;
				bool explosion = false;
				if (dinfo.HasValue && dinfo.Value.Instigator is Pawn pawn)
				{
					// should needed info from damageinfo...
					instigator = pawn;
					explosion = dinfo.Value.Def.isExplosive;
				}
				else
				{
					// Check explosions because an explosion can cause hediffs that kill the target without passing along the DamageInfo
					foreach (Explosion exp in target.Map.listerThings.ThingsOfDef(ThingDefOf.Explosion))
						if (exp.damagedThings.Contains(target))
						{
							instigator = exp.instigator as Pawn;
							explosion = true;
							break;
						}
				}
				if (
					(explosion && Rand.Chance(Mod.settings.allExplosionsBloodyMess)) ||
					(instigator?.story?.traits?.HasTrait(BloodyTrait.TD_BloodyMess) ?? false))
				{
					doMess = true;
				}
			}
			if (doMess)
			{
				BloodyExplosion.DoBloodyExplosion(target);
				BloodyDestroyPart(target);
			}

		}

		public static void BloodyDestroyPart(Pawn pawn)
		{
			//Find major body part
			if(pawn.health.hediffSet.GetRandomNotMissingPart(null, depth: BodyPartDepth.Outside, partParent:pawn.def.race.body.corePart)
				is BodyPartRecord part)
			{
				Log.Message($"Bloody Mess destroying {part} for {pawn}");
				pawn.health.AddHediff(HediffDefOf.MissingBodyPart, part);
			}
		}
	}
}
