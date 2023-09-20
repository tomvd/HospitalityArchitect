using RimWorld;
using Verse;

namespace HospitalityArchitect
{
    public class MainButtonWorker_Finance : MainButtonWorker
    {
        public override void Activate()
        {
            Find.WindowStack.Add(new Dialog_Finance());
        }
    }
}