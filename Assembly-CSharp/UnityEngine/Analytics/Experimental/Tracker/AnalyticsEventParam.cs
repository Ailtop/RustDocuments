using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class AnalyticsEventParam
	{
		public enum RequirementType
		{
			None,
			Required,
			Optional
		}

		[SerializeField]
		private RequirementType m_RequirementType;

		[SerializeField]
		private string m_GroupID;

		[SerializeField]
		private string m_Name;

		[SerializeField]
		private ValueProperty m_Value;

		public RequirementType requirementType => m_RequirementType;

		public string groupID => m_GroupID;

		public ValueProperty valueProperty => m_Value;

		public string name => m_Name.Trim();

		public object value => m_Value.propertyValue;

		public AnalyticsEventParam(string name = null, params Type[] validTypes)
		{
			m_Name = name;
			long num = validTypes.LongLength;
		}
	}
}
