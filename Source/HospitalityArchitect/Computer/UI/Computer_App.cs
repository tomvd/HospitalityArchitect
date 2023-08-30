using UnityEngine;

namespace RT
{
    public abstract class ComputerApp
    {
        public abstract void DoWindowContents(Rect inRect);
        public abstract string getLabel();
    }
}