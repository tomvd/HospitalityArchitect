using RimWorld;
using Verse;

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