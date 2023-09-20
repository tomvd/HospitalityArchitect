using RimWorld;
using Verse;

namespace HospitalityArchitect;

public abstract class Designator_AreaHome : Designator_Cells
{
    private DesignateMode mode;

    public override int DraggableDimensions => 2;

    public override bool DragDrawMeasurements => true;

    public Designator_AreaHome(DesignateMode mode)
    {
        this.mode = mode;
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;
        hotKey = KeyBindingDefOf.Misc7;
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(base.Map))
        {
            return false;
        }
        bool flag = base.Map.areaManager.Home[c];
        if (mode == DesignateMode.Add)
        {
            float landValue = Utils.GetLandValue(Map, c);
            if (Map.GetComponent<FinanceService>()
                .canAfford(landValue))
                return !flag;
            else
            {
                return false;
            }
        }
        return flag;
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        float landValue = Utils.GetLandValue(Map, c);
        if (mode == DesignateMode.Add)
        {
            Map.areaManager.Home[c] = true;
            Map.GetComponent<FinanceService>()
                .doAndBookExpenses(FinanceReport.ReportEntryType.Land, landValue);
        }
        else
        {
            Map.areaManager.Home[c] = false;
            Map.GetComponent<FinanceService>().doAndBookIncome(FinanceReport.ReportEntryType.Land, landValue);
        }
    }

    public override void FinalizeDesignationSucceeded()
    {
        base.FinalizeDesignationSucceeded();
        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HomeArea, KnowledgeAmount.Total);
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        base.Map.areaManager.Home.MarkForDraw();
    }
}