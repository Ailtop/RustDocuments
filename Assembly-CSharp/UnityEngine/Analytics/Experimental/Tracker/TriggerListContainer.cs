using System;
using System.Collections.Generic;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class TriggerListContainer
	{
		[SerializeField]
		private List<TriggerRule> m_Rules = new List<TriggerRule>();

		internal List<TriggerRule> rules
		{
			get
			{
				return m_Rules;
			}
			set
			{
				m_Rules = value;
			}
		}
	}
}
