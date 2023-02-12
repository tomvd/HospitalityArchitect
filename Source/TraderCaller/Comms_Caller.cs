using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using RT.UItils;

namespace RT
{
    public class Comms_Caller : ICommunicable
    {

        public Comms_Caller()
        {
        }


        public string GetCallLabel()
        {
            return "Call orbital trader";
        }

        public string GetInfoText()
        {
            return "Immediatly ask an orbital trader to come by.";
        }

        public void TryOpenComms(Pawn negotiator)
        {
            Find.WindowStack.Add(new Dialog_Caller(negotiator));
        }

        public Faction GetFaction()
        {
            return null;
        }

        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator) => FloatMenuUtility.DecoratePrioritizedTask(
         new FloatMenuOption(GetCallLabel(), () => console.GiveUseCommsJob(negotiator, this), MenuOptionPriority.InitiateSocial), negotiator, console);
    }
}