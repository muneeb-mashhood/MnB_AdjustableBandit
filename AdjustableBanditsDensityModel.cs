using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace AdjustableBandits
{
	internal class AdjustableBanditsDensityModel : DefaultBanditDensityModel
	{
		public override int NumberOfMinimumBanditPartiesInAHideoutToInfestIt =>
			AdjustableBandits.GetSettings().NumberOfMinimumBanditPartiesInAHideoutToInfestIt;

		public override int NumberOfMaximumBanditPartiesInEachHideout =>
			AdjustableBandits.GetSettings().NumberOfMaximumBanditPartiesInEachHideout;

		public override int NumberOfMaximumBanditPartiesAroundEachHideout =>
			AdjustableBandits.GetSettings().NumberOfMaximumBanditPartiesAroundEachHideout;

		public override int NumberOfMaximumHideoutsAtEachBanditFaction =>
			AdjustableBandits.GetSettings().NumberOfMaximumHideoutsAtEachBanditFaction;

		public override int NumberOfInitialHideoutsAtEachBanditFaction =>
			AdjustableBandits.GetSettings().NumberOfInitialHideoutsAtEachBanditFaction;

		public override int NumberOfMinimumBanditTroopsInHideoutMission =>
			AdjustableBandits.GetSettings().NumberOfMinimumBanditTroopsInHideoutMission;

		public override int NumberOfMaximumTroopCountForFirstFightInHideout =>  // MathF.Floor(6f * (2f + Campaign.Current.PlayerProgress))
			AdjustableBandits.GetSettings().NumberOfMaximumTroopCountForFirstFightInHideoutFactor <= 0f ? 65536 :
			(int)(base.NumberOfMaximumTroopCountForFirstFightInHideout * AdjustableBandits.GetSettings().NumberOfMaximumTroopCountForFirstFightInHideoutFactor);
		public override int NumberOfMaximumTroopCountForBossFightInHideout =>   // MathF.Floor(1f + 5f * (1f + Campaign.Current.PlayerProgress))
			AdjustableBandits.GetSettings().NumberOfMaximumTroopCountForBossFightInHideoutFactor <= 0f ? 65536 :
			(int)(base.NumberOfMaximumTroopCountForBossFightInHideout * AdjustableBandits.GetSettings().NumberOfMaximumTroopCountForBossFightInHideoutFactor);
		public override float SpawnPercentageForFirstFightInHideoutMission =>
			AdjustableBandits.GetSettings().SpawnPercentageForFirstFightInHideoutMission;

		public override int GetMaxSupportedNumberOfLootersForClan(Clan clan) =>
			AdjustableBandits.GetMaxSupportedNumberOfPartiesForClan(clan);

		public override int GetMaximumTroopCountForHideoutMission(MobileParty party, bool isAssault)
		{
			if (party == null || !party.IsMainParty)
				return base.GetMaximumTroopCountForHideoutMission(party, isAssault);

			float troopCount = AdjustableBandits.GetSettings().PlayerMaximumTroopCountForHideoutMission;
			if (party.HasPerk(DefaultPerks.Tactics.SmallUnitTactics))
				troopCount += DefaultPerks.Tactics.SmallUnitTactics.PrimaryBonus;
			return MathF.Round(troopCount);
		}
	}
}
