using System;
using System.Collections.Generic;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class AnalyticsEventParamListContainer
	{
		[SerializeField]
		private List<AnalyticsEventParam> m_Parameters = new List<AnalyticsEventParam>();

		public List<AnalyticsEventParam> parameters
		{
			get
			{
				return m_Parameters;
			}
			set
			{
				m_Parameters = value;
			}
		}
	}
}
