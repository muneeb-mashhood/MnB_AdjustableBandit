using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using System.IO;

namespace AdjustableBandits
{
	public class AdjustableBandits : MBSubModuleBase
	{
		public static MCMSettings Settings { get; private set; }
		private static readonly MCMSettings FallbackSettings = new MCMSettings();
		private const string LooterClanId = "looters";
		private const string SeaRaiderClanId = "sea_raiders";
		private const string CorsairClanId = "corsairs";
		private const string DesertBanditClanId = "desert_bandits";
		private const string SteppeBanditClanId = "steppe_bandits";
		private const string ForestBanditClanId = "forest_bandits";
		private const string DeserterClanId = "deserters";
		private static readonly HashSet<MBGUID> ScaledPartyIds = new HashSet<MBGUID>();
		private static readonly HashSet<MBGUID> LoggedPartyIds = new HashSet<MBGUID>();
		private static readonly string LogPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			"Mount and Blade II Bannerlord",
			"Configs",
			"ModLogs",
			"AdjustableBandits.log");

		private bool isInitialized;

		public static MCMSettings GetSettings()
		{
			if (Settings != null)
				return Settings;

			try
			{
				Settings = GlobalSettings<MCMSettings>.Instance;
			}
			catch (Exception exc)
			{
				FileLog.Log($"Adjustable Bandits: failed to read MCM settings: {exc}");
			}

			return Settings ?? FallbackSettings;
		}

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
		{
			try
			{
				base.OnGameStart(game, gameStarterObject);
				EnsureInitialized();
				if (game.GameType is Campaign)
				{
					if (gameStarterObject is CampaignGameStarter campaignGameStarter)
						campaignGameStarter.AddModel(new AdjustableBanditsDensityModel());
					else
						throw new Exception($"Unknown {nameof(gameStarterObject)}: '{gameStarterObject?.GetType()}'");
				}
			}
			catch (Exception exc)
			{
				var text = $"ERROR: Adjustable Bandits failed to initialize ({nameof(OnGameStart)}):";
				InformationManager.DisplayMessage(new InformationMessage(text + exc.GetType().ToString(), new Color(1f, 0f, 0f)));
				FileLog.Log(text + "\n" + exc.ToString());
			}
		}

		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			try
			{
				base.OnBeforeInitialModuleScreenSetAsRoot();
				EnsureInitialized();
			}
			catch (Exception exc)
			{
				var text = $"ERROR: Adjustable Bandits failed to initialize ({nameof(OnBeforeInitialModuleScreenSetAsRoot)}):";
				InformationManager.DisplayMessage(new InformationMessage(text + exc.GetType().ToString(), new Color(1f, 0f, 0f)));
				FileLog.Log(text + "\n" + exc.ToString());
			}
		}

		private void EnsureInitialized()
		{
			if (isInitialized)
				return;

			try
			{
				HarmonyPatches.Initialize();
				isInitialized = true;
			}
			catch (Exception exc)
			{
				var text = $"Adjustable Bandits failed to apply Harmony patches:";
				InformationManager.DisplayMessage(new InformationMessage(text + " " + exc.GetType().ToString(), new Color(1f, 0f, 0f)));
				FileLog.Log(text + "\n" + exc.ToString());
			}
		}

		public static void ModifyVariables(MobileParty mobileParty, ref float f, ref float f2, ref float f3)
		{
			if (mobileParty == null || !mobileParty.IsBandit)
				return;

			var modifier = GetPartySizeMultiplier(mobileParty);

			f = Limit(f * modifier);
			f2 = Limit(f2 * modifier);
			f3 = Limit(f3 * modifier);
		}
		private static float Limit(float f) => f > 1f ? f : 1f;

		public static void ApplyPartySizeMultiplierOnce(MobileParty party)
		{
			if (party == null)
				return;

			LogPartyOnce(party);

			if (ScaledPartyIds.Contains(party.Id))
				return;

			if (ApplyPartySizeMultiplier(party))
				ScaledPartyIds.Add(party.Id);
		}

		private static bool ApplyPartySizeMultiplier(MobileParty party)
		{
			if (party == null || !IsTargetParty(party))
				return false;
			var roster = party.MemberRoster;
			if (roster == null)
				return false;

			var multiplier = GetPartySizeMultiplier(party);
			var totalCount = roster.TotalManCount;
			if (totalCount <= 0)
				return false;

			if (multiplier < 1f)
			{
				var heroCount = roster.TotalHeroes;
				var regularCount = roster.TotalRegulars;
				var targetTotal = (int)MathF.Round(totalCount * multiplier);
				var minTotal = heroCount + (regularCount > 0 ? 1 : 0);
				if (targetTotal < minTotal)
					targetTotal = minTotal;
				var removeCount = totalCount - targetTotal;
				if (removeCount <= 0)
					return false;
				if (removeCount > regularCount)
					removeCount = regularCount;
				if (removeCount <= 0)
					return false;
				WriteLog($"size: {totalCount} -> {targetTotal} (remove {removeCount}) party='{party.Name}' clanId='{GetBanditClanId(party)}' multiplier={multiplier:0.###}");
				roster.RemoveNumberOfNonHeroTroopsRandomly(removeCount);
				return true;
			}

			if (multiplier <= 1f)
				return false;

			var elements = roster.GetTroopRoster();
			if (elements == null || elements.Count == 0)
				return false;

			var applied = false;
			var addedTotal = 0;

			foreach (var element in elements)
			{
				var character = element.Character;
				if (character == null)
					continue;
				var addCount = (int)MathF.Round(element.Number * (multiplier - 1f));
				if (addCount <= 0)
					continue;
				roster.AddToCounts(character, addCount, false, 0, 0, true, -1);
				addedTotal += addCount;
				applied = true;
			}

			if (applied)
				WriteLog($"size: {totalCount} -> {totalCount + addedTotal} (add {addedTotal}) party='{party.Name}' clanId='{GetBanditClanId(party)}' multiplier={multiplier:0.###}");
			return applied;
		}

		public static void EnforcePartyCountLimit(MobileParty party)
		{
			if (party == null || !IsTargetParty(party))
				return;

			var clan = party.BanditPartyComponent?.Clan ?? party.MapFaction as Clan;
			if (clan == null)
				return;

			var limit = GetMaxSupportedNumberOfPartiesForClan(clan);
			if (limit < 0)
				return;

			var bandits = MobileParty.AllBanditParties;
			if (bandits == null)
				return;

			var clanId = GetBanditClanId(clan);
			var count = bandits.Count(p => GetBanditClanId(p) == clanId);
			if (count > limit)
				DestroyPartyAction.Apply(null, party);
		}

		public static int GetMaxSupportedNumberOfPartiesForClan(Clan clan)
		{
			var settings = GetSettings();
			var clanId = GetBanditClanId(clan);
			switch (clanId)
			{
				case LooterClanId:
					return settings.NumberOfMaximumLooterParties;
				case SeaRaiderClanId:
					return settings.NumberOfMaximumSeaRaiderParties;
				case CorsairClanId:
					return settings.NumberOfMaximumCorsairParties;
				case DesertBanditClanId:
					return settings.NumberOfMaximumDesertBanditParties;
				case SteppeBanditClanId:
					return settings.NumberOfMaximumSteppeBanditParties;
				case ForestBanditClanId:
					return settings.NumberOfMaximumForestBanditParties;
				case DeserterClanId:
					return settings.NumberOfMaximumDeserterParties;
				default:
					return settings.NumberOfMaximumBanditParties;
			}
		}

		private static float GetPartySizeMultiplier(MobileParty party)
		{
			var settings = GetSettings();
			var clanId = GetBanditClanId(party);
			switch (clanId)
			{
				case LooterClanId:
					return settings.LooterMultiplier;
				case SeaRaiderClanId:
					return settings.SeaRaiderMultiplier;
				case CorsairClanId:
					return settings.CorsairMultiplier;
				case DesertBanditClanId:
					return settings.DesertBanditMultiplier;
				case SteppeBanditClanId:
					return settings.SteppeBanditMultiplier;
				case ForestBanditClanId:
					return settings.ForestBanditMultiplier;
				case DeserterClanId:
					return settings.DeserterMultiplier;
				default:
					return settings.BanditMultiplier;
			}
		}

		private static string GetBanditClanId(MobileParty party)
		{
			return GetBanditClanId(party?.BanditPartyComponent?.Clan ?? party?.MapFaction as Clan);
		}

		private static string GetBanditClanId(Clan clan)
		{
			var id = clan?.StringId?.ToLowerInvariant() ?? string.Empty;
			if (id.Contains("looter"))
				return LooterClanId;
			if (id.Contains("northern_pirate"))
				return SeaRaiderClanId;
			if (id.Contains("southern_pirate"))
				return CorsairClanId;
			if (id.Contains("sea_raider"))
				return SeaRaiderClanId;
			if (id.Contains("corsair"))
				return CorsairClanId;
			if (id.Contains("desert_bandit"))
				return DesertBanditClanId;
			if (id.Contains("steppe_bandit"))
				return SteppeBanditClanId;
			if (id.Contains("forest_bandit"))
				return ForestBanditClanId;
			if (id.Contains("deserter"))
				return DeserterClanId;
			return id;
		}

		private static bool IsTargetParty(MobileParty party)
		{
			if (party.IsBandit)
				return true;

			var clanId = GetBanditClanId(party);
			return clanId == DeserterClanId;
		}

		private static void LogPartyOnce(MobileParty party)
		{
			if (party == null)
				return;

			if (LoggedPartyIds.Contains(party.Id))
				return;

			LoggedPartyIds.Add(party.Id);
			var clanId = GetBanditClanId(party);
			var name = party.Name?.ToString() ?? "(no name)";
			var isBandit = party.IsBandit;
			var multiplier = GetPartySizeMultiplier(party);
			WriteLog($"party='{name}' clanId='{clanId}' isBandit={isBandit} multiplier={multiplier:0.###}");
		}

		private static void WriteLog(string message)
		{
			try
			{
				if (!GetSettings().EnableLogging)
					return;
				var dir = Path.GetDirectoryName(LogPath);
				if (!string.IsNullOrWhiteSpace(dir))
					Directory.CreateDirectory(dir);
				File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
			}
			catch
			{
				// Ignore logging failures.
			}
		}
	}
}
