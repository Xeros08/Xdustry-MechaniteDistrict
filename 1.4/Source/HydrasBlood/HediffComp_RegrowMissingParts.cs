using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;

namespace Xdustry.MechaniteDistict {
	// ============================ Regrow Missing BodyParts (Comp) ============================
	
	
	//* This is a compt that allows one to make a headiff able to start regrowing limbs
	public class HediffCompProperties_RegrowMissingParts : HediffCompProperties {
		public IntRange regrowCooldown;         // Time before starting to regrow another missing body part
		public HediffDef regrowthManagerDef;    // Hediff to substitute the lastInjury of the Hediff_MissingPart of a missing BodyPart

		//public bool regrowLimbs = true;
		//public List<BodyPartDef> limbList;
		public bool regrowOrgans = true;        // Allow the regrowth of organs
		public List<BodyPartDef> organList;     // List of BodyPartDefs to consider as organs

		public float painOffset = .05f;         // The painOffset to be added per Regrowing BodyPart
		public float painOffsetIfOrgan = .6f;   // The painOffset to be added if the BodyPart is an organ



		public HediffCompProperties_RegrowMissingParts() {
			compClass = typeof(HediffComp_RegrowMissingParts);
		}
	}


	//* This is the real COMP that does the behind the scenes magic
	public class HediffComp_RegrowMissingParts : HediffComp {
		//* Variables
		private int cooldownTimer;
		public HediffCompProperties_RegrowMissingParts Props {
			get {
				return (HediffCompProperties_RegrowMissingParts)props;
			}
		}

		private void ResetCooldownTimer() { cooldownTimer = Props.regrowCooldown.RandomInRange; }

		private byte TryRegrowRandomMissingBodyPart() {
			Hediff_MissingPart missing;
#if DEBUG
			Log.Message("Trying to regrow random missing BodyPart");
#endif
			//* Get random missing BodyPart (first parent present in BodyPart group hierarchy (IE: An arm => shoulder), or simply BodyParts (IE: Liver
			if (!Props.regrowOrgans) {
				missing = Pawn.health.hediffSet.GetMissingPartsCommonAncestors().Where(
					hediff => !Props.organList.Contains(hediff.Part.def)
					).RandomElement();

			} else missing = Pawn.health.hediffSet.GetMissingPartsCommonAncestors().RandomElement();

			//* Process the BodyPart
			if (missing != null && (missing.GetType() != typeof(Hediff_RegowingBodyPart))) {
#if DEBUG
				Log.Message($"Trying to regrow: \"{missing}\"");
				Log.Message($"BodyPart has depth: {missing.Part.depth}");
#endif
				var part = missing.Part;
				Pawn.health.RemoveHediff(missing);
				Pawn.health.AddHediff(Props.regrowthManagerDef, part);

				return 0; //* Exit code 0 means that the attempt was successfull
			} else {
#if DEBUG
				Log.Message("There are no missing BodyParts to be regrown");
#endif
				return 1; //* Exit code 1 means that there are no suitable missing BodyParts
			}
		}

		// Magic (please explain me)
		public override void CompPostMake() {
			base.CompPostMake();
			ResetCooldownTimer();
		}
		public override void CompPostTick(ref float severityAdjustment) {
			cooldownTimer--;
			if (cooldownTimer <= 0) {
				TryRegrowRandomMissingBodyPart();
				ResetCooldownTimer();
			}
		}
		public override void CompExposeData() {
			Scribe_Values.Look(ref cooldownTimer, "cooldownTimer");
		}
	}

}