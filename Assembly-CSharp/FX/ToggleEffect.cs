using Characters;
using UnityEngine;

namespace FX
{
	public class ToggleEffect : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Animator _animator;

		private Chronometer _chronometer;

		private void Start()
		{
			Character componentInParent = GetComponentInParent<Character>();
			if (componentInParent != null)
			{
				_chronometer = componentInParent.chronometer.animation;
			}
		}

		private void Update()
		{
			_animator.speed = ((_chronometer == null) ? Chronometer.global.timeScale : _chronometer.timeScale) / Time.timeScale;
		}
	}
}
