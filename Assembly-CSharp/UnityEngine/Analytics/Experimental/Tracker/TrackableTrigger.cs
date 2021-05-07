using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class TrackableTrigger
	{
		[SerializeField]
		private GameObject m_Target;

		[SerializeField]
		private string m_MethodPath;
	}
}
