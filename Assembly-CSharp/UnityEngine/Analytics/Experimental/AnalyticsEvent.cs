using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEngine.Analytics.Experimental
{
	public static class AnalyticsEvent
	{
		private static readonly string k_SdkVersion = "0.4.0";

		private static readonly string k_ErrorFormat_RequiredParamNotSet = "Required param not set ({0}).";

		private static readonly Dictionary<string, object> m_EventData = new Dictionary<string, object>();

		public static string sdkVersion => k_SdkVersion;

		public static bool debugMode { get; set; }

		private static void OnValidationFailed(string message)
		{
			throw new ArgumentException(message);
		}

		private static void AddCustomEventData(IDictionary<string, object> eventData)
		{
			if (eventData == null)
			{
				return;
			}
			for (int i = 0; i < eventData.Count; i++)
			{
				KeyValuePair<string, object> keyValuePair = eventData.ElementAt(i);
				if (!m_EventData.ContainsKey(keyValuePair.Key))
				{
					m_EventData.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}

		private static string SplitCamelCase(string input)
		{
			input = Regex.Replace(input, "([a-z](?=[A-Z]))", "$0_");
			return Regex.Replace(input, "(?<!_|^)[A-Z][a-z]", "_$0");
		}

		public static string EnumToString(object enumValue)
		{
			string text = enumValue.ToString();
			if (enumValue is AdvertisingNetwork || enumValue is AuthorizationNetwork || enumValue is SocialNetwork)
			{
				return text.ToLower();
			}
			if (enumValue is AcquisitionSource || enumValue is AcquisitionType || enumValue is ScreenName || enumValue is ShareType || enumValue is StoreType)
			{
				return SplitCamelCase(text).ToLower();
			}
			return text;
		}

		public static AnalyticsResult Custom(string eventName, IDictionary<string, object> eventData = null)
		{
			AnalyticsResult analyticsResult = AnalyticsResult.UnsupportedPlatform;
			string empty = string.Empty;
			if (string.IsNullOrEmpty(eventName))
			{
				OnValidationFailed("Custom event name cannot be set to null or an empty string.");
			}
			switch (analyticsResult)
			{
			case AnalyticsResult.Ok:
				if (debugMode)
				{
					Debug.LogFormat("Successfully sent '{0}' event (Result: '{1}').{2}", eventName, analyticsResult, empty);
				}
				break;
			case AnalyticsResult.TooManyItems:
			case AnalyticsResult.InvalidData:
				Debug.LogErrorFormat("Failed to send '{0}' event (Result: '{1}').{2}", eventName, analyticsResult, empty);
				break;
			default:
				Debug.LogWarningFormat("Unable to send '{0}' event (Result: '{1}').{2}", eventName, analyticsResult, empty);
				break;
			}
			return analyticsResult;
		}

		public static AnalyticsResult AchievementStep(int stepIndex, string achievementId, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("step_index", stepIndex);
			if (string.IsNullOrEmpty(achievementId))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "achievement_id"));
			}
			else
			{
				m_EventData.Add("achievement_id", achievementId);
			}
			AddCustomEventData(eventData);
			return Custom("achievement_step", m_EventData);
		}

		public static AnalyticsResult AchievementUnlocked(string achievementId, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(achievementId))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "achievement_id"));
			}
			else
			{
				m_EventData.Add("achievement_id", achievementId);
			}
			AddCustomEventData(eventData);
			return Custom("achievement_unlocked", m_EventData);
		}

		public static AnalyticsResult AdComplete(bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("rewarded", rewarded);
			if (!string.IsNullOrEmpty(advertisingNetwork))
			{
				m_EventData.Add("network", advertisingNetwork);
			}
			if (!string.IsNullOrEmpty(placementId))
			{
				m_EventData.Add("placement_id", placementId);
			}
			AddCustomEventData(eventData);
			return Custom("ad_complete", m_EventData);
		}

		public static AnalyticsResult AdComplete(bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
		{
			return AdComplete(rewarded, EnumToString(advertisingNetwork), placementId, eventData);
		}

		public static AnalyticsResult AdOffer(bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("rewarded", rewarded);
			if (!string.IsNullOrEmpty(advertisingNetwork))
			{
				m_EventData.Add("network", advertisingNetwork);
			}
			if (!string.IsNullOrEmpty(placementId))
			{
				m_EventData.Add("placement_id", placementId);
			}
			AddCustomEventData(eventData);
			return Custom("ad_offer", m_EventData);
		}

		public static AnalyticsResult AdOffer(bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
		{
			return AdOffer(rewarded, EnumToString(advertisingNetwork), placementId, eventData);
		}

		public static AnalyticsResult AdSkip(bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("rewarded", rewarded);
			if (!string.IsNullOrEmpty(advertisingNetwork))
			{
				m_EventData.Add("network", advertisingNetwork);
			}
			if (!string.IsNullOrEmpty(placementId))
			{
				m_EventData.Add("placement_id", placementId);
			}
			AddCustomEventData(eventData);
			return Custom("ad_skip", m_EventData);
		}

		public static AnalyticsResult AdSkip(bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
		{
			return AdSkip(rewarded, EnumToString(advertisingNetwork), placementId, eventData);
		}

		public static AnalyticsResult AdStart(bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("rewarded", rewarded);
			if (!string.IsNullOrEmpty(advertisingNetwork))
			{
				m_EventData.Add("network", advertisingNetwork);
			}
			if (!string.IsNullOrEmpty(placementId))
			{
				m_EventData.Add("placement_id", placementId);
			}
			AddCustomEventData(eventData);
			return Custom("ad_start", m_EventData);
		}

		public static AnalyticsResult AdStart(bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
		{
			return AdStart(rewarded, EnumToString(advertisingNetwork), placementId, eventData);
		}

		public static AnalyticsResult ChatMessageSent(IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			AddCustomEventData(eventData);
			return Custom("chat_msg_sent", m_EventData);
		}

		public static AnalyticsResult CutsceneSkip(string cutsceneName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(cutsceneName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "scene_name"));
			}
			else
			{
				m_EventData.Add("scene_name", cutsceneName);
			}
			AddCustomEventData(eventData);
			return Custom("cutscene_skip", m_EventData);
		}

		public static AnalyticsResult CutsceneStart(string cutsceneName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(cutsceneName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "scene_name"));
			}
			else
			{
				m_EventData.Add("scene_name", cutsceneName);
			}
			AddCustomEventData(eventData);
			return Custom("cutscene_start", m_EventData);
		}

		public static AnalyticsResult FirstInteraction(string actionId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (!string.IsNullOrEmpty(actionId))
			{
				m_EventData.Add("action_id", actionId);
			}
			AddCustomEventData(eventData);
			return Custom("first_interaction", m_EventData);
		}

		public static AnalyticsResult GameOver(string levelName = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (!string.IsNullOrEmpty(levelName))
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("game_over", m_EventData);
		}

		public static AnalyticsResult GameOver(int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("level_index", levelIndex);
			if (!string.IsNullOrEmpty(levelName))
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("game_over", m_EventData);
		}

		public static AnalyticsResult GameStart(IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			AddCustomEventData(eventData);
			return Custom("game_start", m_EventData);
		}

		public static AnalyticsResult IAPTransaction(string transactionContext, float price, string itemId, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(transactionContext))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
			}
			else
			{
				m_EventData.Add("transaction_context", transactionContext);
			}
			if (string.IsNullOrEmpty(itemId))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
			}
			else
			{
				m_EventData.Add("item_id", itemId);
			}
			if (!string.IsNullOrEmpty(itemType))
			{
				m_EventData.Add("item_type", itemType);
			}
			if (!string.IsNullOrEmpty(level))
			{
				m_EventData.Add("level", level);
			}
			if (!string.IsNullOrEmpty(transactionId))
			{
				m_EventData.Add("transaction_id", transactionId);
			}
			m_EventData.Add("price", price);
			AddCustomEventData(eventData);
			return Custom("iap_transaction", m_EventData);
		}

		public static AnalyticsResult ItemAcquired(AcquisitionType currencyType, string transactionContext, float amount, string itemId, float balance, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(transactionContext))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
			}
			else
			{
				m_EventData.Add("transaction_context", transactionContext);
			}
			if (string.IsNullOrEmpty(itemId))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
			}
			else
			{
				m_EventData.Add("item_id", itemId);
			}
			if (!string.IsNullOrEmpty(itemType))
			{
				m_EventData.Add("item_type", itemType);
			}
			if (!string.IsNullOrEmpty(level))
			{
				m_EventData.Add("level", level);
			}
			if (!string.IsNullOrEmpty(transactionId))
			{
				m_EventData.Add("transaction_id", transactionId);
			}
			m_EventData.Add("currency_type", EnumToString(currencyType));
			m_EventData.Add("amount", amount);
			m_EventData.Add("balance", balance);
			AddCustomEventData(eventData);
			return Custom("item_acquired", m_EventData);
		}

		public static AnalyticsResult ItemAcquired(AcquisitionType currencyType, string transactionContext, float amount, string itemId, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(transactionContext))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
			}
			else
			{
				m_EventData.Add("transaction_context", transactionContext);
			}
			if (string.IsNullOrEmpty(itemId))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
			}
			else
			{
				m_EventData.Add("item_id", itemId);
			}
			if (!string.IsNullOrEmpty(itemType))
			{
				m_EventData.Add("item_type", itemType);
			}
			if (!string.IsNullOrEmpty(level))
			{
				m_EventData.Add("level", level);
			}
			if (!string.IsNullOrEmpty(transactionId))
			{
				m_EventData.Add("transaction_id", transactionId);
			}
			m_EventData.Add("currency_type", EnumToString(currencyType));
			m_EventData.Add("amount", amount);
			AddCustomEventData(eventData);
			return Custom("item_acquired", m_EventData);
		}

		public static AnalyticsResult ItemSpent(AcquisitionType currencyType, string transactionContext, float amount, string itemId, float balance, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(transactionContext))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
			}
			else
			{
				m_EventData.Add("transaction_context", transactionContext);
			}
			if (string.IsNullOrEmpty(itemId))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
			}
			else
			{
				m_EventData.Add("item_id", itemId);
			}
			if (!string.IsNullOrEmpty(itemType))
			{
				m_EventData.Add("item_type", itemType);
			}
			if (!string.IsNullOrEmpty(level))
			{
				m_EventData.Add("level", level);
			}
			if (!string.IsNullOrEmpty(transactionId))
			{
				m_EventData.Add("transaction_id", transactionId);
			}
			m_EventData.Add("currency_type", EnumToString(currencyType));
			m_EventData.Add("amount", amount);
			m_EventData.Add("balance", balance);
			AddCustomEventData(eventData);
			return Custom("item_spent", m_EventData);
		}

		public static AnalyticsResult ItemSpent(AcquisitionType currencyType, string transactionContext, float amount, string itemId, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(transactionContext))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
			}
			else
			{
				m_EventData.Add("transaction_context", transactionContext);
			}
			if (string.IsNullOrEmpty(itemId))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
			}
			else
			{
				m_EventData.Add("item_id", itemId);
			}
			if (!string.IsNullOrEmpty(itemType))
			{
				m_EventData.Add("item_type", itemType);
			}
			if (!string.IsNullOrEmpty(level))
			{
				m_EventData.Add("level", level);
			}
			if (!string.IsNullOrEmpty(transactionId))
			{
				m_EventData.Add("transaction_id", transactionId);
			}
			m_EventData.Add("currency_type", EnumToString(currencyType));
			m_EventData.Add("amount", amount);
			AddCustomEventData(eventData);
			return Custom("item_spent", m_EventData);
		}

		public static AnalyticsResult LevelComplete(string levelName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(levelName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
			}
			else
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_complete", m_EventData);
		}

		public static AnalyticsResult LevelComplete(int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("level_index", levelIndex);
			if (!string.IsNullOrEmpty(levelName))
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_complete", m_EventData);
		}

		public static AnalyticsResult LevelFail(string levelName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(levelName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
			}
			else
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_fail", m_EventData);
		}

		public static AnalyticsResult LevelFail(int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("level_index", levelIndex);
			if (!string.IsNullOrEmpty(levelName))
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_fail", m_EventData);
		}

		public static AnalyticsResult LevelQuit(string levelName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(levelName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
			}
			else
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_quit", m_EventData);
		}

		public static AnalyticsResult LevelQuit(int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("level_index", levelIndex);
			if (!string.IsNullOrEmpty(levelName))
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_quit", m_EventData);
		}

		public static AnalyticsResult LevelSkip(string levelName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(levelName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
			}
			else
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_skip", m_EventData);
		}

		public static AnalyticsResult LevelSkip(int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("level_index", levelIndex);
			if (!string.IsNullOrEmpty(levelName))
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_skip", m_EventData);
		}

		public static AnalyticsResult LevelStart(string levelName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(levelName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
			}
			else
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_start", m_EventData);
		}

		public static AnalyticsResult LevelStart(int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("level_index", levelIndex);
			if (!string.IsNullOrEmpty(levelName))
			{
				m_EventData.Add("level_name", levelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_start", m_EventData);
		}

		public static AnalyticsResult LevelUp(string newLevelName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(newLevelName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "new_level_name"));
			}
			else
			{
				m_EventData.Add("new_level_name", newLevelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_up", m_EventData);
		}

		public static AnalyticsResult LevelUp(int newLevelIndex, string newLevelName = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("new_level_index", newLevelIndex);
			if (!string.IsNullOrEmpty(newLevelName))
			{
				m_EventData.Add("new_level_name", newLevelName);
			}
			AddCustomEventData(eventData);
			return Custom("level_up", m_EventData);
		}

		public static AnalyticsResult PostAdAction(bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("rewarded", rewarded);
			if (!string.IsNullOrEmpty(advertisingNetwork))
			{
				m_EventData.Add("network", advertisingNetwork);
			}
			if (!string.IsNullOrEmpty(placementId))
			{
				m_EventData.Add("placement_id", placementId);
			}
			AddCustomEventData(eventData);
			return Custom("post_ad_action", m_EventData);
		}

		public static AnalyticsResult PostAdAction(bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("rewarded", rewarded);
			m_EventData.Add("network", EnumToString(advertisingNetwork));
			if (!string.IsNullOrEmpty(placementId))
			{
				m_EventData.Add("placement_id", placementId);
			}
			AddCustomEventData(eventData);
			return Custom("post_ad_action", m_EventData);
		}

		public static AnalyticsResult PushNotificationClick(string messageId, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(messageId))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "message_id"));
			}
			else
			{
				m_EventData.Add("message_id", messageId);
			}
			AddCustomEventData(eventData);
			return Custom("push_notification_click", m_EventData);
		}

		public static AnalyticsResult PushNotificationEnable(IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			AddCustomEventData(eventData);
			return Custom("push_notification_enable", m_EventData);
		}

		public static AnalyticsResult ScreenVisit(ScreenName screenName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("screen_name", EnumToString(screenName));
			AddCustomEventData(eventData);
			return Custom("screen_visit", m_EventData);
		}

		public static AnalyticsResult ScreenVisit(string screenName, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(screenName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "screen_name"));
			}
			else
			{
				m_EventData.Add("screen_name", screenName);
			}
			AddCustomEventData(eventData);
			return Custom("screen_visit", m_EventData);
		}

		public static AnalyticsResult SocialShare(ShareType shareType, SocialNetwork socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("share_type", EnumToString(shareType));
			m_EventData.Add("social_network", EnumToString(socialNetwork));
			if (!string.IsNullOrEmpty(senderId))
			{
				m_EventData.Add("sender_id", senderId);
			}
			if (!string.IsNullOrEmpty(recipientId))
			{
				m_EventData.Add("recipient_id", recipientId);
			}
			AddCustomEventData(eventData);
			return Custom("social_share", m_EventData);
		}

		public static AnalyticsResult SocialShare(ShareType shareType, string socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("share_type", EnumToString(shareType));
			if (string.IsNullOrEmpty(socialNetwork))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "social_network"));
			}
			else
			{
				m_EventData.Add("social_network", socialNetwork);
			}
			if (!string.IsNullOrEmpty(senderId))
			{
				m_EventData.Add("sender_id", senderId);
			}
			if (!string.IsNullOrEmpty(recipientId))
			{
				m_EventData.Add("recipient_id", recipientId);
			}
			AddCustomEventData(eventData);
			return Custom("social_share", m_EventData);
		}

		public static AnalyticsResult SocialShare(string shareType, SocialNetwork socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(shareType))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "share_type"));
			}
			else
			{
				m_EventData.Add("share_type", shareType);
			}
			m_EventData.Add("social_network", EnumToString(socialNetwork));
			if (!string.IsNullOrEmpty(senderId))
			{
				m_EventData.Add("sender_id", senderId);
			}
			if (!string.IsNullOrEmpty(recipientId))
			{
				m_EventData.Add("recipient_id", recipientId);
			}
			AddCustomEventData(eventData);
			return Custom("social_share", m_EventData);
		}

		public static AnalyticsResult SocialShare(string shareType, string socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(shareType))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "share_type"));
			}
			else
			{
				m_EventData.Add("share_type", shareType);
			}
			if (string.IsNullOrEmpty(socialNetwork))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "social_network"));
			}
			else
			{
				m_EventData.Add("social_network", socialNetwork);
			}
			if (!string.IsNullOrEmpty(senderId))
			{
				m_EventData.Add("sender_id", senderId);
			}
			if (!string.IsNullOrEmpty(recipientId))
			{
				m_EventData.Add("recipient_id", recipientId);
			}
			AddCustomEventData(eventData);
			return Custom("social_share", m_EventData);
		}

		public static AnalyticsResult SocialShareAccept(ShareType shareType, SocialNetwork socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("share_type", EnumToString(shareType));
			m_EventData.Add("social_network", EnumToString(socialNetwork));
			if (!string.IsNullOrEmpty(senderId))
			{
				m_EventData.Add("sender_id", senderId);
			}
			if (!string.IsNullOrEmpty(recipientId))
			{
				m_EventData.Add("recipient_id", recipientId);
			}
			AddCustomEventData(eventData);
			return Custom("social_share_accept", m_EventData);
		}

		public static AnalyticsResult SocialShareAccept(ShareType shareType, string socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("share_type", EnumToString(shareType));
			if (string.IsNullOrEmpty(socialNetwork))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "social_network"));
			}
			else
			{
				m_EventData.Add("social_network", socialNetwork);
			}
			if (!string.IsNullOrEmpty(senderId))
			{
				m_EventData.Add("sender_id", senderId);
			}
			if (!string.IsNullOrEmpty(recipientId))
			{
				m_EventData.Add("recipient_id", recipientId);
			}
			AddCustomEventData(eventData);
			return Custom("social_share_accept", m_EventData);
		}

		public static AnalyticsResult SocialShareAccept(string shareType, SocialNetwork socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(shareType))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "share_type"));
			}
			else
			{
				m_EventData.Add("share_type", shareType);
			}
			m_EventData.Add("social_network", EnumToString(socialNetwork));
			if (!string.IsNullOrEmpty(senderId))
			{
				m_EventData.Add("sender_id", senderId);
			}
			if (!string.IsNullOrEmpty(recipientId))
			{
				m_EventData.Add("recipient_id", recipientId);
			}
			AddCustomEventData(eventData);
			return Custom("social_share_accept", m_EventData);
		}

		public static AnalyticsResult SocialShareAccept(string shareType, string socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(shareType))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "share_type"));
			}
			else
			{
				m_EventData.Add("share_type", shareType);
			}
			if (string.IsNullOrEmpty(socialNetwork))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "social_network"));
			}
			else
			{
				m_EventData.Add("social_network", socialNetwork);
			}
			if (!string.IsNullOrEmpty(senderId))
			{
				m_EventData.Add("sender_id", senderId);
			}
			if (!string.IsNullOrEmpty(recipientId))
			{
				m_EventData.Add("recipient_id", recipientId);
			}
			AddCustomEventData(eventData);
			return Custom("social_share_accept", m_EventData);
		}

		public static AnalyticsResult StoreItemClick(StoreType storeType, string itemId, string itemName = null, Dictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("type", EnumToString(storeType));
			if (string.IsNullOrEmpty(itemId) && string.IsNullOrEmpty(itemName))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id or item_name"));
			}
			else
			{
				if (!string.IsNullOrEmpty(itemId))
				{
					m_EventData.Add("item_id", itemId);
				}
				if (!string.IsNullOrEmpty(itemName))
				{
					m_EventData.Add("item_name", itemName);
				}
			}
			AddCustomEventData(eventData);
			return Custom("store_item_click", m_EventData);
		}

		public static AnalyticsResult StoreOpened(StoreType storeType, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("type", EnumToString(storeType));
			AddCustomEventData(eventData);
			return Custom("store_opened", m_EventData);
		}

		public static AnalyticsResult TutorialComplete(string tutorialId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (!string.IsNullOrEmpty(tutorialId))
			{
				m_EventData.Add("tutorial_id", tutorialId);
			}
			AddCustomEventData(eventData);
			return Custom("tutorial_complete", m_EventData);
		}

		public static AnalyticsResult TutorialSkip(string tutorialId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (!string.IsNullOrEmpty(tutorialId))
			{
				m_EventData.Add("tutorial_id", tutorialId);
			}
			AddCustomEventData(eventData);
			return Custom("tutorial_skip", m_EventData);
		}

		public static AnalyticsResult TutorialStart(string tutorialId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (!string.IsNullOrEmpty(tutorialId))
			{
				m_EventData.Add("tutorial_id", tutorialId);
			}
			AddCustomEventData(eventData);
			return Custom("tutorial_start", m_EventData);
		}

		public static AnalyticsResult TutorialStep(int stepIndex, string tutorialId = null, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("step_index", stepIndex);
			if (!string.IsNullOrEmpty(tutorialId))
			{
				m_EventData.Add("tutorial_id", tutorialId);
			}
			AddCustomEventData(eventData);
			return Custom("tutorial_step", m_EventData);
		}

		public static AnalyticsResult UserSignup(string authorizationNetwork, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			if (string.IsNullOrEmpty(authorizationNetwork))
			{
				OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "authorization_network"));
			}
			else
			{
				m_EventData.Add("authorization_network", authorizationNetwork);
			}
			AddCustomEventData(eventData);
			return Custom("user_signup", m_EventData);
		}

		public static AnalyticsResult UserSignup(AuthorizationNetwork authorizationNetwork, IDictionary<string, object> eventData = null)
		{
			m_EventData.Clear();
			m_EventData.Add("authorization_network", EnumToString(authorizationNetwork));
			AddCustomEventData(eventData);
			return Custom("user_signup", m_EventData);
		}
	}
}
