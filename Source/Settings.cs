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
		public bool clean = false;

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.Begin(wrect);

			options.SliderLabeled("TD.TheChanceThatAnyNewPersonGetsTheBloodyMessTrait".Translate(), ref traitChance, "{0:P0}");
			options.SliderLabeled("TD.AlsoJustRandomlyCauseABloodyMessOnAnyKill".Translate(), ref alwaysBloodyMess, "{0:P0}");
			options.SliderLabeled("TD.RandomlyCauseABloodyMessOnAnyExplosiveKill".Translate(), ref allExplosionsBloodyMess, "{0:P0}");
			options.SliderLabeled("TD.AmountOfMeatThrownFromTheBloodyMess".Translate(), ref meatPercent, "{0:P0}");
			options.CheckboxLabeled("TD.NoActualMessJustGraphics".Translate(), ref clean);

			options.End();
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref traitChance, "traitChance", 0.05f);
			Scribe_Values.Look(ref alwaysBloodyMess, "alwaysBloodyMess", 0);
			Scribe_Values.Look(ref allExplosionsBloodyMess, "allExplosionsBloodyMess", 0);
			Scribe_Values.Look(ref meatPercent, "meatPercent", 0.1f);
			Scribe_Values.Look(ref clean, "meatPecleanrcent", false);
		}
	}
}
