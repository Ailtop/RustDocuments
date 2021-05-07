using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Chimera
{
	public class ChimeraCombat : MonoBehaviour
	{
		[SerializeField]
		private Chimera _chimera;

		[SerializeField]
		[Subcomponent(typeof(WeightedPattern))]
		private WeightedPattern _normalPatterns;

		[SerializeField]
		[Range(0f, 100f)]
		private float _speedUpHealthCondition = 0.6f;

		[SerializeField]
		private float _animationHighSpeed = 1.5f;

		private const float _stompHighSpeed = 1.2f;

		private bool _speedUpState;

		public IEnumerator Combat()
		{
			if (!_speedUpState && _chimera.character.health.percent < (double)_speedUpHealthCondition)
			{
				_chimera.SetAnimationSpeed(_animationHighSpeed);
				_speedUpState = true;
			}
			if (_chimera.CanUseWreckDrop())
			{
				yield return _chimera.RunPattern(Pattern.WreckDrop);
				yield return _chimera.RunPattern(Pattern.VenomBreath);
				yield return _chimera.RunPattern(Pattern.WreckDestroy);
				yield break;
			}
			if (_chimera.CanUseSubjectDrop())
			{
				yield return _chimera.RunPattern(Pattern.SubjectDrop);
				yield return _chimera.RunPattern(Pattern.SkippableIdle);
				yield break;
			}
			if (_chimera.CanUseStomp() && MMMaths.RandomBool())
			{
				if (_speedUpState)
				{
					_chimera.SetAnimationSpeed(1.2f);
				}
				int count = Random.Range(1, 4);
				for (int i = 0; i < count; i++)
				{
					yield return _chimera.RunPattern(Pattern.Stomp);
				}
				if (_speedUpState)
				{
					_chimera.SetAnimationSpeed(_animationHighSpeed);
				}
				yield return _chimera.RunPattern(Pattern.SkippableIdle);
				yield break;
			}
			if (_chimera.CanUseBite() && MMMaths.Chance(0.1f))
			{
				yield return _chimera.RunPattern(Pattern.Bite);
				yield break;
			}
			Pattern pattern = _normalPatterns.TakeOne();
			if (pattern == Pattern.VenomFall && !_chimera.CanUseVenomFall())
			{
				do
				{
					pattern = _normalPatterns.TakeOne();
				}
				while (pattern == Pattern.VenomFall);
			}
			yield return _chimera.RunPattern(pattern);
			switch (pattern)
			{
			case Pattern.VenomFall:
				yield return _chimera.RunPattern(Pattern.Idle);
				if (_speedUpState)
				{
					yield return _chimera.RunPattern(Pattern.Idle);
				}
				break;
			default:
				yield return _chimera.RunPattern(Pattern.SkippableIdle);
				break;
			case Pattern.VenomBall:
				break;
			}
		}
	}
}
