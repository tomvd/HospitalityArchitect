using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HospitalityArchitect
{
    public class CustomerService : MapComponent
    {
        public Dictionary<Pawn, CustomerData> Customers;
        private List<Pawn> _colonistsKeysWorkingList;
        private List<CustomerData> _colonistsValuesWorkingList;
        
        public bool openForBusiness = false;
        public List<bool> openingHours = new System.Collections.Generic.List<bool>
        {
            false,false,false,false,false,false,false,true, //8
            false,false,false,true, //12
            false,false,false,true,//16
            false,false,false,true,//20
            false,false,false,false
        };
        public List<RecipeDef> refusedOperations = new List<RecipeDef>();
        public int bedsReserved = 0;
        
        public CustomerService(Map map) : base(map)
        {
            Customers = new Dictionary<Pawn, CustomerData>();
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            openingHours ??= new System.Collections.Generic.List<bool>
            {
                false,false,false,false,false,false,false,true, //8
                false,false,false,true, //12
                false,false,false,true,//16
                false,false,false,true,//20
                false,false,false,false
            };
            refusedOperations ??= new List<RecipeDef>();
            
            Scribe_Collections.Look(ref openingHours, "openingHours");
            Scribe_Collections.Look(ref refusedOperations, "refusedOperations");
            Scribe_Values.Look(ref openForBusiness, "openForBusiness", false);
            Scribe_Values.Look(ref bedsReserved, "bedsReserved", 0);
            Customers ??= new Dictionary<Pawn, CustomerData>();
            Scribe_Collections.Look(ref Customers, "patients", LookMode.Reference, LookMode.Deep, ref _colonistsKeysWorkingList, ref _colonistsValuesWorkingList);
        }

        public bool IsOpen()
        {
            if (!openForBusiness) return false;
            return openingHours[GenLocalDate.HourOfDay(map)];
        }

        public void CustomerArrived(Pawn pawn, CustomerData data)
        {
            Customers.Add(pawn, data);
            MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
        }
        
        public void CustomerLeaves(Pawn pawn)
        {
            if (Customers.TryGetValue(pawn, out var patientData))
            {
                /*float silver = PatientUtility.CalculateSilverToReceive(pawn, patientData);
                if (silver > 0)
                {
                    if (pawn.Faction != null)
                    {
                        int goodwill = PatientUtility.CalculateGoodwillToGain(pawn, patientData);
                        Messages.Message(
                            $"{pawn.NameFullColored} leaves: +" + silver.ToStringMoney() + ", goodwill change: " +
                            goodwill + " " +
                            pawn.Faction.name, MessageTypeDefOf.NeutralEvent);
                        pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwill, false);
                    }
                    else
                    {
                        Messages.Message($"{pawn.NameFullColored} leaves: +" + silver.ToStringMoney(), MessageTypeDefOf.NeutralEvent);                        
                    }
                    var silverThing = ThingMaker.MakeThing(ThingDefOf.Silver);
                    silverThing.stackCount = (int)silver;
                    GenPlace.TryPlaceThing(silverThing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }
*/
                Customers.Remove(pawn);
                MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
            }
            else
            {
                Log.Message($"{pawn.NameFullColored} leaves but is not a patient anymore?");   
            }
        }

        public void PatientDied(Pawn pawn)
        {
            if (Customers.TryGetValue(pawn, out var patientData))
            {
                Messages.Message($"{pawn.NameFullColored} died: -10 "+pawn.Faction.name, MessageTypeDefOf.PawnDeath);
                pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -10, false);
                Customers.Remove(pawn);    
                MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
            }
            // else - was not a patient?
        }
        
        public void DismissCustomer(Pawn pawn)
        {
            if (Customers.TryGetValue(pawn, out var patientData))
            {
                Messages.Message(
                    $"{pawn.NameFullColored} dismissed.", MessageTypeDefOf.NeutralEvent); 
                Customers.Remove(pawn);    
                MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
            }
            // else - was not a patient?
            
        }

        public bool IsRecreationAvailable()
        {
            //return map.listerBuildings.
            return true;
        }
        
    }

 
}