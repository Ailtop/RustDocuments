using UnityEngine;
using UnityEngine.UI;

namespace TwoDLaserPack
{
	public class RotationScript : MonoBehaviour
	{
		public Slider hSlider;

		public Transform pivot;

		public bool rotationEnabled;

		public float rotationAmount;

		private Transform transformCached;

		private void Start()
		{
			transformCached = base.transform;
		}

		private void Update()
		{
			if (rotationEnabled)
			{
				transformCached.RotateAround(pivot.localPosition, Vector3.forward, rotationAmount);
			}
		}

		public void OnHSliderChanged()
		{
			rotationAmount = hSlider.value;
		}
	}
}
