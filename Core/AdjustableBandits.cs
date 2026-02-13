using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
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
		private const string NorthernPiratesClanId = "northern_pirates";
		private const string SouthernPiratesClanId = "southern_pirates";
		private const string DesertBanditClanId = "desert_bandits";
		private const string SteppeBanditClanId = "steppe_bandits";
		private const string ForestBanditClanId = "forest_bandits";
		private const string MountainBanditClanId = "mountain_bandits";
		private const string DeserterClanId = "deserters";
		private static readonly HashSet<MBGUID> ScaledPartyIds = new HashSet<MBGUID>();
		private static readonly HashSet<MBGUID> LoggedPartyIds = new HashSet<MBGUID>();
		private static MethodInfo CreateBanditPartyMethod;
		private static MethodInfo CreateLooterPartyMethod;
		private static readonly HashSet<string> LoggedSpawnSignatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static readonly Random SpawnRandom = new Random();
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
				LogError($"Adjustable Bandits: failed to read MCM settings: {exc}");
			}

			return Settings ?? FallbackSettings;
		}

		public static bool ResetSettingsFromJson(out string message)
		{
			message = null;
			try
			{
				var settings = GetSettings();
				if (settings == null)
				{
					message = "Settings are not available.";
					return false;
				}

				if (!SettingsDefaultsJson.TryLoad(out var path, out var error, out var applyAction))
				{
					message = error;
					LogError($"defaults: {error}");
					return false;
				}

				applyAction(settings);
				settings.NormalizeMinMaxValues();

				TrySaveSettings(settings);

				message = $"Defaults loaded from JSON: {path}";
				WriteLog($"defaults: reset applied from '{path}'");
				return true;
			}
			catch (Exception exc)
			{
				message = $"Failed to reset defaults: {exc.Message}";
				LogError($"defaults: reset failed: {exc}");
				return false;
			}
		}

		private static void TrySaveSettings(MCMSettings settings)
		{
			try
			{
				var saveMethod = settings.GetType().GetMethod("Save", BindingFlags.Public | BindingFlags.Instance);
				saveMethod?.Invoke(settings, null);
			}
			catch (Exception saveExc)
			{
				LogError($"defaults: save failed: {saveExc}");
			}
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
					{
						campaignGameStarter.AddModel(new AdjustableBanditsDensityModel());
						campaignGameStarter.AddBehavior(new BanditMinimumSpawnBehavior());
					}
					else
						throw new Exception($"Unknown {nameof(gameStarterObject)}: '{gameStarterObject?.GetType()}'");
				}
			}
			catch (Exception exc)
			{
				var text = $"ERROR: Adjustable Bandits failed to initialize ({nameof(OnGameStart)}):";
				InformationManager.DisplayMessage(new InformationMessage(text + exc.GetType().ToString(), new Color(1f, 0f, 0f)));
				LogError(text + "\n" + exc);
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
				LogError(text + "\n" + exc);
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
				LogError(text + "\n" + exc);
			}
		}

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
				var targetTotal = (int)Math.Round(totalCount * multiplier);
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
				var addCount = (int)Math.Round(element.Number * (multiplier - 1f));
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
			var minimum = GetMinSupportedNumberOfPartiesForClan(clan);
			if (minimum > limit)
				limit = minimum;
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
				case NorthernPiratesClanId:
					return settings.NumberOfMaximumNorthernPirateParties;
				case SouthernPiratesClanId:
					return settings.NumberOfMaximumSouthernPirateParties;
				case DesertBanditClanId:
					return settings.NumberOfMaximumDesertBanditParties;
				case SteppeBanditClanId:
					return settings.NumberOfMaximumSteppeBanditParties;
				case ForestBanditClanId:
					return settings.NumberOfMaximumForestBanditParties;
				case MountainBanditClanId:
					return settings.NumberOfMaximumMountainBanditParties;
				case DeserterClanId:
					return settings.NumberOfMaximumDeserterParties;
				default:
					return settings.NumberOfMaximumBanditParties;
			}
		}

		public static int GetMinSupportedNumberOfPartiesForClan(Clan clan)
		{
			var settings = GetSettings();
			var clanId = GetBanditClanId(clan);
			switch (clanId)
			{
				case LooterClanId:
					return settings.NumberOfMinimumLooterParties;
				case SeaRaiderClanId:
					return settings.NumberOfMinimumSeaRaiderParties;
				case NorthernPiratesClanId:
					return settings.NumberOfMinimumNorthernPirateParties;
				case SouthernPiratesClanId:
					return settings.NumberOfMinimumSouthernPirateParties;
				case DesertBanditClanId:
					return settings.NumberOfMinimumDesertBanditParties;
				case SteppeBanditClanId:
					return settings.NumberOfMinimumSteppeBanditParties;
				case ForestBanditClanId:
					return settings.NumberOfMinimumForestBanditParties;
				case MountainBanditClanId:
					return settings.NumberOfMinimumMountainBanditParties;
				case DeserterClanId:
					return settings.NumberOfMinimumDeserterParties;
				default:
					return 0;
			}
		}

		public static void EnsureMinimumParties()
		{
			try
			{
				var parties = MobileParty.All;
				if (parties == null)
					return;

				var minimumTargets = GetMinimumPartyTargets();
				if (minimumTargets.Count == 0)
					return;

				var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				foreach (var party in parties)
				{
					var clanId = GetBanditClanId(party);
					if (string.IsNullOrWhiteSpace(clanId))
						continue;
					if (!minimumTargets.ContainsKey(clanId))
						continue;
					counts[clanId] = counts.TryGetValue(clanId, out var value) ? value + 1 : 1;
				}

				foreach (var target in minimumTargets)
				{
					var clanId = target.Key;
					var minimum = target.Value;
					if (minimum <= 0)
						continue;

					var maximum = GetMaxSupportedNumberOfPartiesForClanId(clanId);
					if (maximum >= 0 && minimum > maximum)
						minimum = maximum;

					counts.TryGetValue(clanId, out var current);
					var toSpawn = minimum - current;
					if (toSpawn <= 0)
						continue;

					SpawnBanditParties(clanId, toSpawn);
				}
			}
			catch (Exception exc)
			{
				WriteLog($"minimums: failed to enforce daily minimums: {exc}");
			}
		}

		private static Dictionary<string, int> GetMinimumPartyTargets()
		{
			var settings = GetSettings();
			return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
			{
				[LooterClanId] = settings.NumberOfMinimumLooterParties,
				[DeserterClanId] = settings.NumberOfMinimumDeserterParties,
				[SeaRaiderClanId] = settings.NumberOfMinimumSeaRaiderParties,
				[NorthernPiratesClanId] = settings.NumberOfMinimumNorthernPirateParties,
				[SouthernPiratesClanId] = settings.NumberOfMinimumSouthernPirateParties,
				[MountainBanditClanId] = settings.NumberOfMinimumMountainBanditParties,
				[ForestBanditClanId] = settings.NumberOfMinimumForestBanditParties,
				[DesertBanditClanId] = settings.NumberOfMinimumDesertBanditParties,
				[SteppeBanditClanId] = settings.NumberOfMinimumSteppeBanditParties,
			};
		}

		private static int GetMaxSupportedNumberOfPartiesForClanId(string clanId)
		{
			var settings = GetSettings();
			switch (clanId)
			{
				case LooterClanId:
					return settings.NumberOfMaximumLooterParties;
				case SeaRaiderClanId:
					return settings.NumberOfMaximumSeaRaiderParties;
				case NorthernPiratesClanId:
					return settings.NumberOfMaximumNorthernPirateParties;
				case SouthernPiratesClanId:
					return settings.NumberOfMaximumSouthernPirateParties;
				case DesertBanditClanId:
					return settings.NumberOfMaximumDesertBanditParties;
				case SteppeBanditClanId:
					return settings.NumberOfMaximumSteppeBanditParties;
				case ForestBanditClanId:
					return settings.NumberOfMaximumForestBanditParties;
				case MountainBanditClanId:
					return settings.NumberOfMaximumMountainBanditParties;
				case DeserterClanId:
					return settings.NumberOfMaximumDeserterParties;
				default:
					return settings.NumberOfMaximumBanditParties;
			}
		}

		private static void SpawnBanditParties(string clanId, int count)
		{
			var clan = FindClan(clanId);
			if (clan == null)
			{
				WriteLog($"minimums: clan not found for '{clanId}'");
				return;
			}

			for (var i = 0; i < count; i++)
			{
				if (!TryCreateBanditParty(clanId, clan))
					break;
			}
		}

		private static Clan FindClan(string clanId)
		{
			var clans = Clan.All;
			if (clans == null)
				return null;
			return clans.FirstOrDefault(clan =>
				string.Equals(clan?.StringId, clanId, StringComparison.OrdinalIgnoreCase));
		}

		private static Settlement GetSpawnSettlement(Clan clan, Hideout hideout)
		{
			if (hideout?.Settlement != null)
				return hideout.Settlement;

			if (clan?.HomeSettlement != null)
				return clan.HomeSettlement;

			var settlements = Settlement.All?.Where(settlement => settlement != null).ToList();
			if (settlements == null || settlements.Count == 0)
				return null;

			return settlements[SpawnRandom.Next(settlements.Count)];
		}

		private static Hideout GetSpawnHideout(Clan clan)
		{
			var hideouts = Hideout.All?.Where(hideout => hideout?.Settlement != null).ToList();
			if (hideouts != null && hideouts.Count > 0)
				return hideouts[SpawnRandom.Next(hideouts.Count)];

			return clan?.HomeSettlement?.Hideout;
		}

		private static object GetSpawnPosition(Settlement settlement, Hideout hideout)
		{
			// Get position from hideout's settlement or the settlement itself
			var targetSettlement = hideout?.Settlement ?? settlement;
			if (targetSettlement != null)
				return targetSettlement.GatePosition;

			// Fallback: use a random settlement's position
			var anySettlement = Settlement.All?.FirstOrDefault();
			if (anySettlement != null)
				return anySettlement.GatePosition;

			// Last resort: return default/invalid position
			return Activator.CreateInstance(typeof(CampaignVec2).Assembly.GetType("TaleWorlds.Library.CampaignVec2"));
		}

		private static bool TryCreateBanditParty(string clanId, Clan clan)
		{
			var method = clanId == LooterClanId ? GetCreateLooterPartyMethod() : GetCreateBanditPartyMethod();
			if (method == null)
			{
				WriteLog("minimums: missing party creation method");
				return false;
			}

			var args = BuildSpawnArguments(method, clanId, clan);
			if (args == null)
			{
				LogSpawnSignature(method, "unsupported party creation signature");
				return false;
			}

			try
			{
				method.Invoke(null, args);
				return true;
			}
			catch (Exception exc)
			{
				WriteLog($"minimums: failed to create party for '{clanId}': {exc}");
				return false;
			}
		}

		private static MethodInfo GetCreateBanditPartyMethod()
		{
			if (CreateBanditPartyMethod != null)
				return CreateBanditPartyMethod;
			CreateBanditPartyMethod = FindCreatePartyMethod("CreateBanditParty");
			return CreateBanditPartyMethod;
		}

		private static MethodInfo GetCreateLooterPartyMethod()
		{
			if (CreateLooterPartyMethod != null)
				return CreateLooterPartyMethod;
			CreateLooterPartyMethod = FindCreatePartyMethod("CreateLooterParty");
			return CreateLooterPartyMethod;
		}

		private static MethodInfo FindCreatePartyMethod(string methodName)
		{
			var methods = typeof(BanditPartyComponent)
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
				.Where(method => method.Name == methodName)
				.ToList();

			if (methods.Count == 0)
				return null;

			return methods.FirstOrDefault(method => method.GetParameters()
				.Any(parameter => parameter.ParameterType == typeof(Clan))) ?? methods[0];
		}

		private static object[] BuildSpawnArguments(MethodInfo method, string clanId, Clan clan)
		{
			var parameters = method.GetParameters();
			var args = new object[parameters.Length];

			// Get spawn location and position
			var hideout = GetSpawnHideout(clan);
			var settlement = GetSpawnSettlement(clan, hideout);
			var spawnPosition = GetSpawnPosition(settlement, hideout);

			for (var i = 0; i < parameters.Length; i++)
			{
				var param = parameters[i];
				var paramType = param.ParameterType;
				if (param.HasDefaultValue)
					args[i] = param.DefaultValue;
				else if (paramType == typeof(string))
					args[i] = clanId; // Use clan ID as base for party string ID
				else if (paramType == typeof(Clan))
					args[i] = clan;
				else if (paramType == typeof(Settlement))
					args[i] = settlement;
				else if (paramType == typeof(Hideout))
					args[i] = hideout;
				else if (paramType == typeof(PartyTemplateObject))
					args[i] = clan?.DefaultPartyTemplate; // Use clan's default party template for troops
				else if (paramType.Name == "CampaignVec2")
					args[i] = spawnPosition; // Use proper spawn position
				else if (paramType == typeof(MobileParty))
					args[i] = null;
				else if (paramType.IsValueType)
					args[i] = Activator.CreateInstance(paramType);
				else
					args[i] = null;
			}
			return args;
		}

		private static void LogSpawnSignature(MethodInfo method, string reason)
		{
			var signature = BuildSignature(method);
			if (!LoggedSpawnSignatures.Add(signature))
				return;
			WriteLog($"minimums: {reason} '{signature}'");
		}

		private static string BuildSignature(MethodInfo method)
		{
			var parameters = method.GetParameters();
			var typeNames = parameters
				.Select(param => param.ParameterType.Name)
				.ToArray();
			return $"{method.Name}({string.Join(", ", typeNames)})";
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
				case NorthernPiratesClanId:
					return settings.NorthernPirateMultiplier;
				case SouthernPiratesClanId:
					return settings.SouthernPirateMultiplier;
				case DesertBanditClanId:
					return settings.DesertBanditMultiplier;
				case SteppeBanditClanId:
					return settings.SteppeBanditMultiplier;
				case ForestBanditClanId:
					return settings.ForestBanditMultiplier;
				case MountainBanditClanId:
					return settings.MountainBanditMultiplier;
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
				return NorthernPiratesClanId;
			if (id.Contains("southern_pirate"))
				return SouthernPiratesClanId;
			if (id.Contains("sea_raider"))
				return SeaRaiderClanId;
			if (id.Contains("desert_bandit"))
				return DesertBanditClanId;
			if (id.Contains("steppe_bandit"))
				return SteppeBanditClanId;
			if (id.Contains("forest_bandit"))
				return ForestBanditClanId;
			if (id.Contains("mountain_bandit"))
				return MountainBanditClanId;
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

		internal static void LogError(string message)
		{
			try
			{
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

		public static void LogAction(string message)
		{
			WriteLog($"action: {message}");
		}

		public static void ResetSettingsFromJsonDefaults()
		{
			if (ResetSettingsFromJson(out var message))
			{
				InformationManager.DisplayMessage(new InformationMessage(message));
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(message ?? "Failed to apply defaults from JSON.", new Color(1f, 0f, 0f)));
			}
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

		public static void ShowPartyDetails()
		{
			try
			{
				var parties = MobileParty.All;
				if (parties == null || parties.Count == 0)
				{
					InformationManager.DisplayMessage(new InformationMessage("No parties found."));
					return;
				}

				var clans = new[]
				{
					LooterClanId,
					DeserterClanId,
					SeaRaiderClanId,
					NorthernPiratesClanId,
					SouthernPiratesClanId,
					DesertBanditClanId,
					SteppeBanditClanId,
					ForestBanditClanId,
					MountainBanditClanId,
				};

				var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				foreach (var clanId in clans)
				{
					counts[clanId] = 0;
					totals[clanId] = 0;
				}

				// Get player position for distance calculations
				var playerParty = MobileParty.MainParty;
				var playerPos = playerParty?.GetPosition2D ?? new Vec2(0, 0);

				LogAction("party-details: begin");
				LogAction($"party-details: Player position: ({playerPos.X:0.0}, {playerPos.Y:0.0})");
				LogAction("party-details: ===== DETAILED PARTY LIST =====");

				// Track distance ranges
				var nearbyCount = 0;    // < 50 units
				var midCount = 0;       // 50-150 units
				var farCount = 0;       // > 150 units

				foreach (var party in parties)
				{
					if (party == null)
						continue;
					if (!IsTargetParty(party))
						continue;
					var clanId = GetBanditClanId(party);
					if (!counts.ContainsKey(clanId))
						continue;

					var size = party.MemberRoster?.TotalManCount ?? 0;
					var prisonerCount = party.PrisonRoster?.TotalManCount ?? 0;
					var name = party.Name?.ToString() ?? "(no name)";
					var stringId = party.StringId ?? "(no id)";
					var position = party.GetPosition2D;
					var multiplier = GetPartySizeMultiplier(party);
					var settlement = party.CurrentSettlement?.Name?.ToString() ?? party.TargetSettlement?.Name?.ToString() ?? "none";

					// Calculate distance from player
					var distance = playerPos.Distance(position);

					// Track distance ranges
					if (distance < 50f)
						nearbyCount++;
					else if (distance < 150f)
						midCount++;
					else
						farCount++;

					counts[clanId] = counts[clanId] + 1;
					totals[clanId] = totals[clanId] + size;

					// Detailed log entry for each party
					LogAction($"party-details: [{clanId}] '{name}' (ID: {stringId})");
					LogAction($"  - Size: {size} troops, Prisoners: {prisonerCount}, Multiplier: {multiplier:0.###}");
					LogAction($"  - Position: ({position.X:0.0}, {position.Y:0.0}), Distance from player: {distance:0.0} units");
					LogAction($"  - Settlement: {settlement}");
				}


				// Build summary
				LogAction("party-details: ===== SUMMARY =====");
				LogAction($"party-details: Location: Player at ({playerPos.X:0.0}, {playerPos.Y:0.0})");
				LogAction($"party-details: Distance ranges - Nearby (<50): {nearbyCount}, Mid (50-150): {midCount}, Far (>150): {farCount}");

				var summaryLines = new List<string>();
				summaryLines.Add("===== Bandit Party Summary =====");
				summaryLines.Add("");

				var grandTotalParties = 0;
				var grandTotalTroops = 0;

				foreach (var clanId in clans)
				{
					var count = counts[clanId];
					var total = totals[clanId];
					var avg = count > 0 ? (double)total / count : 0d;

					if (count > 0)
					{
						summaryLines.Add($"{clanId}: {count} parties, {total} troops, avg {avg:0.0}");
						LogAction($"party-details: summary: {clanId}: parties={count} total={total} avg={avg:0.0}");

						grandTotalParties += count;
						grandTotalTroops += total;
					}
				}

				summaryLines.Add("");
				summaryLines.Add($"TOTAL: {grandTotalParties} parties, {grandTotalTroops} troops");
				LogAction($"party-details: summary: GRAND TOTAL: parties={grandTotalParties} troops={grandTotalTroops}");
				LogAction("party-details: end");

				var summary = string.Join("\n", summaryLines);
				// Show pop-up dialog
				InformationManager.ShowInquiry(
					new InquiryData(
						"Bandit Party Details",
						summary,
						true,
						false,
						"OK",
						null,
						null,
						null
					),
					true
				);
			}
			catch (Exception exc)
			{
				LogError($"party-details: failed to gather details: {exc}");
			}
		}
	}
}
