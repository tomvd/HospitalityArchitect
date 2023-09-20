using UnityEngine;

namespace HospitalityArchitect
{
    public abstract class ComputerApp
    {
        public abstract void DoWindowContents(Rect inRect);
        public abstract string getLabel();
    }
}