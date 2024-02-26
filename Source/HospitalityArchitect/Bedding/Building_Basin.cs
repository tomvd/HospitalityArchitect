using DubsBadHygiene;

namespace HospitalityArchitect.Laundry;

public class Building_basin : Building_AssignableFixture
{
    public float heatUsedPerTick = 0.00025f;

    public float sewageUsedPerTick = 0.01f;

    public float waterUsedPerTick = 0.04f;

    public override float waterUsed => 1f;

    public override FixtureType fixture => FixtureType.Basin;

    public override FixtureQuality Quality => FixtureQuality.normal;
}