using UnityEngine;

namespace Characters.AI.Hero
{
	public class DualFinish : MonoBehaviour
	{
		[Header("Position")]
		[SerializeField]
		private float _noise = 2f;

		[Header("Rotation")]
		[SerializeField]
		[MinMaxSlider(0f, 90f)]
		private Vector2 _angleRange;

		[SerializeField]
		private GameObject _clockWise;

		[SerializeField]
		private GameObject _counterClockWise;

		public void OnEnable()
		{
			SetPosition();
			SetRotation();
		}

		private void SetPosition()
		{
			Vector3 translation = Random.insideUnitSphere * _noise;
			if (MMMaths.RandomBool())
			{
				_clockWise.transform.Translate(translation);
				_counterClockWise.transform.localPosition = Vector2.zero;
			}
			else
			{
				_clockWise.transform.localPosition = Vector2.zero;
				_counterClockWise.transform.Translate(translation);
			}
		}

		private void SetRotation()
		{
			_clockWise.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(_angleRange.x, _angleRange.y));
			_counterClockWise.transform.rotation = Quaternion.Euler(0f, 0f, 180f - Random.Range(_angleRange.x, _angleRange.y));
		}
	}
}
