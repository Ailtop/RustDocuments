using System;
using System.Collections.Generic;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class StandardEventPayload
	{
		[SerializeField]
		private AnalyticsEventParamListContainer m_Parameters;

		private static Dictionary<string, object> m_EventData = new Dictionary<string, object>();

		[SerializeField]
		private string m_Name = "";

		public AnalyticsEventParamListContainer parameters => m_Parameters;

		public string name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;
			}
		}

		public StandardEventPayload()
		{
			m_Parameters = new AnalyticsEventParamListContainer();
		}

		public virtual AnalyticsResult Send()
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new Exception("Analtyics Event Tracker failed to send the CustomEvent. The event Name field cannot be empty.");
			}
			if (!IsCustomDataValid())
			{
				throw new Exception("Analytics event tracker failed to send. The event data is not valid. Parameter names cannot be null or empty.");
			}
			if (!IsRequiredDataValid())
			{
				throw new Exception("Analytics event tracker failed to send. The event data is not valid. Please check the values of required parameters.");
			}
			return AnalyticsEvent.Custom(name.Trim(), GetParameters());
		}

		private IDictionary<string, object> GetParameters()
		{
			m_EventData.Clear();
			List<AnalyticsEventParam> list = parameters.parameters;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && list[i].valueProperty.IsValid())
				{
					m_EventData.Add(list[i].name, list[i].value);
				}
			}
			return m_EventData;
		}

		private bool IsCustomDataValid()
		{
			List<AnalyticsEventParam> list = parameters.parameters;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && string.IsNullOrEmpty(list[i].name) && list[i].valueProperty.IsValid())
				{
					return false;
				}
			}
			return true;
		}

		private bool IsRequiredDataValid()
		{
			List<AnalyticsEventParam> list = parameters.parameters;
			Dictionary<string, List<bool>> dictionary = new Dictionary<string, List<bool>>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == null || list[i].requirementType != AnalyticsEventParam.RequirementType.Required)
				{
					continue;
				}
				if (string.IsNullOrEmpty(list[i].groupID))
				{
					if (!dictionary.ContainsKey("none"))
					{
						dictionary.Add("none", new List<bool>());
					}
					dictionary["none"].Add(list[i].valueProperty.IsValid());
				}
				else
				{
					if (!dictionary.ContainsKey(list[i].groupID))
					{
						dictionary.Add(list[i].groupID, new List<bool>());
					}
					dictionary[list[i].groupID].Add(list[i].valueProperty.IsValid());
				}
			}
			foreach (string key in dictionary.Keys)
			{
				if (key == "none")
				{
					if (dictionary[key].Contains(false))
					{
						return false;
					}
				}
				else if (!dictionary[key].Contains(true))
				{
					return false;
				}
			}
			return true;
		}
	}
}
