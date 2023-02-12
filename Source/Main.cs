using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Noise;
using Verse.Grammar;
using RimWorld;
using RimWorld.Planet;

// using System.Reflection;
using HarmonyLib;

namespace RT
{
    public class RimworldTycoon : Mod
    {
        public RimworldTycoon(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("Adamas.RimworldTycoon");
            harmonyInstance.PatchAll();
        }

        public static Harmony harmonyInstance;
    }

    public class MyMapComponent : MapComponent
    {
        public MyMapComponent(Map map) : base(map){}
        public override void FinalizeInit()
        {         
            //Find.ResearchManager.DebugSetAllProjectsFinished();
        }
    }

    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            Log.Message("Rimworld Tycoon loaded successfully!");
        }
    }

}
