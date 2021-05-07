using System;
using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Adventurer;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	public abstract class AdventurerController : AIController, IRunSequence
	{
		[SerializeField]
		protected AdventurerHealthBarAttacher _adventurerHealthBarAttacher;

		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		protected CheckWithinSight _checkWithinSight;

		[Header("Idle")]
		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.SkipableIdle))]
		protected Characters.AI.Behaviours.Adventurer.SkipableIdle _skipableIdle;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		protected Idle _idle;

		[SerializeField]
		[Subcomponent(typeof(SpawnLineText))]
		private SpawnLineText _spawnIdleLineText;

		[Header("Intro")]
		[Space]
		[SerializeField]
		protected Characters.Actions.Action _introMotion;

		[Header("Groggy")]
		[SerializeField]
		protected Characters.Actions.Action _groggy;

		[Header("Potion")]
		[Subcomponent(typeof(DrinkPotion))]
		[SerializeField]
		protected DrinkPotion _drinkPotion;

		[Header("Potion")]
		[Subcomponent(typeof(Runaway))]
		[SerializeField]
		protected Runaway _runaway;

		[Header("Commander")]
		[SerializeField]
		protected Commander _commander;

		protected SequenceSelector _sequenceSelector;

		public abstract IEnumerator RunPattern(Pattern pattern);

		public IEnumerator CRunSequence(Strategy strategy)
		{
			if (_sequenceSelector == null)
			{
				Debug.LogError(base.name + "의 sequence Selector객체가 생성되지 않았습니다.");
				throw new NullReferenceException();
			}
			return _sequenceSelector.CRun(strategy);
		}

		public virtual IEnumerator CRunIntro()
		{
			character.invulnerable.Detach(this);
			_introMotion.TryStart();
			while (_introMotion.running)
			{
				yield return null;
			}
			_adventurerHealthBarAttacher.Show();
		}

		protected override IEnumerator CProcess()
		{
			character.invulnerable.Attach(this);
			if (_commander == null)
			{
				_commander = GetComponentInParent<Commander>();
			}
			yield return CPlayStartOption();
			yield return CRunIntro();
		}

		protected IEnumerator Idle()
		{
			if (CanUsePotion() && MMMaths.RandomBool())
			{
				yield return DrinkPotion();
				yield break;
			}
			_spawnIdleLineText.Run(character);
			yield return _idle.CRun(this);
		}

		protected IEnumerator DrinkPotion()
		{
			yield return _drinkPotion.CRun(this);
		}

		public bool ShouldRunaway()
		{
			return Mathf.Abs(base.target.transform.position.x - character.transform.position.x) < 2f;
		}

		protected IEnumerator CRunaway()
		{
			yield return _runaway.CRun(this);
		}

		protected bool CanUsePotion()
		{
			return character.health.percent < 0.89999997615814209;
		}
	}
}
