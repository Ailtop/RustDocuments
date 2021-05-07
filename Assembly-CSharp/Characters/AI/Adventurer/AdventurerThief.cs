using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Adventurer.Thief;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class AdventurerThief : AdventurerController
	{
		[Header("ShadowStep")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(ShadowStep))]
		private ShadowStep _shadowStep;

		[Header("Flashcut")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(FlashCut))]
		private FlashCut _flashCut;

		[Header("Gigantic Shuriken")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(GiganticShuriken))]
		private GiganticShuriken _giganticShuriken;

		[SerializeField]
		[Subcomponent(typeof(GiganticShuriken))]
		private GiganticShuriken _giganticShurikenLongCool;

		[Header("Shuriken")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Shuriken))]
		private Shuriken _shuriken;

		[Header("Multiple Bunshin")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(MultipleBunshin))]
		private MultipleBunshin _multipleBunshin;

		[Header("SwitchTeleport")]
		[Space]
		[SerializeField]
		[Range(0f, 1f)]
		private float _chanceOfSwitchTeleport;

		[SerializeField]
		private Action _switchTeleport;

		private bool _orderSwitchTeleport;

		[Header("Tool")]
		[Space]
		[SerializeField]
		private Transform _teleportDestination;

		[Header("Pattern Management")]
		[Subcomponent(typeof(ThiefPatternSelector))]
		[SerializeField]
		private ThiefPatternSelector _thiefPatternSelector;

		public ThiefPatternSelector thiefPatternSelector => _thiefPatternSelector;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _shadowStep, _flashCut, _giganticShuriken, _shuriken, _multipleBunshin };
			_sequenceSelector = new ThiefSequenceSelector(this);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			character.invulnerable.Attach(this);
			if (_commander == null)
			{
				_commander = GetComponentInParent<Commander>();
			}
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			character.health.onTakeDamage.Remove(TrySwitchTeleport);
		}

		protected override IEnumerator CProcess()
		{
			yield return base.CProcess();
		}

		public override IEnumerator RunPattern(Pattern pattern)
		{
			if (!base.dead)
			{
				if (pattern is ThiefPattern.ShadowStep)
				{
					yield return CastShadowStep();
				}
				else if (pattern is ThiefPattern.FlashCut)
				{
					yield return CastFlashCut();
				}
				else if (pattern is ThiefPattern.Shuriken)
				{
					yield return CastSuriken();
				}
				else if (pattern is ThiefPattern.GiganticShuriken)
				{
					yield return CastGiganticShuriken();
				}
				else if (pattern is ThiefPattern.GiganticShurikenLongCool)
				{
					yield return CastGiganticShurikenLongCool();
				}
				else if (pattern is ThiefPattern.MultipleBunshin)
				{
					yield return CastMultipleBunshin();
				}
				else if (pattern is ThiefPattern.SwithingTeleport)
				{
					yield return CastSwitchingTeleport();
				}
				else if (pattern is ThiefPattern.DrinkPotion)
				{
					yield return DrinkPotion();
				}
				else if (pattern is ThiefPattern.SkipableIdle)
				{
					yield return SkipableIdle();
				}
				else if (pattern is ThiefPattern.Idle)
				{
					yield return Idle();
				}
				else if (pattern is ThiefPattern.Runaway)
				{
					yield return CRunaway();
				}
			}
		}

		private bool TrySwitchTeleport(ref Damage damage)
		{
			if (!_switchTeleport.canUse)
			{
				return false;
			}
			if (!_orderSwitchTeleport && MMMaths.Chance(_chanceOfSwitchTeleport))
			{
				StopAllCoroutines();
				_orderSwitchTeleport = true;
				StartCoroutine(CProcess());
				return true;
			}
			return false;
		}

		private IEnumerator CastShadowStep()
		{
			yield return _shadowStep.CRun(this);
		}

		private IEnumerator CastFlashCut()
		{
			yield return _flashCut.CRun(this);
		}

		private IEnumerator CastSuriken()
		{
			yield return _shuriken.CRun(this);
		}

		private IEnumerator CastGiganticShuriken()
		{
			yield return _giganticShuriken.CRun(this);
		}

		private IEnumerator CastGiganticShurikenLongCool()
		{
			yield return _giganticShurikenLongCool.CRun(this);
		}

		private IEnumerator CastMultipleBunshin()
		{
			yield return _multipleBunshin.CRun(this);
			if (_multipleBunshin.result == Characters.AI.Behaviours.Behaviour.Result.Fail)
			{
				yield return DoGroggy();
			}
		}

		private IEnumerator CastSwitchingTeleport()
		{
			Bounds bounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			Vector2 vector = ((!(base.target.transform.position.x > bounds.center.x)) ? new Vector2(Random.Range(bounds.center.x, bounds.max.x), bounds.max.y) : new Vector2(Random.Range(bounds.min.x, bounds.center.x), bounds.max.y));
			_teleportDestination.position = vector;
			_switchTeleport.TryStart();
			while (_switchTeleport.running)
			{
				yield return null;
			}
			if (character.transform.position.x > base.target.transform.position.x)
			{
				character.lookingDirection = Character.LookingDirection.Left;
			}
			else
			{
				character.lookingDirection = Character.LookingDirection.Right;
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

		public bool CanUseCastingSkill()
		{
			if (_multipleBunshin.CanUse())
			{
				return CheckSpecialMoveStartCondition();
			}
			return false;
		}

		public bool CanUseGiganticShuriken()
		{
			return _giganticShuriken.CanUse();
		}

		public bool CanUseGiganticShurikenLongCool()
		{
			return _giganticShurikenLongCool.CanUse();
		}

		private bool CheckSpecialMoveStartCondition()
		{
			return character.health.percent < 0.60000002384185791;
		}
	}
}
