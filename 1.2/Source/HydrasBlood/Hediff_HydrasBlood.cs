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
	// ============================ Hydra's Blood (Hediff) ============================


	//* This class is mainly for cheching if the Hediff is present, as its a child class of Hediff_High
	public class Hediff_HydrasBlood : HediffWithComps {
		HediffComp_RegrowMissingParts regrow_comp;
		public override void ExposeData() { //TODO Fix the expose data, null error for the regrow_comp
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.LoadingVars) {
				regrow_comp = HediffUtility.TryGetComp<HediffComp_RegrowMissingParts>(this);
			}
		}
		public override void PostMake() {
			base.PostMake();
			regrow_comp = HediffUtility.TryGetComp<HediffComp_RegrowMissingParts>(this);
		}
		public override float PainOffset {
			get {
				//HediffComp_RegrowMissingParts regrow_comp = HediffUtility.TryGetComp<HediffComp_RegrowMissingParts>(this); //! Testing
				float painOffset = CurStage.painOffset; //* Include Hediff Stage painOffset
				if (regrow_comp == null) {
#if DEBUG
					Log.Warning($"[Hediff_HydrasBlood] regrow_comp is null, pain can't be calculated");
#endif
					return painOffset;
				}

				foreach (var rgrow in pawn.health.hediffSet.hediffs.Where(hediff => hediff is Hediff_RegowingBodyPart)) {
					if (rgrow != null && rgrow.Visible) {
						painOffset += (regrow_comp.Props.organList.Contains(rgrow.Part.def)) ?
							regrow_comp.Props.painOffsetIfOrgan : regrow_comp.Props.painOffset;
					}
				}

				return painOffset;
			}
		}
	}

}
