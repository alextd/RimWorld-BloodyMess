using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using TD.Utilities;

namespace Bloody_Mess
{
	public class Settings : ModSettings
	{
		public float traitChance = 0.05f;

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.Begin(wrect);

			options.SliderLabeled("The chance that any new person gets the Blood Mess trait", ref traitChance, "{0:P0}");

			options.End();
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref traitChance, "traitChance", 0.05f);
		}
	}
}
