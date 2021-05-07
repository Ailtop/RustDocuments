using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public abstract class TrackableProperty
	{
		[SerializeField]
		protected Object m_Target;

		[SerializeField]
		protected string m_Path;
	}
}
