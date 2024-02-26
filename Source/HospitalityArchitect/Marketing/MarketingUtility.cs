using System;
using UnityEngine;

namespace HospitalityArchitect;

public static class MarketingUtility
{
    public static float GetMarketingCost(GuestTypeDef type)
    {
        return Mathf.Round(type.budget.min * 0.75f);
    }
}