using Verse;

namespace HospitalityArchitect;

public class JobDriver_Quicksell_Designated : JobDriver_Quicksell
{
    protected override DesignationDef Designation => HADefOf.Quicksell;
    protected override float TotalNeededWork => 100;
    
    protected override void FinishedRemoving()
    {
        Map.GetComponent<FinanceService>().doAndBookIncome(FinanceReport.ReportEntryType.Misc,QuickSellUtil.QSPrice(Target));
        Target.Destroy(DestroyMode.Refund);
    }
}