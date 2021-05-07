using System.Collections;
using UnityEngine;

namespace UI
{
	public class CurrencyEffect : MonoBehaviour
	{
		[SerializeField]
		private Animator _animator;

		private float _animationLength;

		private float _remainTime;

		private void Awake()
		{
			_animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
		}

		public void Play()
		{
			_animator.enabled = true;
			_animator.Play(0, 0, 0f);
			_animator.enabled = false;
			_remainTime = _animationLength;
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
				CoroutineProxy.instance.StartCoroutine(CPlay());
			}
		}

		private IEnumerator CPlay()
		{
			while (_remainTime > 0f)
			{
				yield return null;
				float deltaTime = Chronometer.global.deltaTime;
				_animator.Update(deltaTime);
				_remainTime -= deltaTime;
			}
			base.gameObject.SetActive(false);
		}
	}
}
