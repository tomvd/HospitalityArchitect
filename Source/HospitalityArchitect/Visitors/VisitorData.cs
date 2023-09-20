using Verse;

namespace HospitalityArchitect;

public enum VisitorType
{
    Test = 0,
    Shopping = 1,
    Dining = 2,
    Wellness = 3,
    Gambling = 4
}

public class VisitorData : IExposable
{
    public VisitorData(int arrivedAtTick, float initialMarketValue, float initialMood, VisitorType type)
    {
        this.ArrivedAtTick = arrivedAtTick;
        this.InitialMarketValue = initialMarketValue;
        Type = type;
        InitialMood = initialMood;
    }

    public VisitorData()
    {
    }

    public int ArrivedAtTick;
    public float InitialMarketValue;
    public float InitialMood;
    public VisitorType Type;
    public float Bill;
    public string Cure;
    public string Diagnosis;
    public RecipeDef CureRecipe;
    public void ExposeData()
    {
        Scribe_Values.Look(ref ArrivedAtTick, "ArrivedAtTick");
        Scribe_Values.Look(ref InitialMarketValue, "InitialMarketValue");
        Scribe_Values.Look(ref InitialMood, "InitialMood");
        Scribe_Values.Look(ref Bill, "Bill");
        Scribe_Values.Look(ref Cure, "Cure");
        Scribe_Values.Look(ref Diagnosis, "Diagnosis");
        Scribe_Defs.Look(ref CureRecipe, "CureRecipe");
    }
}