using System.Collections.Generic;
using System.Linq;
using Hospitality;
using Hospitality.Utilities;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace HospitalityArchitect
{
    public class CompHotelGuestBed : ThingComp
    {
        public PawnKindDef guestType;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref guestType, "guestType");
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
            return l;
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent is Building_GuestBed gb && gb.IsGuestBed())
            {
                yield return new Command_SetGuestType(this)
                {
                    defaultLabel = "SetGuestType",//.Translate(),
                    //icon = ContentFinder<Texture2D>.Get("UI/Commands/pricetag" + pricing),
                    disabled = false
                };
            }
        }
    }
    
    
}