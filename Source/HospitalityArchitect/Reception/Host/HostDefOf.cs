using RimWorld;
using Verse;

namespace HospitalityArchitect.Reception
{
    [DefOf]
    internal static class HostDefOf
    {
        public static readonly JobDef Reception_Host;
        public static readonly JobDef Reception_StandBy;
        public static readonly WorkTypeDef Reception_Hosting;
        [MayRequire("CashRegister")]
        public static readonly SoundDef CashRegister_Register_Kaching;
    }
}