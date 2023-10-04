using RimWorld;
using Verse;

namespace HospitalityArchitect;

public enum CustomerType
{
    Test = 0,
    Shopping = 1,
    Dining = 2,
    Wellness = 3,
    Gambling = 4
}

public class CustomerData : IExposable
{
    public CustomerData(int arrivedAtTick, CustomerType type)
    {
        this.ArrivedAtTick = arrivedAtTick;
        Type = type;
    }

    public CustomerData()
    {
    }

    public int ArrivedAtTick;
    public CustomerType Type;
    public float Bill;
    public void ExposeData()
    {
        Scribe_Values.Look(ref ArrivedAtTick, "ArrivedAtTick");
        Scribe_Values.Look(ref Bill, "Bill");
        Scribe_Values.Look(ref Type, "VisitorType");
    }
}