using HarmonyLib;
using Verse;
// using System.Reflection;

namespace RT
{
    public class RimworldTycoon : Mod
    {
        public static Harmony harmonyInstance;

        public RimworldTycoon(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("Adamas.RimworldTycoon");
            harmonyInstance.PatchAll();
        }
    }

    public class MyMapComponent : MapComponent
    {
        public MyMapComponent(Map map) : base(map)
        {
        }

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