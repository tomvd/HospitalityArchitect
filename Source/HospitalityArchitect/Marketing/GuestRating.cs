using System.Collections.Generic;
using Verse;

namespace HospitalityArchitect;

public class GuestRating : IExposable
{

    public GuestRating(string name, string kind, int rating, string good, string bad)
    {
        this.name = name;
        this.kind = kind;
        this.rating = rating;
        this.good = good;
        this.bad = bad;
    }

    public GuestRating()
    {
    }
    
    public string name;
    public string kind;
    public int rating;
    public string good;
    public string bad;
    

    public void ExposeData()
    {
        Scribe_Values.Look(ref name, "name");
        Scribe_Values.Look(ref kind, "kind");
        Scribe_Values.Look(ref rating, "rating");
        Scribe_Values.Look(ref good, "good");
        Scribe_Values.Look(ref bad, "bad");
    } 
}