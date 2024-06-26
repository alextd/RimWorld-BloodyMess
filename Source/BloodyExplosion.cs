﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using LudeonTK;

namespace Bloody_Mess
{
	[DefOf]
	public static class BloodyExplosion
	{
		public static DamageDef TD_BloodSplatterDamage;
		public static ThingDef TD_ProjectileBlood;
		public static ThingDef TD_ProjectileMeat;
		public static SoundDef Explosion_Rocket;
		public static SoundDef Hive_Spawn;

		const float explosionRadius = 2.5f;
		const float filthChanceBase = 0.5f;
		const int filthCount = 2;
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
			SoundDef soundDef = pawn.RaceProps.IsFlesh ? Hive_Spawn : Explosion_Rocket;
			float filthChance = Mod.settings.clean ? 0 : filthChanceBase;

				GenExplosion.DoExplosion(origin,
														map,
														explosionRadius,
														TD_BloodSplatterDamage,
														null,//attacker?
														doVisualEffects: false,
														explosionSound: soundDef, // todo: maybe something from https://www.youtube.com/watch?v=vsF4BM1qhok
														propagationSpeed: propagationSpeed,
														preExplosionSpawnThingDef: bloodDef,
														preExplosionSpawnChance: filthChance,
														preExplosionSpawnThingCount: filthCount,
														postExplosionSpawnThingDef: bloodDef,
														postExplosionSpawnChance: filthChance/2,
														postExplosionSpawnThingCount: filthCount);


