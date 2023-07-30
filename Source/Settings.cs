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
		public float alwaysBloodyMess = 0;
		public float allExplosionsBloodyMess = 0;
		public float meatPercent = 0.1f;

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.Begin(wrect);

			options.SliderLabeled("The chance that any new person gets the Bloody Mess trait", ref traitChance, "{0:P0}");
			options.SliderLabeled("Also just randomly cause a bloody mess on any kill", ref alwaysBloodyMess, "{0:P0}");
			options.SliderLabeled("Randomly cause a bloody mess on any explosive kill", ref allExplosionsBloodyMess, "{0:P0}");
			options.SliderLabeled("Amount of meat thrown from the bloody mess", ref meatPercent, "{0:P0}");

			options.End();
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref traitChance, "traitChance", 0.05f);
			Scribe_Values.Look(ref alwaysBloodyMess, "alwaysBloodyMess", 0);
			Scribe_Values.Look(ref allExplosionsBloodyMess, "allExplosionsBloodyMess", 0);
			Scribe_Values.Look(ref meatPercent, "meatPercent", 0.1f);
		}
	}
}
