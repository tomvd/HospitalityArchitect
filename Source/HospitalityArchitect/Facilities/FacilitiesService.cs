using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HospitalityArchitect.Facilities;

public class FacilitiesService : MapComponent
{
    public Dictionary<String, FacilitiesData> facilitiesData;
    
    public FacilitiesService(Map map) : base(map)
    {
        facilitiesData = new Dictionary<string, FacilitiesData>();
        facilitiesData.Add("Casino", new FacilitiesData());
        facilitiesData.Add("Restaurant", new FacilitiesData());
        facilitiesData.Add("Spa", new FacilitiesData());
        facilitiesData.Add("Store", new FacilitiesData());
        facilitiesData.Add("Bar", new FacilitiesData());                
        facilitiesData.Add("Nightclub", new FacilitiesData());                
    }
    
    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (GenTicks.TicksGame % 40 == 0) // every ingame minute (+/-)
        {
            RefreshFacilitiesData();
        }
    }

    void RefreshFacilitiesData()
    {
        FacilitiesData data;
        // TODO calculate facility value rating 1/2/3 stars
        if (facilitiesData.TryGetValue("Casino", out data))
        {
            data.totalCapacity = map.listerBuildings.allBuildingsColonist.FindAll(building =>
                building.def.building?.joyKind?.defName.EqualsIgnoreCase("Gamble") == true ||
                building.def.defName.EqualsIgnoreCase("PokerTable")||
                building.def.defName.EqualsIgnoreCase("Joy_RouletteTable")
                ).Count;
            if (data.totalCapacity > 0)
                data.isOpen = true;
            else
                data.isOpen = false;
        }
    }

    public bool FacilityAvailable(string name)
    {
        FacilitiesData data;
        if (facilitiesData.TryGetValue(name, out data))
        {
            return data.isOpen; // todo capacity check? facility value rating matches guest value rating
        }
        else
        {
            return false;
        }
    }
    
}