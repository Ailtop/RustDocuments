using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Adventurer.Cleric;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class AdventurerCleric : AdventurerController
	{
		[Header("Holy Cross")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(HollyCross))]
		private HollyCross _hollyCross;

		[Header("Teleport")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Cleric.EscapeTeleport))]
		private Characters.AI.Behaviours.Adventurer.Cleric.EscapeTeleport _escapeTeleport;

		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Cleric.EscapeTeleport))]
		private Characters.AI.Behaviours.Adventurer.Cleric.EscapeTeleport _escapeTeleportLongCool;

		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Cleric.ChaseTeleport))]
		private Characters.AI.Behaviours.Adventurer.Cleric.ChaseTeleport _chaseTeleport;

		[Header("Reinforce")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Reinforce))]
		private Reinforce _reinforce;

		[Header("Heal")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Cleric.Heal))]
		private Characters.AI.Behaviours.Adventurer.Cleric.Heal _heal;

		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Cleric.Heal))]
		private Characters.AI.Behaviours.Adventurer.Cleric.Heal _healShortCool;

		[Header("Massive Heal")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(MassiveHeal))]
		private MassiveHeal _massiveHeal;

		[Header("Casting")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(CastingSkill))]
		private CastingSkill _castingSkill;

		[Header("Tools")]
		[Space]
		[SerializeField]
		private Collider2D _minimumCollider;

		[SerializeField]
		[Subcomponent(typeof(ClericPatternSelector))]
		private ClericPatternSelector _clericPatternSelector;

		public ClericPatternSelector clericPatternSelector => _clericPatternSelector;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _hollyCross, _massiveHeal, _escapeTeleport, _reinforce, _heal, _castingSkill };
			_sequenceSelector = new ClericSequenceSelector(this);
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

		protected override IEnumerator CProcess()
		{
			yield return base.CProcess();
		}

		public override IEnumerator RunPattern(Pattern pattern)
		{
			if (!base.dead)
			{
				if (pattern is ClericPattern.Reinforce)
				{
					yield return CastReinforce();
				}
				else if (pattern is ClericPattern.Heal)
				{
					yield return CastHeal();
				}
				else if (pattern is ClericPattern.HealShortCool)
				{
					yield return CastHealShortCool();
				}
				else if (pattern is ClericPattern.HolyCross)
				{
					yield return CastHollyCross();
				}
				else if (pattern is ClericPattern.EscapeTeleport)
				{
					yield return CastEscapeTeleport();
				}
				else if (pattern is ClericPattern.EscapeTeleportLongCool)
				{
					yield return CastEscapeTeleportLongCool();
				}
				else if (pattern is ClericPattern.MassiveHeal)
				{
					yield return TryMassiveHeal();
				}
				else if (pattern is ClericPattern.SkipableIdle)
				{
					yield return SkipableIdle();
				}
				else if (pattern is ClericPattern.Idle)
				{
					yield return Idle();
				}
				else if (pattern is ClericPattern.DrinkPotion)
				{
					yield return DrinkPotion();
				}
				else if (pattern is ClericPattern.Runaway)
				{
					yield return CRunaway();
				}
				else if (pattern is ClericPattern.ChaseTeleport)
				{
					yield return CastChaseTeleport();
				}
			}
		}

		private IEnumerator CastReinforce()
		{
			yield return _reinforce.CRun(this);
		}

		private IEnumerator CastHollyCross()
		{
			yield return _hollyCross.CRun(this);
		}

		private IEnumerator CastMassiveHeal()
		{
			yield return _massiveHeal.CRun(this);
		}

		private IEnumerator CastEscapeTeleport()
		{
			yield return _escapeTeleport.CRun(this);
		}

		private IEnumerator CastEscapeTeleportLongCool()
		{
			yield return _escapeTeleportLongCool.CRun(this);
		}

		private IEnumerator CastChaseTeleport()
		{
			yield return _chaseTeleport.CRun(this);
		}

		private IEnumerator CastHeal()
		{
			yield return _heal.CRun(this);
		}

		private IEnumerator CastHealShortCool()
		{
			yield return _healShortCool.CRun(this);
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

		private IEnumerator TryMassiveHeal()
		{
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _castingSkill.CRun(this);
			if (!base.dead)
			{
				if (_castingSkill.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return CastMassiveHeal();
					yield break;
				}
				character.CancelAction();
				yield return DoGroggy();
			}
		}

		public bool CanUseCastingSkill()
		{
			if (_castingSkill.CanUse())
			{
				return CanUseMassiveHeal();
			}
			return false;
		}

		private bool CanUseMassiveHeal()
		{
			return character.health.percent < 0.5;
		}

		public bool CanUseEscapeTeleport()
		{
			if (IsTargetWithInMinimumDistance())
			{
				return _escapeTeleport.CanUse();
			}
			return false;
		}

		public bool CanUseEscapeTeleportLongCool()
		{
			if (IsTargetWithInMinimumDistance())
			{
				return _escapeTeleportLongCool.CanUse();
			}
			return false;
		}

		public bool CanUseSelfHeal()
		{
			if (_heal.CanUse())
			{
				return character.health.percent < 0.89999997615814209;
			}
			return false;
		}

		public bool CanUseHealShortCool()
		{
			if (_commander.GetLowestHealthCharacter(null).health.percent < 0.89999997615814209)
			{
				return _healShortCool.CanUse();
			}
			return false;
		}

		public bool IsTargetWithInMinimumDistance()
		{
			return FindClosestPlayerBody(_minimumCollider);
		}

		public bool HasParty()
		{
			return _commander.alives.Count > 1;
		}
	}
}
