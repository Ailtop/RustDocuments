using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class ValueProperty
	{
		public enum PropertyType
		{
			Disabled,
			Static,
			Dynamic
		}

		[SerializeField]
		private PropertyType m_PropertyType = PropertyType.Static;

		[SerializeField]
		private string m_ValueType;

		[SerializeField]
		private string m_Value = "";

		[SerializeField]
		private TrackableField m_Target;

		public string valueType
		{
			get
			{
				return m_ValueType;
			}
			set
			{
				m_ValueType = value;
			}
		}

		public string propertyValue
		{
			get
			{
				if (m_PropertyType == PropertyType.Dynamic && m_Target != null)
				{
					return m_Target.GetValue()?.ToString().Trim();
				}
				if (m_Value != null)
				{
					return m_Value.Trim();
				}
				return null;
			}
		}

		public TrackableField target => m_Target;

		public bool IsValid()
		{
			switch (m_PropertyType)
			{
			case PropertyType.Static:
				if (string.IsNullOrEmpty(m_Value))
				{
					return Type.GetType(m_ValueType) != typeof(string);
				}
				return true;
			case PropertyType.Dynamic:
				if (m_Target != null)
				{
					return m_Target.GetValue() != null;
				}
				return false;
			default:
				return false;
			}
		}
	}
}
