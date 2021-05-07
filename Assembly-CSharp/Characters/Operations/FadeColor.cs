using System.Collections;
using UnityEngine;

namespace Characters.Operations
{
	public class FadeColor : CharacterOperation
	{
		[SerializeField]
		private SpriteRenderer _sprite;

		[SerializeField]
		private Color _target;

		[SerializeField]
		private float _duration;

		public override void Run(Character owner)
		{
			StartCoroutine(CFade(owner));
		}

		private IEnumerator CFade(Character owner)
		{
			Color startColor = _sprite.color;
			Color different = _target - _sprite.color;
			float time = 0f;
			while (time < _duration)
			{
				time += owner.chronometer.master.deltaTime;
				_sprite.color = startColor + different * (time / _duration);
				yield return null;
			}
			_sprite.color = _target;
		}
	}
}
