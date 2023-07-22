using System.Reflection;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace Bloody_Mess
{
	public class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{
#if DEBUG
			Harmony.DEBUG = true;
#endif

			Harmony harmony = new("Uuugggg.rimworld.Bloody_Mess.main");	
			harmony.PatchAll();
		}
	}
}