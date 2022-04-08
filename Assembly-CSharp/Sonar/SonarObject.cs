using UnityEngine;

namespace Sonar;

public class SonarObject : MonoBehaviour, IClientComponent
{
	public enum SType
	{
		MoonPool = 0,
		Sub = 1
	}

	[SerializeField]
	private SType sonarType;
}