			IntVec3 startPos = pawn.Position;//seems meaningless to projectiles.
			Vector3 launchPos = pawn.DrawPos;
			for (int i = 0; i < numProjectiles * (Mod.settings.clean ? 3 : 1); i++)
			{
				ProjectileItem projectileBlood = (ProjectileItem)GenSpawn.Spawn(TD_ProjectileBlood, startPos, map);
				projectileBlood.SetItem(new(bloodDef, 4));

				IntVec3 targetPos = origin + GenRadial.RadialPattern[Rand.Range(GenRadial.NumCellsInRadius(explosionRadius*(Mod.settings.clean?0.5f:1.5f)), GenRadial.NumCellsInRadius(explosionRadius * 2.5f))];
				projectileBlood.Launch(pawn, launchPos, targetPos, targetPos, ProjectileHitFlags.All);

				if(i < numProjectilesMeat)
				{
					ProjectileItem projectileMeat = (ProjectileItem)GenSpawn.Spawn(TD_ProjectileMeat, startPos, map);

					// Find what to launch based on butcher products:
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
						potentialProjectileDefs.AddRange(pawn.def.butcherProducts.Select(dc => new ThingDefCountClass(dc.thingDef, dc.count)));
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

					// Decide on The Chosen Meat
					Log.Message($"ProjectileMeat for {pawn} could launch {potentialProjectileDefs.ToStringSafeEnumerable()}");
					ThingDefCountClass theChosenMeat = potentialProjectileDefs.RandomElementByWeightWithFallback(o => o.count);
					potentialProjectileDefs.Clear();

					projectileMeat.SetItem(theChosenMeat);

					Log.Message($"ProjectileMeat launching ({theChosenMeat}) at {targetPos}");
					projectileMeat.Launch(pawn, launchPos, targetPos, targetPos, ProjectileHitFlags.All);
				}
			}
		}
	}

	public class ProjectileItem : Projectile
	{
		private ThingDef itemDef;
		private int itemCount;
		private Material itemMat;
		private Mesh mesh;
		private float rotSpeed;

		public void SetItem(ThingDefCountClass defCount)
		{
			//probably never happens but let's be sure to get no nullrefs
			if (defCount == null)
				defCount = new(ThingDefOf.Gold, 100);

			mesh = MeshPool.GridPlane(new Vector2(def.size.x, def.size.z));
			itemDef = defCount.thingDef;
			itemCount = Mathf.CeilToInt(defCount.count * Mod.settings.meatPercent);
			itemMat = itemDef.graphic is Graphic_StackCount gr
				? gr.SubGraphicForStackCount(itemCount, def).MatSingle
				: itemDef.DrawMatSingle;

			rotSpeed = Rand.Range(180, 720);
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			//Same as root but meatDef's graphicData.
			float num = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction);
			Vector3 drawPos = drawLoc;
			Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
			if (def.projectile.shadowSize > 0f)
			{
				DrawShadow(drawPos, num);
			}
			Graphics.DrawMesh(mesh, position, Quaternion.AngleAxis(rotSpeed * DistanceCoveredFraction, Vector3.up), itemMat, 0);
			Comps_PostDraw();
		}

		//should be protected
		public override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			if (itemCount > 0)
			{
				// with 0: Just graphics, no spawn

				Log.Message($"ProjectileItem {itemDef} impacted ({hitThing}) at {Position}, HitFlags = {HitFlags}");
				if (itemDef.IsFilth)
				{
					if(!Mod.settings.clean)
						FilthMaker.TryMakeFilth(Position, Map, itemDef, itemCount);
				}
				else
				{
					Thing impactItem = ThingMaker.MakeThing(itemDef);
					impactItem.stackCount = itemCount;
					if (!GenPlace.TryPlaceThing(impactItem, Position, Map, ThingPlaceMode.Near))
						impactItem.Destroy();
				}
			}

			//todo: cover hitThing pawns in blood? hediff like a wound that shows bloody mark?
			//todo: damage pawns with item's blunt damage?
			base.Impact(hitThing, blockedByShield);
		}
	}

	public class DamageWorker_BloodSplatter : DamageWorker
	{
		public override void ExplosionAffectCell(Explosion explosion, IntVec3 cell, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
		{
			//watered down DamageWorker because ThrowExplosionCell also throws dust.

			// Also now that we're here, tweak the values.

			// FleckMaker.ThrowExplosionCell(c, explosion.Map, def.explosionCellFleck, color);
			// public static void ThrowExplosionCell(IntVec3 cell, Map map, FleckDef fleckDef, Color color)public static void ThrowExplosionCell(IntVec3 cell, Map map, FleckDef fleckDef, Color color)

			Map map = explosion.Map;
			if (cell.ShouldSpawnMotesAt(map))
			{
				float t = Mathf.Clamp01((explosion.Position - cell).LengthHorizontal / explosion.radius);
				Color color = explosion.preExplosionSpawnThingDef == ThingDefOf.Filth_MachineBits ? Color.grey :
					explosion.preExplosionSpawnThingDef.graphicData.color;
				color.a = 1 - t;

				FleckCreationData dataStatic = FleckMaker.GetDataStatic(cell.ToVector3Shifted(), map, def.explosionCellFleck);
				dataStatic.rotation = Rand.Range(0, 360);
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


	// Make ProjectileItem auto-pass InterceptChanceFactorFromDistance so walls intercept projectiles at close range
	[HarmonyPatch(typeof(Projectile), nameof(Projectile.CheckForFreeIntercept))]
	public static class CheckForFreeInterceptPass
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo InterceptChanceFactorFromDistanceInfo = AccessTools.Method("VerbUtility:InterceptChanceFactorFromDistance");

			foreach (var inst in instructions)
			{
				yield return inst;

				// One might ideally skip the call but it's easier to just replace the value on the stack.
				if(inst.Calls(InterceptChanceFactorFromDistanceInfo))
				{
					// on stack: float from InterceptChanceFactorFromDistanceInfo, ready for stloc.0
					yield return new(OpCodes.Ldarg_0);//Projectile this
					yield return new(OpCodes.Call, AccessTools.Method(typeof(CheckForFreeInterceptPass), nameof(OneIfProjectileItem))); // OneIfProjectileItem(ret, this)
				}
			}
		}

		public static float OneIfProjectileItem(float value, Projectile projectile)
		{
			if (projectile is ProjectileItem)
				return 1;
			return value;
		}
	}


	[HarmonyPatch(typeof(Projectile), nameof(Projectile.CheckForFreeInterceptBetween))]
	public static class CheckForFreeInterceptBetweenPass
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
			CheckForFreeInterceptPass.Transpiler(instructions);
	}

}
