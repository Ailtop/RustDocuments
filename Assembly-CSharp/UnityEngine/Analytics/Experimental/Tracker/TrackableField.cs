using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class TrackableField : TrackableProperty
	{
		[SerializeField]
		private string[] m_ValidTypeNames;

		[SerializeField]
		private string m_Type;

		[SerializeField]
		private string m_EnumType;

		public TrackableField(params Type[] validTypes)
		{
			if (validTypes != null && validTypes.Length != 0)
			{
				m_ValidTypeNames = new string[validTypes.Length];
				for (int i = 0; i < validTypes.Length; i++)
				{
					m_ValidTypeNames[i] = validTypes[i].ToString();
				}
			}
		}

		public object GetValue()
		{
			if (m_Target == null || string.IsNullOrEmpty(m_Path))
			{
				return null;
			}
			object obj = m_Target;
			string[] array = m_Path.Split('.');
			foreach (string name in array)
			{
				try
				{
					obj = obj.GetType().GetProperty(name).GetValue(obj, null);
				}
				catch
				{
					obj = obj.GetType().GetField(name).GetValue(obj);
				}
			}
			return obj;
		}
	}
}
