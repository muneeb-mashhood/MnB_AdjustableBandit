using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace AdjustableBandits
{
	internal static class HarmonyPatches
	{
		public static void Initialize()
		{
			var harmony = new Harmony("sy.adjustablebandits");
			var fillPartyStacks = FindFillPartyStacksMethod();
			var partySizeLimit = AccessTools.Method(typeof(DefaultPartySizeLimitModel), "CalculateMobilePartyMemberSizeLimit");
			var createBanditParty = AccessTools.Method(typeof(BanditPartyComponent), "CreateBanditParty");
			var createLooterParty = AccessTools.Method(typeof(BanditPartyComponent), "CreateLooterParty");
			var convertToBanditParty = AccessTools.Method(typeof(BanditPartyComponent), "ConvertPartyToBanditParty");
			var convertToLooterParty = AccessTools.Method(typeof(BanditPartyComponent), "ConvertPartyToLooterParty");

			TryPatch(harmony, fillPartyStacks, nameof(MobileParty_FillPartyStacks_Postfix));
			TryPatch(harmony, partySizeLimit, nameof(DefaultPartySizeLimitModel_CalculateMobilePartyMemberSizeLimit_Postfix));
			TryPatch(harmony, createBanditParty, nameof(BanditPartyComponent_CreateBanditParty_Postfix));
			TryPatch(harmony, createLooterParty, nameof(BanditPartyComponent_CreateLooterParty_Postfix));
			TryPatch(harmony, convertToBanditParty, nameof(BanditPartyComponent_ConvertPartyToBanditParty_Postfix));
			TryPatch(harmony, convertToLooterParty, nameof(BanditPartyComponent_ConvertPartyToLooterParty_Postfix));
		}

		private static void TryPatch(Harmony harmony, MethodInfo target, string postfixName)
		{
			if (target == null)
			{
				AdjustableBandits.LogError($"Adjustable Bandits: missing method for patch '{postfixName}'.");
				return;
			}

			try
			{
				harmony.Patch(target,
					postfix: new HarmonyMethod(typeof(HarmonyPatches), postfixName));
			}
			catch (Exception exc)
			{
				AdjustableBandits.LogError($"Adjustable Bandits: patch failed '{postfixName}': {exc}");
			}
		}

		private static MethodInfo FindFillPartyStacksMethod()
		{
			var names = new[]
			{
				"FillPartyStacks",
				"FillPartyStack",
				"FillPartyRoster",
				"FillParty"
			};

			var type = typeof(MobileParty);
			foreach (var name in names)
			{
				var method = AccessTools.Method(type, name);
				if (method != null)
					return method;
			}

			var baseType = type.BaseType;
			while (baseType != null)
			{
				foreach (var name in names)
				{
					var method = AccessTools.Method(baseType, name);
					if (method != null)
						return method;
				}
				baseType = baseType.BaseType;
			}

			return null;
		}

		private static void MobileParty_FillPartyStacks_Postfix(MobileParty __instance)
		{
			AdjustableBandits.ApplyPartySizeMultiplierOnce(__instance);
		}

		private static void DefaultPartySizeLimitModel_CalculateMobilePartyMemberSizeLimit_Postfix(ref ExplainedNumber __result, MobileParty party)
		{
			if (party.IsBandit)
			{
				const int baseLimit = 20;
				var sizeLimit = AdjustableBandits.GetSettings().BanditPartySizeLimit;
				if (sizeLimit > baseLimit)
					__result.Add(sizeLimit - baseLimit, new TextObject("{=adjban_text_BanditBonus}Bandit Bonus"));
			}
		}

		private static void BanditPartyComponent_CreateBanditParty_Postfix(MobileParty __result)
		{
			AdjustableBandits.ApplyPartySizeMultiplierOnce(__result);
			AdjustableBandits.EnforcePartyCountLimit(__result);
		}

		private static void BanditPartyComponent_CreateLooterParty_Postfix(MobileParty __result)
		{
			AdjustableBandits.ApplyPartySizeMultiplierOnce(__result);
			AdjustableBandits.EnforcePartyCountLimit(__result);
		}

		private static void BanditPartyComponent_ConvertPartyToBanditParty_Postfix(MobileParty mobileParty)
		{
			AdjustableBandits.ApplyPartySizeMultiplierOnce(mobileParty);
			AdjustableBandits.EnforcePartyCountLimit(mobileParty);
		}

		private static void BanditPartyComponent_ConvertPartyToLooterParty_Postfix(MobileParty mobileParty)
		{
			AdjustableBandits.ApplyPartySizeMultiplierOnce(mobileParty);
			AdjustableBandits.EnforcePartyCountLimit(mobileParty);
		}
	}
}
