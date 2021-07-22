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
	// ============================ BodyPart Regrowing ============================


	//* This is the Hediff that actually "regrows" the BodyParts (replaces the missing parts) => It's a glorified countdown timer
	public class Hediff_RegowingBodyPart : Hediff_MissingPart {
		public override string LabelBase { //TODO Make this safe to use (null checking)
			get { return $"{def.label}"; }
		}
		public override void PostMake() {
			base.PostMake(); //* Initialize comps
		}

		public override bool ShouldRemove {
			get {
				if (comps != null) {
					for (int i = 0; i < comps.Count; i++) {
						if (comps[i].CompShouldRemove) {
							return true;
						}
					}
				}
				return base.ShouldRemove;
			}
		} //* Allows comps to determine if it should dissapear at 0 severity

		public override void ExposeData() {
			base.ExposeData();
		}
	}


	//* COMP Properties
	public class HediffCompProperties_RegrowthManager : HediffCompProperties {
		public int ticksToRegrow;           // Ticks after which the hediff will disappear, leaving behind the corresponding BodyPart

		public HediffDef activatorHediffDef;   // The Hediff its looking for when activeIfHediff is true
		public bool showProgress = false;   // Show progress next to Hediff name (0-100%)

		public HediffCompProperties_RegrowthManager() {
			compClass = typeof(HediffComp_RegrowthManager);
		}
	}


	//* COMP
	public class HediffComp_RegrowthManager : HediffComp {
		//* Variables
		public int totalTicks, ticksLeft;
		public float percentage = 0;
		public HediffCompProperties_RegrowthManager Props {
			get {
				return (HediffCompProperties_RegrowthManager)props;
			}
		}

		//* Magic (please explain me)
		public override void CompPostMake() {
			totalTicks = Props.ticksToRegrow; // Because of rare tick
			ticksLeft = totalTicks;
		}
		public override void CompPostTick(ref float severityAdjustment) {
			if (Props.activatorHediffDef != null) { //* only Tick if activatorHediff is present
				if (!Pawn.health.hediffSet.HasHediff(Props.activatorHediffDef)) return;
			}

			if (!parent.pawn.health.hediffSet.GetMissingPartsCommonAncestors().Contains(parent)) return; //* only regrow if previous segment is healed

			ticksLeft--;
			percentage = ((totalTicks - ticksLeft) / (float)totalTicks); // from 0 to 1

			if (ticksLeft <= 0) {
#if DEBUG
				Log.Message($"Finished regrowing BodyPart: \"{parent.Part}\"");
#endif
				Pawn.health.RemoveHediff(parent);
			}
		}

		//public override bool CompShouldRemove => true; // Short version of "return true;"
		public override string CompLabelInBracketsExtra {
			get {
				if (Props.showProgress) {
					return percentage.ToStringPercent();
				} else return null;
			}
		} //* Stuff inside "()" next to Hediff's label

		public override string CompTipStringExtra {
			get {
				if (Props.showProgress) {
					return $"Progress: {percentage.ToStringPercent()}"; //TODO Translation support
				} else return null; //TODO check for this causing errors
			}
		} // Mouseover stats

		public override void CompExposeData() {
			Scribe_Values.Look(ref totalTicks, "totalTicks");
			Scribe_Values.Look(ref ticksLeft, "ticksLeft");
			//Scribe_Values.Look(ref percentage, "percentage", 0);
		}
	}
}
