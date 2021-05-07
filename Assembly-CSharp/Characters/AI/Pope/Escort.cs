using System;
using System.Collections;
using Level.Traps;
using UnityEngine;

namespace Characters.AI.Pope
{
	public class Escort : Trap
	{
		[SerializeField]
		private float _radius = 10f;

		[SerializeField]
		private float _sizeChangeDuration = 15f;

		[SerializeField]
		private EscortOrb[] _orbs;

		private float _elapsed;

		private bool _converged = true;

		private void Start()
		{
			float num = 0f;
			EscortOrb[] orbs = _orbs;
			for (int i = 0; i < orbs.Length; i++)
			{
				orbs[i].Initialize(num);
				num += (float)Math.PI * 2f / (float)_orbs.Length;
			}
		}

		public void Show()
		{
			base.gameObject.SetActive(true);
		}

		private void Update()
		{
			EscortOrb[] orbs = _orbs;
			for (int i = 0; i < orbs.Length; i++)
			{
				orbs[i].Move(_radius);
			}
		}

		public void ChangeSize()
		{
			if (_converged)
			{
				StartCoroutine(CChangeSizeUp());
			}
			else
			{
				StartCoroutine(CChangeSizeDown());
			}
		}

		private IEnumerator CChangeSize()
		{
			while (true)
			{
				_elapsed = 0f;
				while (_elapsed < _sizeChangeDuration)
				{
					_elapsed += Chronometer.global.deltaTime;
					yield return null;
				}
				yield return CChangeSizeUp();
				_elapsed = 0f;
				while (_elapsed < _sizeChangeDuration)
				{
					_elapsed += Chronometer.global.deltaTime;
					yield return null;
				}
				yield return CChangeSizeDown();
			}
		}

		private IEnumerator CChangeSizeUp()
		{
			float elapsed = 0f;
			float duration = 1f;
			float start = _radius;
			float end = _radius * 3f;
			while (elapsed < duration)
			{
				elapsed += Chronometer.global.deltaTime;
				_radius = Mathf.Lerp(start, end, elapsed / duration);
				yield return null;
			}
			_converged = false;
		}

		private IEnumerator CChangeSizeDown()
		{
			float elapsed = 0f;
			float duration = 1f;
			float start = _radius;
			float end = _radius / 3f;
			while (elapsed < duration)
			{
				elapsed += Chronometer.global.deltaTime;
				_radius = Mathf.Lerp(start, end, elapsed / duration);
				yield return null;
			}
			_converged = true;
		}
	}
}
