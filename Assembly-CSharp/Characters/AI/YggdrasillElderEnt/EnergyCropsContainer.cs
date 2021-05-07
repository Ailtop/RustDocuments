using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class EnergyCropsContainer : MonoBehaviour
	{
		[SerializeField]
		private Transform _leftBottom;

		[SerializeField]
		private GameObject _orb;

		[SerializeField]
		private int _width;

		[SerializeField]
		private int _height;

		[SerializeField]
		private float _distance;

		[SerializeField]
		private float _noise;

		private void OnEnable()
		{
			Generate();
		}

		private void Generate()
		{
		}
	}
}
