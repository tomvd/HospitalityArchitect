using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using RT.UItils;

namespace RT
{
    public class Comms_Buy : ICommunicable
    {

        public Comms_Buy()
        {
        }


        public string GetCallLabel()
        {
            return "Buy from rimazon";
        }

        public string GetInfoText()
        {
            return "Buy from rimazon";
        }

        public void TryOpenComms(Pawn negotiator)
        {
            Find.WindowStack.Add(new Dialog_Buy(negotiator));
        }

        public Faction GetFaction()
        {
            return null;
        }

        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator) => FloatMenuUtility.DecoratePrioritizedTask(
         new FloatMenuOption(GetCallLabel(), () => console.GiveUseCommsJob(negotiator, this), MenuOptionPriority.InitiateSocial), negotiator, console);
    }
}