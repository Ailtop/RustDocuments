using System;
using UnityEngine;

namespace Characters.AI.Chimera
{
	public class WeightedPattern : MonoBehaviour
	{
		private WeightedRandomizer<Pattern> _weightedRandomizer;

		[SerializeField]
		[Range(0f, 100f)]
		private float _idleWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _biteWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _slamWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _venomFallWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _venomBallWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _venomCannonWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _subjectDropWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _wreckDropWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _wreckDestroyWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _venomBreathWeight;

		private void Awake()
		{
			_weightedRandomizer = WeightedRandomizer.From<Pattern>(new ValueTuple<Pattern, float>(Pattern.Idle, _idleWeight), new ValueTuple<Pattern, float>(Pattern.Bite, _biteWeight), new ValueTuple<Pattern, float>(Pattern.Stomp, _slamWeight), new ValueTuple<Pattern, float>(Pattern.VenomFall, _venomFallWeight), new ValueTuple<Pattern, float>(Pattern.VenomBall, _venomBallWeight), new ValueTuple<Pattern, float>(Pattern.VenomCannon, _venomCannonWeight), new ValueTuple<Pattern, float>(Pattern.SubjectDrop, _subjectDropWeight), new ValueTuple<Pattern, float>(Pattern.WreckDrop, _wreckDropWeight), new ValueTuple<Pattern, float>(Pattern.WreckDestroy, _wreckDestroyWeight), new ValueTuple<Pattern, float>(Pattern.VenomBreath, _venomBreathWeight));
		}

		public Pattern TakeOne()
		{
			return _weightedRandomizer.TakeOne();
		}
	}
}
