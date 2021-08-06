using UnityEngine;

namespace Sonar
{
	public class SonarObject : MonoBehaviour, IClientComponent
	{
		public enum SType
		{
			MoonPool,
			Sub
		}

		[SerializeField]
		private SType sonarType;
	}
}
