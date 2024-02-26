using System;
using System.Collections.Generic;
using System.Linq;
using DubsBadHygiene;
using Gastronomy.Restaurant;
using RimWorld;
using Storefront.Store;
using Verse;

namespace HospitalityArchitect.Facilities;

public class FacilitiesService : MapComponent
{
    public Dictionary<String, FacilitiesData> facilitiesData;
    
    public FacilitiesService(Map map) : base(map)
    {
        facilitiesData = new Dictionary<string, FacilitiesData>();
        facilitiesData.Add("Public Bathroom", new FacilitiesData());
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
        if (facilitiesData.TryGetValue("Public Bathroom", out data))
        {
            data.totalCapacity = SanitationUtil.AllFixtures(map)
                .Count(thing => thing is Building_AssignableFixture fixture 
                                && (fixture.fixture is FixtureType.Bath || fixture.fixture is FixtureType.Toilet)
                                && fixture.AllSpawnedBeds().Count == 0);
            if (data.totalCapacity > 0)
                data.isOpen = true;
            else
                data.isOpen = false;
        }        
        
        // TODO calculate facility value rating 1/2/3 stars (or make different facilities called something like Luxury Casino?)
        if (facilitiesData.TryGetValue("Casino", out data))
        {
            data.totalCapacity = map.listerBuildings.allBuildingsColonist.Count(building =>
                building.def.building?.joyKind?.defName.EqualsIgnoreCase("Gamble") == true ||
                building.def.defName.EqualsIgnoreCase("PokerTable")||
                building.def.defName.EqualsIgnoreCase("Joy_RouletteTable")
                );
            if (data.totalCapacity > 0)
                data.isOpen = true;
            else
                data.isOpen = false;
        }
        
        if (facilitiesData.TryGetValue("Spa", out data))
        {
            // TODO ideally the capacity of the spa building is size.x*size.y
            data.totalCapacity = map.listerBuildings.allBuildingsColonist.Where(building =>
                building.def.building?.joyKind?.defName.EqualsIgnoreCase("Hydrotherapy") == true ||
                building.def.defName.EqualsIgnoreCase("DBHSaunaSeating")
            ).Sum(building => building.def.size.x*building.def.size.z);
            if (data.totalCapacity > 0)
                data.isOpen = true;
            else
                data.isOpen = false;
        }
        
        if (facilitiesData.TryGetValue("Restaurant", out data))
        {
            RestaurantsManager rm = Find.CurrentMap.GetComponent<RestaurantsManager>();
            foreach (var restaurantController in rm.restaurants)
            {
                if (restaurantController.IsOpenedRightNow)
                    data.totalCapacity += restaurantController.Seats;
            }
            if (data.totalCapacity > 0)
                data.isOpen = true;
            else
                data.isOpen = false;
        }
        
        if (facilitiesData.TryGetValue("Store", out data))
        {
            StoresManager rm = Find.CurrentMap.GetComponent<StoresManager>();
            foreach (var restaurantController in rm.Stores)
            {
                if (restaurantController.IsOpenedRightNow)
                    data.totalCapacity += 5;
            }
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
            return data.totalCapacity > 0; // todo capacity check? facility value rating matches guest value rating
        }
        else
        {
            return false;
        }
    }
    
    public bool FacilityIsOpen(string name)
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