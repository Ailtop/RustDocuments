using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Adventurer.Hero;
using Characters.AI.Behaviours.Attacks;
using CutScenes;
using Data;
using Runnables;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class AdventurerHero : AdventurerController
	{
		[Header("Dash")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Hero.Dash))]
		private Characters.AI.Behaviours.Adventurer.Hero.Dash _dash;

		[Header("BackDash")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(BackDash))]
		private BackDash _backDash;

		[Header("ComboAttack")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(ComboAttack))]
		private ComboAttack _commboAttack;

		[Header("EnergyBlast")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(EnergyBlast))]
		private EnergyBlast _energyBlast;

		[Header("EnergyBall")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(EnergyBall))]
		private EnergyBall _energyBall;

		[Header("Stinger")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Stinger))]
		private Stinger _stinger;

		[Header("SwordAuraWave")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(SwordAuraWave))]
		private SwordAuraWave _swordAuraWave;

		[Header("Charge")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(CastingSkill))]
		private CastingSkill _castingSkill;

		[Header("Tool")]
		[Space]
		[SerializeField]
		private Collider2D _dashTrigger;

		[SerializeField]
		private Collider2D _backDashTrigger;

		[Header("Pattern Management")]
		[SerializeField]
		[Subcomponent(typeof(HeroPatternSelector))]
		private HeroPatternSelector _herorPatternSelector;

		[SerializeField]
		private Runnable _cutScene;

		public HeroPatternSelector heroPatternSelector => _herorPatternSelector;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _dash, _backDash, _commboAttack, _energyBlast, _energyBall, _swordAuraWave, _skipableIdle };
			_sequenceSelector = new HeroSequenceSelector(this);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(_checkWithinSight.CRun(this));
			character.invulnerable.Attach(this);
			if (_commander == null)
			{
				_commander = GetComponentInParent<Commander>();
			}
		}

		public override IEnumerator CRunIntro()
		{
			if (!GameData.Progress.cutscene.GetData(CutScenes.Key.rookieHero) && _cutScene != null)
			{
				_cutScene.Run();
			}
			else
			{
				yield return base.CRunIntro();
			}
		}

		protected override IEnumerator CProcess()
		{
			yield return base.CProcess();
		}

		public void CombatTutorial()
		{
			character.invulnerable.Detach(this);
			if (_commander == null)
			{
				_commander = GetComponentInParent<Commander>();
			}
			_adventurerHealthBarAttacher.Show();
		}

		public override IEnumerator RunPattern(Pattern pattern)
		{
			if (!base.dead)
			{
				while (!GameData.Progress.cutscene.GetData(CutScenes.Key.rookieHero))
				{
					yield return null;
				}
				if (pattern is HeroPattern.Dash)
				{
					yield return CastDash();
				}
				else if (pattern is HeroPattern.BackDash)
				{
					yield return CastBackDash();
				}
				else if (pattern is HeroPattern.Stinger)
				{
					yield return CastStinger();
				}
				else if (pattern is HeroPattern.EnergyBlast)
				{
					yield return CastEnergyBlast();
				}
				else if (pattern is HeroPattern.EnergyBall)
				{
					yield return CastEnergyBall();
				}
				else if (pattern is HeroPattern.SwordAuraWave)
				{
					yield return TrySpeacialMove();
				}
				else if (pattern is HeroPattern.ComboAttack)
				{
					yield return CastComoboAttack();
				}
				else if (pattern is HeroPattern.DrinkPotion)
				{
					yield return DrinkPotion();
				}
				else if (pattern is HeroPattern.SkipableIdle)
				{
					yield return SkipableIdle();
				}
				else if (pattern is HeroPattern.Idle)
				{
					yield return Idle();
				}
				else if (pattern is HeroPattern.Runaway)
				{
					yield return CRunaway();
				}
			}
		}

		private IEnumerator CastDash()
		{
			yield return _dash.CRun(this);
		}

		private IEnumerator CastBackDash()
		{
			yield return _backDash.CRun(this);
		}

		private IEnumerator CastComoboAttack()
		{
			yield return _commboAttack.CRun(this);
		}

		private IEnumerator CastSwordAuraWave()
		{
			yield return _swordAuraWave.CRun(this);
		}

		private IEnumerator CastEnergyBlast()
		{
			yield return _energyBlast.CRun(this);
		}

		private IEnumerator CastEnergyBall()
		{
			yield return _energyBall.CRun(this);
		}

		private IEnumerator CastStinger()
		{
			yield return _stinger.CRun(this);
		}

		private IEnumerator SkipableIdle()
		{
			if (CanUsePotion())
			{
				yield return _skipableIdle.CRun(this);
				if (!base.dead && _skipableIdle.result == Characters.AI.Behaviours.Behaviour.Result.Fail)
				{
					yield return DrinkPotion();
				}
			}
			else
			{
				yield return _skipableIdle.CRun(this);
			}
		}

		private IEnumerator DoGroggy()
		{
			_groggy.TryStart();
			while (_groggy.running)
			{
				yield return null;
			}
		}

		private IEnumerator TrySpeacialMove()
		{
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _castingSkill.CRun(this);
			if (!base.dead)
			{
				if (_castingSkill.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return CastSwordAuraWave();
					yield break;
				}
				character.CancelAction();
				yield return DoGroggy();
			}
		}

		public bool CanUseBackDash()
		{
			return _backDash.CanUse();
		}

		public bool CanUseCastingSkill()
		{
			if (character.health.percent < 0.5)
			{
				return _castingSkill.CanUse();
			}
			return false;
		}

		public bool IsTargetWithInDashTrigger()
		{
			return FindClosestPlayerBody(_dashTrigger);
		}

		public bool IsTargetWithInBackDashTrigger()
		{
			return FindClosestPlayerBody(_backDashTrigger);
		}
	}
}
