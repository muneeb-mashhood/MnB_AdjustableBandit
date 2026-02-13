using TaleWorlds.CampaignSystem;

namespace AdjustableBandits
{
  internal sealed class BanditMinimumSpawnBehavior : CampaignBehaviorBase
  {
    private static int hoursSinceLastEnforce;

    public override void RegisterEvents()
    {
      CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
    }

    public override void SyncData(IDataStore dataStore)
    {
    }

    private static void OnHourlyTick()
    {
      hoursSinceLastEnforce++;
      if (hoursSinceLastEnforce < 3)
        return;

      hoursSinceLastEnforce = 0;
      AdjustableBandits.EnsureMinimumParties();
    }
  }
}
