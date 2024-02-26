using System.Collections.Generic;
using Verse;

namespace HospitalityArchitect;

public class GuestTypeData  : IExposable
{
    public float influencePoints;
    public int bookings;
    public int totalVisits;
    public float avgRating;
    public int QualifiedBedsCached;
    public Stack<int> bookingHours;

    public GuestTypeData()
    {
    }
    
    public void ExposeData()
    {
        Scribe_Values.Look(ref influencePoints, "influencePoints");
        Scribe_Values.Look(ref totalVisits, "totalVisits");
        Scribe_Values.Look(ref avgRating, "avgRating");
        Scribe_Values.Look(ref bookings, "bookings");
        Scribe_Collections.Look(ref bookingHours, "bookingHours");
        bookingHours ??= new Stack<int>();
    }
}