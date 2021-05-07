using UnityEngine;

namespace Characters.AI.Hero
{
	public class PillarGenerator : MonoBehaviour
	{
		[SerializeField]
		private PillarOfLightContainer _containerPrefab;

		[SerializeField]
		private string _pillarPrefabPath;

		[SerializeField]
		private PillarOfLight _pillarPrefab;

		[SerializeField]
		private float _distance = 2.1f;

		[SerializeField]
		[MinMaxSlider(0f, 90f)]
		private Vector2 _angleRange;

		[SerializeField]
		private int _count = 9;
	}
}
