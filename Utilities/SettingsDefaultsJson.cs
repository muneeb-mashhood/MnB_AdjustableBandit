using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using TaleWorlds.Library;

namespace AdjustableBandits
{
  internal static class SettingsDefaultsJson
  {
    internal const string DefaultsFileName = "adjustablebandits.defaults.json";
    private static readonly CanonicalDefaultsModel HardDefaults = new CanonicalDefaultsModel
    {
      Looters = new PartyGroup { Multiplier = 3.0f, NumberOfMinParties = 120, NumberOfMaxParties = 200 },
      Bandit = new PartyGroup { Multiplier = 1.0f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      DesertBandit = new PartyGroup { Multiplier = 0.6f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      SteppeBandit = new PartyGroup { Multiplier = 0.6f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      ForestBandit = new PartyGroup { Multiplier = 0.6f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      MountainBandit = new PartyGroup { Multiplier = 0.6f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      Deserter = new PartyGroup { Multiplier = 1.0f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      SeaRaider = new PartyGroup { Multiplier = 0.6f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      NorthernPirate = new PartyGroup { Multiplier = 0.6f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      SouthernPirate = new PartyGroup { Multiplier = 0.6f, NumberOfMinParties = 20, NumberOfMaxParties = 80 },
      Hideout = new HideoutGroup
      {
        NumberOfInitialHideoutsAtEachBanditFaction = 3,
        NumberOfMaximumHideoutsAtEachBanditFaction = 10,
        NumberOfMinimumBanditPartiesInAHideoutToInfestIt = 2,
        NumberOfMaximumBanditPartiesAroundEachHideout = 8,
        NumberOfMaximumBanditPartiesInEachHideout = 4,
        NumberOfMaximumTroopCountForFirstFightInHideoutFactor = 1.0f,
        NumberOfMaximumTroopCountForBossFightInHideoutFactor = 1.0f,
        SpawnPercentageForFirstFightInHideoutMission = 0.75f,
        NumberOfMinimumBanditTroopsInHideoutMission = 10,
        PlayerMaximumTroopCountForHideoutMission = 10
      },
      BanditPartySizeLimit = 20,
      EnableLogging = false
    };

    private class CanonicalDefaultsModel
    {
      public PartyGroup Looters { get; set; }
      public PartyGroup Bandit { get; set; }
      public PartyGroup DesertBandit { get; set; }
      public PartyGroup SteppeBandit { get; set; }
      public PartyGroup ForestBandit { get; set; }
      public PartyGroup MountainBandit { get; set; }
      public PartyGroup Deserter { get; set; }
      public PartyGroup SeaRaider { get; set; }
      public PartyGroup NorthernPirate { get; set; }
      public PartyGroup SouthernPirate { get; set; }

      public HideoutGroup Hideout { get; set; }

      public int? BanditPartySizeLimit { get; set; }
      public bool? EnableLogging { get; set; }
    }

    private class PartyGroup
    {
      public float? Multiplier { get; set; }
      public int? NumberOfMinParties { get; set; }
      public int? NumberOfMaxParties { get; set; }
    }

    private class HideoutGroup
    {
      public int? NumberOfInitialHideoutsAtEachBanditFaction { get; set; }
      public int? NumberOfMaximumHideoutsAtEachBanditFaction { get; set; }
      public int? NumberOfMinimumBanditPartiesInAHideoutToInfestIt { get; set; }
      public int? NumberOfMaximumBanditPartiesAroundEachHideout { get; set; }
      public int? NumberOfMaximumBanditPartiesInEachHideout { get; set; }
      public float? NumberOfMaximumTroopCountForFirstFightInHideoutFactor { get; set; }
      public float? NumberOfMaximumTroopCountForBossFightInHideoutFactor { get; set; }
      public float? SpawnPercentageForFirstFightInHideoutMission { get; set; }
      public int? NumberOfMinimumBanditTroopsInHideoutMission { get; set; }
      public int? PlayerMaximumTroopCountForHideoutMission { get; set; }
    }

    private class DefaultsModel
    {
      public PartyGroup Looters { get; set; }
      public PartyGroup Bandit { get; set; }
      public PartyGroup DesertBandit { get; set; }
      public PartyGroup SteppeBandit { get; set; }
      public PartyGroup ForestBandit { get; set; }
      public PartyGroup MountainBandit { get; set; }
      public PartyGroup Deserter { get; set; }
      public PartyGroup SeaRaider { get; set; }
      public PartyGroup NorthernPirate { get; set; }
      public PartyGroup SouthernPirate { get; set; }

      public HideoutGroup Hideout { get; set; }

      public float? BanditMultiplier { get; set; }
      public float? DesertBanditMultiplier { get; set; }
      public float? SteppeBanditMultiplier { get; set; }
      public float? ForestBanditMultiplier { get; set; }
      public float? MountainBanditMultiplier { get; set; }
      public float? LooterMultiplier { get; set; }
      public float? DeserterMultiplier { get; set; }
      public float? SeaRaiderMultiplier { get; set; }
      public float? NorthernPirateMultiplier { get; set; }
      public float? SouthernPirateMultiplier { get; set; }

      public int? BanditPartySizeLimit { get; set; }

      public int? NumberOfMaximumLooterParties { get; set; }
      public int? NumberOfMinimumLooterParties { get; set; }
      public int? NumberOfMaximumDeserterParties { get; set; }
      public int? NumberOfMinimumDeserterParties { get; set; }
      public int? NumberOfMaximumBanditParties { get; set; }
      public int? NumberOfMaximumDesertBanditParties { get; set; }
      public int? NumberOfMinimumDesertBanditParties { get; set; }
      public int? NumberOfMaximumSteppeBanditParties { get; set; }
      public int? NumberOfMinimumSteppeBanditParties { get; set; }
      public int? NumberOfMaximumForestBanditParties { get; set; }
      public int? NumberOfMinimumForestBanditParties { get; set; }
      public int? NumberOfMaximumMountainBanditParties { get; set; }
      public int? NumberOfMinimumMountainBanditParties { get; set; }
      public int? NumberOfMaximumSeaRaiderParties { get; set; }
      public int? NumberOfMinimumSeaRaiderParties { get; set; }
      public int? NumberOfMaximumNorthernPirateParties { get; set; }
      public int? NumberOfMinimumNorthernPirateParties { get; set; }
      public int? NumberOfMaximumSouthernPirateParties { get; set; }
      public int? NumberOfMinimumSouthernPirateParties { get; set; }

      public int? NumberOfInitialHideoutsAtEachBanditFaction { get; set; }
      public int? NumberOfMaximumHideoutsAtEachBanditFaction { get; set; }
      public int? NumberOfMinimumBanditPartiesInAHideoutToInfestIt { get; set; }
      public int? NumberOfMaximumBanditPartiesAroundEachHideout { get; set; }
      public int? NumberOfMaximumBanditPartiesInEachHideout { get; set; }
      public float? NumberOfMaximumTroopCountForFirstFightInHideoutFactor { get; set; }
      public float? NumberOfMaximumTroopCountForBossFightInHideoutFactor { get; set; }
      public float? SpawnPercentageForFirstFightInHideoutMission { get; set; }
      public int? NumberOfMinimumBanditTroopsInHideoutMission { get; set; }
      public int? PlayerMaximumTroopCountForHideoutMission { get; set; }

      public bool? EnableLogging { get; set; }
    }

    internal static bool TryLoad(out string loadedPath, out string error, out Action<MCMSettings> applyAction)
    {
      loadedPath = null;
      error = null;
      applyAction = null;

      try
      {
        var path = ResolveDefaultsPath();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
          error = $"Defaults JSON not found ({DefaultsFileName}).";
          return false;
        }

        var json = File.ReadAllText(path);
        var serializer = new JavaScriptSerializer();

        var defaultsMap = serializer.DeserializeObject(serializer.Serialize(HardDefaults)) as Dictionary<string, object>;
        if (defaultsMap == null)
        {
          error = "Hardcoded defaults are invalid.";
          return false;
        }

        Dictionary<string, object> jsonMap = null;
        try
        {
          jsonMap = serializer.DeserializeObject(json) as Dictionary<string, object>;
        }
        catch
        {
        }

        var changed = false;
        if (jsonMap == null)
        {
          jsonMap = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
          changed = true;
        }

        if (MergeMissingValues(jsonMap, defaultsMap))
          changed = true;

        if (changed)
        {
          var updatedJson = serializer.Serialize(jsonMap);
          File.WriteAllText(path, updatedJson);
        }

        var defaults = serializer.Deserialize<DefaultsModel>(serializer.Serialize(jsonMap));
        if (defaults == null)
        {
          error = "Defaults JSON is empty or invalid.";
          return false;
        }

        applyAction = settings => ApplyDefaults(settings, defaults);
        loadedPath = path;
        return true;
      }
      catch (Exception exc)
      {
        error = $"Failed to load defaults JSON: {exc.Message}";
        return false;
      }
    }

    private static bool MergeMissingValues(IDictionary<string, object> current, IDictionary<string, object> fallback)
    {
      var changed = false;

      foreach (var fallbackEntry in fallback)
      {
        if (!TryGetKeyIgnoreCase(current, fallbackEntry.Key, out var existingKey))
        {
          current[fallbackEntry.Key] = fallbackEntry.Value;
          changed = true;
          continue;
        }

        var currentValue = current[existingKey];
        if (currentValue == null)
        {
          current[existingKey] = fallbackEntry.Value;
          changed = true;
          continue;
        }

        if (currentValue is IDictionary<string, object> currentObject &&
            fallbackEntry.Value is IDictionary<string, object> fallbackObject)
        {
          if (MergeMissingValues(currentObject, fallbackObject))
            changed = true;
        }
      }

      return changed;
    }

    private static bool TryGetKeyIgnoreCase(IDictionary<string, object> source, string key, out string existingKey)
    {
      foreach (var sourceKey in source.Keys)
      {
        if (string.Equals(sourceKey, key, StringComparison.OrdinalIgnoreCase))
        {
          existingKey = sourceKey;
          return true;
        }
      }

      existingKey = null;
      return false;
    }

    private static string ResolveDefaultsPath()
    {
      var candidates = new List<string>();

      try
      {
        var basePath = BasePath.Name;
        if (!string.IsNullOrWhiteSpace(basePath))
        {
          candidates.Add(Path.Combine(basePath, "Modules", "AdjustableBandits", "ModuleData", DefaultsFileName));
        }
      }
      catch
      {
      }

      try
      {
        var assemblyDir = Path.GetDirectoryName(typeof(AdjustableBandits).Assembly.Location);
        if (!string.IsNullOrWhiteSpace(assemblyDir))
        {
          var moduleRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", ".."));
          candidates.Add(Path.Combine(moduleRoot, "ModuleData", DefaultsFileName));
        }
      }
      catch
      {
      }

      try
      {
        var cwd = Directory.GetCurrentDirectory();
        if (!string.IsNullOrWhiteSpace(cwd))
        {
          candidates.Add(Path.Combine(cwd, "_Module", "ModuleData", DefaultsFileName));
        }
      }
      catch
      {
      }

      return candidates.FirstOrDefault(File.Exists);
    }

    private static void ApplyDefaults(MCMSettings settings, DefaultsModel defaults)
    {
      if (settings == null || defaults == null)
        return;

      Apply(defaults.Looters?.Multiplier, value => settings.LooterMultiplier = value);
      Apply(defaults.Looters?.NumberOfMinParties, value => settings.NumberOfMinimumLooterParties = value);
      Apply(defaults.Looters?.NumberOfMaxParties, value => settings.NumberOfMaximumLooterParties = value);

      Apply(defaults.Bandit?.Multiplier, value => settings.BanditMultiplier = value);
      Apply(defaults.Bandit?.NumberOfMaxParties, value => settings.NumberOfMaximumBanditParties = value);

      Apply(defaults.DesertBandit?.Multiplier, value => settings.DesertBanditMultiplier = value);
      Apply(defaults.DesertBandit?.NumberOfMinParties, value => settings.NumberOfMinimumDesertBanditParties = value);
      Apply(defaults.DesertBandit?.NumberOfMaxParties, value => settings.NumberOfMaximumDesertBanditParties = value);

      Apply(defaults.SteppeBandit?.Multiplier, value => settings.SteppeBanditMultiplier = value);
      Apply(defaults.SteppeBandit?.NumberOfMinParties, value => settings.NumberOfMinimumSteppeBanditParties = value);
      Apply(defaults.SteppeBandit?.NumberOfMaxParties, value => settings.NumberOfMaximumSteppeBanditParties = value);

      Apply(defaults.ForestBandit?.Multiplier, value => settings.ForestBanditMultiplier = value);
      Apply(defaults.ForestBandit?.NumberOfMinParties, value => settings.NumberOfMinimumForestBanditParties = value);
      Apply(defaults.ForestBandit?.NumberOfMaxParties, value => settings.NumberOfMaximumForestBanditParties = value);

      Apply(defaults.MountainBandit?.Multiplier, value => settings.MountainBanditMultiplier = value);
      Apply(defaults.MountainBandit?.NumberOfMinParties, value => settings.NumberOfMinimumMountainBanditParties = value);
      Apply(defaults.MountainBandit?.NumberOfMaxParties, value => settings.NumberOfMaximumMountainBanditParties = value);

      Apply(defaults.Deserter?.Multiplier, value => settings.DeserterMultiplier = value);
      Apply(defaults.Deserter?.NumberOfMinParties, value => settings.NumberOfMinimumDeserterParties = value);
      Apply(defaults.Deserter?.NumberOfMaxParties, value => settings.NumberOfMaximumDeserterParties = value);

      Apply(defaults.SeaRaider?.Multiplier, value => settings.SeaRaiderMultiplier = value);
      Apply(defaults.SeaRaider?.NumberOfMinParties, value => settings.NumberOfMinimumSeaRaiderParties = value);
      Apply(defaults.SeaRaider?.NumberOfMaxParties, value => settings.NumberOfMaximumSeaRaiderParties = value);

      Apply(defaults.NorthernPirate?.Multiplier, value => settings.NorthernPirateMultiplier = value);
      Apply(defaults.NorthernPirate?.NumberOfMinParties, value => settings.NumberOfMinimumNorthernPirateParties = value);
      Apply(defaults.NorthernPirate?.NumberOfMaxParties, value => settings.NumberOfMaximumNorthernPirateParties = value);

      Apply(defaults.SouthernPirate?.Multiplier, value => settings.SouthernPirateMultiplier = value);
      Apply(defaults.SouthernPirate?.NumberOfMinParties, value => settings.NumberOfMinimumSouthernPirateParties = value);
      Apply(defaults.SouthernPirate?.NumberOfMaxParties, value => settings.NumberOfMaximumSouthernPirateParties = value);

      Apply(defaults.Hideout?.NumberOfInitialHideoutsAtEachBanditFaction, value => settings.NumberOfInitialHideoutsAtEachBanditFaction = value);
      Apply(defaults.Hideout?.NumberOfMaximumHideoutsAtEachBanditFaction, value => settings.NumberOfMaximumHideoutsAtEachBanditFaction = value);
      Apply(defaults.Hideout?.NumberOfMinimumBanditPartiesInAHideoutToInfestIt, value => settings.NumberOfMinimumBanditPartiesInAHideoutToInfestIt = value);
      Apply(defaults.Hideout?.NumberOfMaximumBanditPartiesAroundEachHideout, value => settings.NumberOfMaximumBanditPartiesAroundEachHideout = value);
      Apply(defaults.Hideout?.NumberOfMaximumBanditPartiesInEachHideout, value => settings.NumberOfMaximumBanditPartiesInEachHideout = value);
      Apply(defaults.Hideout?.NumberOfMaximumTroopCountForFirstFightInHideoutFactor, value => settings.NumberOfMaximumTroopCountForFirstFightInHideoutFactor = value);
      Apply(defaults.Hideout?.NumberOfMaximumTroopCountForBossFightInHideoutFactor, value => settings.NumberOfMaximumTroopCountForBossFightInHideoutFactor = value);
      Apply(defaults.Hideout?.SpawnPercentageForFirstFightInHideoutMission, value => settings.SpawnPercentageForFirstFightInHideoutMission = value);
      Apply(defaults.Hideout?.NumberOfMinimumBanditTroopsInHideoutMission, value => settings.NumberOfMinimumBanditTroopsInHideoutMission = value);
      Apply(defaults.Hideout?.PlayerMaximumTroopCountForHideoutMission, value => settings.PlayerMaximumTroopCountForHideoutMission = value);

      Apply(defaults.BanditMultiplier, value => settings.BanditMultiplier = value);
      Apply(defaults.DesertBanditMultiplier, value => settings.DesertBanditMultiplier = value);
      Apply(defaults.SteppeBanditMultiplier, value => settings.SteppeBanditMultiplier = value);
      Apply(defaults.ForestBanditMultiplier, value => settings.ForestBanditMultiplier = value);
      Apply(defaults.MountainBanditMultiplier, value => settings.MountainBanditMultiplier = value);
      Apply(defaults.LooterMultiplier, value => settings.LooterMultiplier = value);
      Apply(defaults.DeserterMultiplier, value => settings.DeserterMultiplier = value);
      Apply(defaults.SeaRaiderMultiplier, value => settings.SeaRaiderMultiplier = value);
      Apply(defaults.NorthernPirateMultiplier, value => settings.NorthernPirateMultiplier = value);
      Apply(defaults.SouthernPirateMultiplier, value => settings.SouthernPirateMultiplier = value);

      Apply(defaults.BanditPartySizeLimit, value => settings.BanditPartySizeLimit = value);

      Apply(defaults.NumberOfMaximumLooterParties, value => settings.NumberOfMaximumLooterParties = value);
      Apply(defaults.NumberOfMinimumLooterParties, value => settings.NumberOfMinimumLooterParties = value);
      Apply(defaults.NumberOfMaximumDeserterParties, value => settings.NumberOfMaximumDeserterParties = value);
      Apply(defaults.NumberOfMinimumDeserterParties, value => settings.NumberOfMinimumDeserterParties = value);
      Apply(defaults.NumberOfMaximumBanditParties, value => settings.NumberOfMaximumBanditParties = value);
      Apply(defaults.NumberOfMaximumDesertBanditParties, value => settings.NumberOfMaximumDesertBanditParties = value);
      Apply(defaults.NumberOfMinimumDesertBanditParties, value => settings.NumberOfMinimumDesertBanditParties = value);
      Apply(defaults.NumberOfMaximumSteppeBanditParties, value => settings.NumberOfMaximumSteppeBanditParties = value);
      Apply(defaults.NumberOfMinimumSteppeBanditParties, value => settings.NumberOfMinimumSteppeBanditParties = value);
      Apply(defaults.NumberOfMaximumForestBanditParties, value => settings.NumberOfMaximumForestBanditParties = value);
      Apply(defaults.NumberOfMinimumForestBanditParties, value => settings.NumberOfMinimumForestBanditParties = value);
      Apply(defaults.NumberOfMaximumMountainBanditParties, value => settings.NumberOfMaximumMountainBanditParties = value);
      Apply(defaults.NumberOfMinimumMountainBanditParties, value => settings.NumberOfMinimumMountainBanditParties = value);
      Apply(defaults.NumberOfMaximumSeaRaiderParties, value => settings.NumberOfMaximumSeaRaiderParties = value);
      Apply(defaults.NumberOfMinimumSeaRaiderParties, value => settings.NumberOfMinimumSeaRaiderParties = value);
      Apply(defaults.NumberOfMaximumNorthernPirateParties, value => settings.NumberOfMaximumNorthernPirateParties = value);
      Apply(defaults.NumberOfMinimumNorthernPirateParties, value => settings.NumberOfMinimumNorthernPirateParties = value);
      Apply(defaults.NumberOfMaximumSouthernPirateParties, value => settings.NumberOfMaximumSouthernPirateParties = value);
      Apply(defaults.NumberOfMinimumSouthernPirateParties, value => settings.NumberOfMinimumSouthernPirateParties = value);

      Apply(defaults.NumberOfInitialHideoutsAtEachBanditFaction, value => settings.NumberOfInitialHideoutsAtEachBanditFaction = value);
      Apply(defaults.NumberOfMaximumHideoutsAtEachBanditFaction, value => settings.NumberOfMaximumHideoutsAtEachBanditFaction = value);
      Apply(defaults.NumberOfMinimumBanditPartiesInAHideoutToInfestIt, value => settings.NumberOfMinimumBanditPartiesInAHideoutToInfestIt = value);
      Apply(defaults.NumberOfMaximumBanditPartiesAroundEachHideout, value => settings.NumberOfMaximumBanditPartiesAroundEachHideout = value);
      Apply(defaults.NumberOfMaximumBanditPartiesInEachHideout, value => settings.NumberOfMaximumBanditPartiesInEachHideout = value);
      Apply(defaults.NumberOfMaximumTroopCountForFirstFightInHideoutFactor, value => settings.NumberOfMaximumTroopCountForFirstFightInHideoutFactor = value);
      Apply(defaults.NumberOfMaximumTroopCountForBossFightInHideoutFactor, value => settings.NumberOfMaximumTroopCountForBossFightInHideoutFactor = value);
      Apply(defaults.SpawnPercentageForFirstFightInHideoutMission, value => settings.SpawnPercentageForFirstFightInHideoutMission = value);
      Apply(defaults.NumberOfMinimumBanditTroopsInHideoutMission, value => settings.NumberOfMinimumBanditTroopsInHideoutMission = value);
      Apply(defaults.PlayerMaximumTroopCountForHideoutMission, value => settings.PlayerMaximumTroopCountForHideoutMission = value);

      Apply(defaults.EnableLogging, value => settings.EnableLogging = value);
    }

    private static void Apply(float? value, Action<float> setter)
    {
      if (value.HasValue)
        setter(value.Value);
    }

    private static void Apply(int? value, Action<int> setter)
    {
      if (value.HasValue)
        setter(value.Value);
    }

    private static void Apply(bool? value, Action<bool> setter)
    {
      if (value.HasValue)
        setter(value.Value);
    }
  }
}