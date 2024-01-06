using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect
{
    [DefOf]
    public static class HADefOf
    {
        public static readonly JobDef HA_LeaveMap;
        public static readonly JobDef HA_UseComputer;
        public static readonly ThingDef HA_DeliveryVehicle;
        public static readonly ThingDef HA_Bus;
        public static readonly ThingDef HA_DecalLineThin;
        public static readonly DesignationDef Quicksell;
        public static readonly JobDef QuicksellDesignated;
        
        public static readonly JobDef Reception_CheckIn;
        public static readonly JobDef InspectBed;
        public static readonly JobDef ClaimBed;
        //public static readonly ThoughtDef HospitalityArchitect_Serviced;
        public static readonly ThoughtDef HospitalityArchitect_ServicedMood;
        public static readonly ResearchProjectDef HA_BusinessGuests;
    }
}