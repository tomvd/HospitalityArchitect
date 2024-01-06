using RimWorld;
using Verse;
using Verse.Sound;

namespace HospitalityArchitect;

public class IncidentWorker_RimazonSales : IncidentWorker_RimazonPriceEffect
{
    public override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        if (!map.GetComponent<RimazonService>().categoryOnSale.NullOrEmpty())
        {
            return false;
        }
        if (base.TryExecuteWorker(parms))
        {
            SoundDefOf.PsychicSootheGlobal.PlayOneShotOnCamera((Map)parms.target);
            return true;
        }
        return false;
    }

    protected override void DoConditionAndLetter(IncidentParms parms, Map map, int duration, Gender gender, float points)
    {
        //GameCondition_PsychicEmanation gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicSoothe, duration);
        //gameCondition_PsychicEmanation.gender = gender;
        //map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
        map.GetComponent<RimazonService>().categoryOnSale = "meals";
        map.GetComponent<RimazonService>().salesEndsOn = GenDate.TicksGame + duration;
        SendStandardLetter("Rimazon Sales", "20% off in category meals! The sales ends in 1 or 2 days.", LetterDefOf.PositiveEvent, parms, LookTargets.Invalid);
    }
}