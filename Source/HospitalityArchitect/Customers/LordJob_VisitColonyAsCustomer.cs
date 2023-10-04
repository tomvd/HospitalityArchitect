using RimWorld;
using Verse;
using Verse.AI.Group;

namespace HospitalityArchitect;
public class LordJob_VisitColonyAsCustomer : LordJob
{
        public LordJob_VisitColonyAsCustomer()
        {
            // Required
        }

        public LordJob_VisitColonyAsCustomer(Faction faction)
        {

        }

        public override bool NeverInRestraints => true;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override StateGraph CreateGraph()
        {
            StateGraph graphArrive = new StateGraph();
            StateGraph graphExit = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
            // Be a customer
            var toilVisiting = new LordToil_Customer();
            graphArrive.lordToils.Add(toilVisiting);
            // Exit
            LordToil toilExit = graphArrive.AttachSubgraph(graphExit).StartingToil;

            // Leave if customer is satisfied
            {
                Transition transition = new Transition(toilVisiting, toilExit);
                transition.triggers.Add(new Trigger_SentAway()); // Sent away during stay
                transition.preActions.Add(new TransitionAction_EnsureHaveNearbyExitDestination());
                transition.postActions.Add(new TransitionAction_WakeAll());
                graphArrive.transitions.Add(transition);
            }
            return graphArrive;
        }
        
        public override void Notify_PawnLost(Pawn pawn, PawnLostCondition condition)
        {
            //Log.Message($"{pawn.NameFullColored} lost because of {condition}");
            Find.CurrentMap.GetComponent<CustomerService>().DismissCustomer(pawn);
        }
}