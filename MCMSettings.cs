using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.MapEvents;

namespace AdjustableBandits
{
	public class MCMSettings : AttributeGlobalSettings<MCMSettings>
	{
		public override string Id => "AdjustableBandits";
		public override string DisplayName => "Adjustable Bandits";
		public override string FolderName => "AdjustableBandits";
		public override string FormatType => "json";

		#region BANDIT POPULATION
		private const string LootersGroupName = "{=adjban_group_Looters}Looters";
		private const string BanditsGroupName = "{=adjban_group_Bandits}Bandits";
		private const string SeaRaidersGroupName = "{=adjban_group_SeaRaidersCorsairs}Sea Raiders & Corsairs";
		private const string DesertersGroupName = "{=adjban_group_Deserters}Deserters";
		private const string LogsGroupName = "{=adjban_group_Logs}Logs";

		[SettingPropertyFloatingInteger(
			"{=adjban_name_BanditPartySizeMultiplier}Bandit Party Size Multiplier (other bandits)",
			0.01f,
			100.0f,
			"0.00",
			RequireRestart = false,
			HintText = "{=adjban_hint_BanditPartySizeMultiplier}Scales other bandit party sizes (not looters, sea raiders, or corsairs). Applies to newly spawned parties. [Default: 1.00]",
			Order = 6)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public float BanditMultiplier { get; set; } = 1f;

		[SettingPropertyFloatingInteger(
			"Desert Bandit Party Size Multiplier",
			0.01f,
			100.0f,
			"0.00",
			RequireRestart = false,
			HintText = "Scales desert bandit party sizes. Applies to newly spawned parties. [Default: 0.60]",
			Order = 0)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public float DesertBanditMultiplier { get; set; } = 0.6f;

		[SettingPropertyFloatingInteger(
			"Steppe Bandit Party Size Multiplier",
			0.01f,
			100.0f,
			"0.00",
			RequireRestart = false,
			HintText = "Scales steppe bandit party sizes. Applies to newly spawned parties. [Default: 0.60]",
			Order = 2)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public float SteppeBanditMultiplier { get; set; } = 0.6f;

		[SettingPropertyFloatingInteger(
			"Forest Bandit Party Size Multiplier",
			0.01f,
			100.0f,
			"0.00",
			RequireRestart = false,
			HintText = "Scales forest bandit party sizes. Applies to newly spawned parties. [Default: 0.60]",
			Order = 4)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public float ForestBanditMultiplier { get; set; } = 0.6f;

		[SettingPropertyFloatingInteger(
			"Looter Party Size Multiplier",
			0.01f,
			100.0f,
			"0.00",
			RequireRestart = false,
			HintText = "Scales looter party sizes. Applies to newly spawned parties. [Default: 2.00]",
			Order = 0)]
		[SettingPropertyGroup(
			LootersGroupName,
			GroupOrder = 1)]
		public float LooterMultiplier { get; set; } = 2f;

		[SettingPropertyFloatingInteger(
			"Deserter Party Size Multiplier",
			0.01f,
			100.0f,
			"0.00",
			RequireRestart = false,
			HintText = "Scales deserter party sizes. Applies to newly spawned parties. [Default: 1.00]",
			Order = 0)]
		[SettingPropertyGroup(
			DesertersGroupName,
			GroupOrder = 3)]
		public float DeserterMultiplier { get; set; } = 1f;

		[SettingPropertyFloatingInteger(
			"Sea Raider Party Size Multiplier",
			0.01f,
			100.0f,
			"0.00",
			RequireRestart = false,
			HintText = "Scales sea raider party sizes. Applies to newly spawned parties. [Default: 0.60]",
			Order = 0)]
		[SettingPropertyGroup(
			SeaRaidersGroupName,
			GroupOrder = 4)]
		public float SeaRaiderMultiplier { get; set; } = 0.6f;

		[SettingPropertyFloatingInteger(
			"Corsair Party Size Multiplier",
			0.01f,
			100.0f,
			"0.00",
			RequireRestart = false,
			HintText = "Scales corsair party sizes. Applies to newly spawned parties. [Default: 0.60]",
			Order = 2)]
		[SettingPropertyGroup(
			SeaRaidersGroupName,
			GroupOrder = 4)]
		public float CorsairMultiplier { get; set; } = 0.6f;

		[SettingPropertyInteger(
			"{=adjban_name_BanditPartySizeLimit}Bandit Party Size Limit (affects Movement Speed)",
			20,
			1000,
			"0",
			RequireRestart = false,
			HintText =
			"{=adjban_hint_BanditPartySizeLimit}Movement speed penalty starts when a bandit party exceeds this limit; higher values reduce or remove the penalty. [Default: 20]",
			Order = 8)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public int BanditPartySizeLimit { get; set; } = 20;


		[SettingPropertyInteger(
			"{=adjban_name_MaxNumLooterParties}Maximum Number of Looter Parties",
			0,
			1000,
			"{=adjban_format_Parties}0 Parties",
			RequireRestart = false,
			HintText = "{=adjban_hint_MaxNumLooterParties}Caps total looter parties on the world map. [Default: 200]",
			Order = 1)]
		[SettingPropertyGroup(
			LootersGroupName,
			GroupOrder = 1)]
		public int NumberOfMaximumLooterParties { get; set; } = 200;

		[SettingPropertyInteger(
			"Maximum Number of Deserter Parties",
			0,
			1000,
			"0 Parties",
			RequireRestart = false,
			HintText = "Caps total deserter parties on the world map. [Default: 30]",
			Order = 1)]
		[SettingPropertyGroup(
			DesertersGroupName,
			GroupOrder = 3)]
		public int NumberOfMaximumDeserterParties { get; set; } = 30;

		[SettingPropertyInteger(
			"Maximum Number of Bandit Parties (other bandits)",
			0,
			1000,
			"0 Parties",
			RequireRestart = false,
			HintText = "Caps total other bandit parties (not looters, sea raiders, or corsairs). [Default: 5]",
			Order = 7)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public int NumberOfMaximumBanditParties { get; set; } = 5;

		[SettingPropertyInteger(
			"Maximum Number of Desert Bandit Parties",
			0,
			1000,
			"0 Parties",
			RequireRestart = false,
			HintText = "Caps total desert bandit parties on the world map. [Default: 5]",
			Order = 1)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public int NumberOfMaximumDesertBanditParties { get; set; } = 5;

		[SettingPropertyInteger(
			"Maximum Number of Steppe Bandit Parties",
			0,
			1000,
			"0 Parties",
			RequireRestart = false,
			HintText = "Caps total steppe bandit parties on the world map. [Default: 5]",
			Order = 3)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public int NumberOfMaximumSteppeBanditParties { get; set; } = 5;

		[SettingPropertyInteger(
			"Maximum Number of Forest Bandit Parties",
			0,
			1000,
			"0 Parties",
			RequireRestart = false,
			HintText = "Caps total forest bandit parties on the world map. [Default: 5]",
			Order = 5)]
		[SettingPropertyGroup(
			BanditsGroupName,
			GroupOrder = 2)]
		public int NumberOfMaximumForestBanditParties { get; set; } = 5;

		[SettingPropertyInteger(
			"Maximum Number of Sea Raider Parties",
			0,
			1000,
			"0 Parties",
			RequireRestart = false,
			HintText = "Caps total sea raider parties on the world map. [Default: 5]",
			Order = 1)]
		[SettingPropertyGroup(
			SeaRaidersGroupName,
			GroupOrder = 4)]
		public int NumberOfMaximumSeaRaiderParties { get; set; } = 5;

		[SettingPropertyInteger(
			"Maximum Number of Corsair Parties",
			0,
			1000,
			"0 Parties",
			RequireRestart = false,
			HintText = "Caps total corsair parties on the world map. [Default: 5]",
			Order = 3)]
		[SettingPropertyGroup(
			SeaRaidersGroupName,
			GroupOrder = 4)]
		public int NumberOfMaximumCorsairParties { get; set; } = 5;

		#endregion

		#region HIDEOUTS
		private const string HideoutsGroupName = "{=adjban_group_Hideouts}Hideouts";

		[SettingPropertyInteger(
			"{=adjban_name_InitialHideoutsPerFaction}Initial Hideouts per Faction",
			0,
			50,
			"{=adjban_format_Hideouts}0 Hideouts",
			RequireRestart = false,
			HintText = "{=adjban_hint_InitialHideoutsPerFaction}Hideouts per bandit faction at campaign start. [Default: 3]",
			Order = 0)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public int NumberOfInitialHideoutsAtEachBanditFaction { get; set; } = 3;

		[SettingPropertyInteger(
			"{=adjban_name_MaxHideoutsPerFaction}Maximum Hideouts per Faction",
			0,
			100,
			"{=adjban_format_Hideouts}0 Hideouts",
			RequireRestart = false,
			HintText = "{=adjban_hint_MaxHideoutsPerFaction}Maximum hideouts per bandit faction. [Default: 10]",
			Order = 1)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public int NumberOfMaximumHideoutsAtEachBanditFaction { get; set; } = 10;

		[SettingPropertyInteger(
			"{=adjban_name_MinPartiesToInfestHideout}Minimum Parties to Infest Hideout",
			1,
			10,
			"{=adjban_format_Parties}0 Parties",
			RequireRestart = false,
			HintText = "{=adjban_hint_MinPartiesToInfestHideout}Minimum bandit parties needed to infest a hideout. [Default: 2]",
			Order = 2)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public int NumberOfMinimumBanditPartiesInAHideoutToInfestIt { get; set; } = 2;

		[SettingPropertyInteger(
			"{=adjban_name_MaxPartiesAroundHideout}Maximum Parties around Hideout",
			1,
			100,
			"{=adjban_format_Parties}0 Parties",
			RequireRestart = false,
			HintText = "{=adjban_hint_MaxPartiesAroundHideout}Maximum bandit parties roaming around a hideout. [Default: 8]",
			Order = 3)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public int NumberOfMaximumBanditPartiesAroundEachHideout { get; set; } = 8;

		[SettingPropertyInteger(
			"{=adjban_name_MaxPartiesInHideout}Maximum Parties in Hideout",
			1,
			10,
			"{=adjban_format_Parties}0 Parties",
			RequireRestart = false,
			HintText = "{=adjban_hint_MaxPartiesInHideout}Maximum bandit parties inside a hideout. [Default: 4]",
			Order = 4)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public int NumberOfMaximumBanditPartiesInEachHideout { get; set; } = 4;

		[SettingPropertyFloatingInteger(
			"{=adjban_name_MaxTroopCountFirstFight}Maximum Troop Count in First Fight - Factor",
			0f,
			10f,
			"0.0",
			RequireRestart = false,
			HintText = "{=adjban_hint_MaxTroopCountFirstFight}Multiplier for the first fight troop cap in hideouts. Set to 0 for unlimited. [Default: 1.0]",
			Order = 5)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public float NumberOfMaximumTroopCountForFirstFightInHideoutFactor { get; set; } = 1f;

		[SettingPropertyFloatingInteger(
			"{=adjban_name_MaxTroopCountBossFight}Maximum Troop Count in Boss Fight - Factor",
			0f,
			10f,
			"0.0",
			RequireRestart = false,
			HintText = "{=adjban_hint_MaxTroopCountBossFight}Multiplier for the boss fight troop cap in hideouts. Set to 0 for unlimited. [Default: 1.0]",
			Order = 6)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public float NumberOfMaximumTroopCountForBossFightInHideoutFactor { get; set; } = 1f;

		[SettingPropertyFloatingInteger(
			"{=adjban_name_SpawnPercFirstFight}Spawn Percentage in First Fight",
			0f,
			0.99f,
			"0%",
			RequireRestart = false,
			HintText = "{=adjban_hint_SpawnPercFirstFight}Percent of hideout troops spawned in the first fight. [Default: 75%]",
			Order = 7)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public float SpawnPercentageForFirstFightInHideoutMission { get; set; } = 0.75f;


		[SettingPropertyInteger(
			"{=adjban_name_MinBanditTroopsHideoutMission}Minimum Bandit Troops in Hideout Mission",
			1,
			100,
			"{=adjban_format_Troops}0 Troops",
			RequireRestart = false,
			HintText = "{=adjban_hint_MinBanditTroopsHideoutMission}Minimum enemy troops in a hideout mission. [Default: 10]",
			Order = 8)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public int NumberOfMinimumBanditTroopsInHideoutMission { get; set; } = 10;

		[SettingPropertyInteger(
			"{=adjban_name_MaxPlayerTroopsHideoutMission}Maximum Player Troops in Hideout Mission",
			1,
			100,
			"{=adjban_format_Troops}0 Troops",
			RequireRestart = false,
			HintText = "{=adjban_hint_MaxPlayerTroopsHideoutMission}Maximum player troops allowed in a hideout mission. [Default: 10]",
			Order = 9)]
		[SettingPropertyGroup(
			HideoutsGroupName,
			GroupOrder = 5)]
		public int PlayerMaximumTroopCountForHideoutMission { get; set; } = 10;
		#endregion

		#region LOGS
		[SettingPropertyBool(
			"Enable Logging",
			RequireRestart = false,
			HintText = "Writes debug info to %USERPROFILE%\\Documents\\Mount and Blade II Bannerlord\\Configs\\ModLogs\\AdjustableBandits.log. [Default: Off]",
			Order = 0)]
		[SettingPropertyGroup(
			LogsGroupName,
			GroupOrder = 6)]
		public bool EnableLogging { get; set; } = false;
		#endregion

		#region ACTIONS
		private const string ActionsGroupName = "{=adjban_group_Actions}Actions";

		[SettingPropertyButton(
			"{=adjban_name_RemoveBanditParties}Remove all bandit parties",
			RequireRestart = false,
			HintText = "{=adjban_hint_RemoveBanditParties}Removes all bandit parties from the current game. This cannot be undone.",
			Content = "{=adjban_content_RemoveBanditParties}Remove bandits",
			Order = 0)]
		[SettingPropertyGroup(
			ActionsGroupName,
			GroupOrder = 7)]
		public Action RemoveAllBanditParties { get; set; } = () =>
		{
			InformationManager.ShowInquiry(
				new InquiryData(
					new TextObject("{=adjban_inquiryTitle_RemoveBandits}Removing Bandits...").ToString(),
					new TextObject("{=adjban_inquiryText_RemoveBandits}Remove all bandit parties?\nThis cannot be undone!").ToString(),
					true, true,
					new TextObject("{=adjban_yes}Yes").ToString(),
					new TextObject("{=adjban_no}No").ToString(),
					action,
					() => { }));

			void action()
			{
				int partiesRemoved = 0;
				int partiesNotRemovedSettlement = 0;
				int partiesNotRemovedMapEvent = 0;
				int troopsRemoved = 0;
				int troopsHideout = 0;
				int troopsMapEvent = 0;

				var parties = MobileParty.AllBanditParties;
				if (parties?.Count > 0)
				{
					for (int i = parties.Count - 1; i >= 0; i--)
					{
						var party = parties[i];
						if (party == null)
							continue;
						if (party.CurrentSettlement != null)
						{
							partiesNotRemovedSettlement++;
							troopsHideout += party.MemberRoster?.TotalManCount ?? 0;
							continue;
						}
						if (party.Party?.MapEvent != null)
						{
							partiesNotRemovedMapEvent++;
							troopsMapEvent += party.MemberRoster?.TotalManCount ?? 0;
							continue;
						}
						troopsRemoved += party.MemberRoster?.TotalManCount ?? 0;
						DestroyPartyAction.Apply(null, party);
						partiesRemoved++;
					}
				}
				InformationManager.DisplayMessage(new InformationMessage(
					"Bandit Party Removal:" +
					$"\n {partiesRemoved} parties removed ({troopsRemoved} troops)" +
					$"\n {partiesNotRemovedSettlement} parties in hideouts were not removed ({troopsHideout} troops)" +
					$"\n {partiesNotRemovedMapEvent} parties could not be removed due to map events ({troopsMapEvent} troops)"));
			}
		};

		[SettingPropertyButton(
			"{=adjban_name_RemoveHideouts}Remove all hideouts",
			RequireRestart = false,
			HintText = "{=adjban_hint_RemoveHideouts}Removes all hideouts from the current game. This cannot be undone.",
			Content = "{=adjban_content_RemoveHideouts}Remove hideouts",
			Order = 1)]
		[SettingPropertyGroup(
			ActionsGroupName,
			GroupOrder = 7)]
		public Action RemoveAllHideouts { get; set; } = () =>
		{
			InformationManager.ShowInquiry(
				new InquiryData(
					new TextObject("{=adjban_inquiryTitle_RemoveHideouts}Removing Hideouts...").ToString(),
					new TextObject("{=adjban_inquiryText_RemoveHideouts}Remove all hideouts?\nThis cannot be undone!").ToString(),
					true, true,
					new TextObject("{=adjban_yes}Yes").ToString(),
					new TextObject("{=adjban_no}No").ToString(),
					action,
					() => { }));

			void action()
			{
				int hideoutsRemoved = 0;
				int hideoutsNotRemoved = 0;
				int partiesRemoved = 0;
				int partiesNotRemovedMapEvent = 0;
				int troopsRemoved = 0;
				int troopsMapEvent = 0;

				var hideouts = Hideout.All;
				if (hideouts != null)
				{
					for (int i = hideouts.Count - 1; i >= 0; i--)
					{
						var hideout = hideouts[i];
						if (hideout == null)
							continue;
						var parties = hideout.GetDefenderParties(MapEvent.BattleTypes.Hideout)?.ToList();
						if (parties?.Count > 0)
						{
							foreach (var party in parties)
							{
								if (party?.MobileParty == null)
									continue;
								if (party.MobileParty.Party?.MapEvent != null)
								{
									troopsMapEvent += party.MemberRoster?.TotalManCount ?? 0;
									partiesNotRemovedMapEvent++;
									continue;
								}
								troopsRemoved += party.MemberRoster?.TotalManCount ?? 0;
								DestroyPartyAction.Apply(null, party.MobileParty);
								partiesRemoved++;
							}

							if (!hideout.Owner.Settlement.IsVisible)
								hideoutsRemoved++;
							else
								hideoutsNotRemoved++;
						}
					}
				}
				InformationManager.DisplayMessage(new InformationMessage(
					"Hideout Removal:" +
					$"\n {hideoutsRemoved} hideouts removed" +
					$"\n {hideoutsNotRemoved} hideouts not removed" +
					$"\n {partiesRemoved} parties removed from hideouts ({troopsRemoved} troops)" +
					$"\n {partiesNotRemovedMapEvent} parties could not be removed due to map events ({troopsMapEvent} troops)"));
			}
		};
		#endregion
	}
}
