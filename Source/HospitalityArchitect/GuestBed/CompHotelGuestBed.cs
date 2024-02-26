using System.Collections.Generic;
using System.Linq;
using DubsBadHygiene;
using Hospitality;
using Hospitality.Utilities;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace HospitalityArchitect
{
    [StaticConstructorOnStartup]
    public class CompHotelGuestBed : ThingComp
    {
        public PawnKindDef guestType;
        public static Material LowWaterMat = MaterialPool.MatFrom("DBH/UI/contamination", ShaderDatabase.MetaOverlay);
        public bool needBedding;
        

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref guestType, "guestType");
            Scribe_Values.Look(ref needBedding, "needBedding");
            if (parent.def.defName.Equals("CampingTentSpot") || parent.def.defName.Equals("CampingTentSpotGuest") ||
                parent.def.defName.Equals("ModernTentGuest"))
            {
                guestType = DefDatabase<PawnKindDef>.GetNamed("CamperGuest", false); // force guest type
            }                       
            if (guestType == null)
            {
                guestType = DefDatabase<PawnKindDef>.GetNamed("BackpackerGuest", false);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent.def.defName.Equals("CampingTentSpot") || parent.def.defName.Equals("CampingTentSpotGuest") ||
                parent.def.defName.Equals("ModernTentGuest"))
            {
                guestType = DefDatabase<PawnKindDef>.GetNamed("CamperGuest", false); // force guest type
            }                       
            if (guestType == null)
            {
                guestType = DefDatabase<PawnKindDef>.GetNamed("BackpackerGuest", false);
            }            
        }

        public override string CompInspectStringExtra()
        {
            string l = "";
            if (guestType != null && parent is Building_GuestBed gb && gb.IsGuestBed())
            {
                GuestTypeDef type = guestType.GetModExtension<GuestTypeDef>();
                l += "Guest type: " + guestType.label;
                if (type.bedroomRequirements != null && !type.bedroomRequirements.TrueForAll(br => br.MetOrDisabled(gb.GetRoom())))
                {
                    l += " " + string.Join(",",type.bedroomRequirements
                        .Where(br => !br.MetOrDisabled(gb.GetRoom())).Select(br => br.Label(gb.GetRoom())).ToList());
                }

                if (gb.GetRentalFee() > type.bedBudget)
                {
                    l += " " + "bed fee too high!";
                }
                l += "\n";
                l += "Rating: " + BedUtility.StaticBedValue(gb,out _, out _, out _, out _, out _, out _);
            }

            if (needBedding)
            {
                l += "\n";
                l += "Needs clean bedding!";                
            }
            return l;
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent is Building_GuestBed gb && gb.IsGuestBed() && !(parent.def.defName.Equals("CampingTentSpotGuest") || parent.def.defName.Equals("ModernTentGuest")))
            {
                yield return new Command_SetGuestType(this)
                {
                    defaultLabel = "SetGuestType",//.Translate(),
                    //icon = ContentFinder<Texture2D>.Get("UI/Commands/pricetag" + pricing),
                    disabled = false
                };
                
                if (Prefs.DevMode)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Debug: make bedding dirty",
                        action = MakeBeddingDirty
                    };            
                }                
            }
        }
        
        public override void PostDraw()
        {
            //base.PostDraw(); // TODO why is this not working??
            if (needBedding)
                DubUtils.RenderPulsingOverlay(parent, LowWaterMat, parent.DrawPos, MeshPool.plane08, Quaternion.identity);
        }

        public void MakeBeddingDirty()
        {
            if (needBedding) return; // already dirty
            needBedding = true;
            Thing thing = ThingMaker.MakeThing(HADefOf.DirtyBedding, null);
            GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near, null, null);
        }
        
    }
    
    
}