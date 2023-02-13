using RimWorld.Planet;
using System;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RT
{
    public class MainButtonWorker_Finance : MainButtonWorker
    {
        public override void Activate()
        {
            Find.WindowStack.Add(new Dialog_Finance());
        }
    }
}