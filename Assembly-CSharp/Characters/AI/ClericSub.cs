using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public class ClericSub : AIController
	{
		[SerializeField]
		private AdventurerHealthBarAttacher _adventurerHealthBarAttacher;

		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[Header("Intro")]
		[Space]
		[SerializeField]
		private Action _introMotion;

		[Header("Holy Cross")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _hollyCross;

		[SerializeField]
		private Transform _attackArea;

		[Header("Teleport")]
		[SerializeField]
		[Subcomponent(typeof(Teleport))]
		private Teleport _teleport;

		[SerializeField]
		private Collider2D _minimumCollider;

		[Space]
		[SerializeField]
		private Transform _teleportPoint;

		[Header("Give Buff")]
		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _giveBuff;

		[Header("Groggy")]
		[SerializeField]
		private Action _groggy;

		[Header("Heal")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(HealToTarget))]
		private HealToTarget _healToTarget;

		[Header("Massive Heal")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(CastingSkill))]
		private CastingSkill _castingSkill;

		[SerializeField]
		[Subcomponent(typeof(HealToTargets))]
		private HealToTargets _massiveHealToTarget;

		[Header("Potion")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(DrinkPotion))]
		private DrinkPotion _drinkPotion;

		[SerializeField]
		private List<Character> _partners;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _hollyCross };
			_teleportPoint.parent = null;
			_attackArea.parent = null;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			foreach (Character partner in _partners)
			{
				partner.health.onDied += delegate
				{
					_partners.Remove(partner);
				};
			}
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			while (!base.dead)
			{
				yield return Combat();
			}
		}

		private IEnumerator Combat()
		{
			yield return RunIntro();
			while (!base.dead)
			{
				yield return null;
				if (base.target == null || base.stuned)
				{
					continue;
				}
				Character partner = null;
				if (_partners.Count != 0)
				{
					partner = SelectPartner();
				}
				if (partner != null && !partner.health.dead && _castingSkill.CanUse() && CheckSpecialMoveStartCondition())
				{
					yield return TryMassiveHeal();
				}
				if (partner != null && !partner.health.dead && _giveBuff.CanUse())
				{
					yield return DoGiveBuffForPartner();
				}
				if (FindClosestPlayerBody(_minimumCollider) != null && _teleport.CanUse())
				{
					yield return CastEscapeTeleport();
				}
				if (partner != null && !partner.health.dead && partner.health.percent <= 1.0)
				{
					if (MMMaths.RandomBool())
					{
						yield return DoHollyCross();
					}
					else
					{
						yield return DoHealForPartner(partner);
					}
				}
				else
				{
					yield return DoHollyCross();
				}
				Character obj = partner;
				if ((object)obj != null && obj.health.percent > 0.89999997615814209)
				{
					yield return _idle.CRun(this);
				}
				else if (MMMaths.RandomBool())
				{
					yield return DrinkPotion();
				}
				else
				{
					yield return _idle.CRun(this);
				}
			}
		}

		private bool CheckSpecialMoveStartCondition()
		{
			foreach (Character partner in _partners)
			{
				if ((object)partner != null && partner.health.percent < 0.699999988079071)
				{
					return true;
				}
			}
			return false;
		}

		private Character SelectPartner()
		{
			return _partners.Random();
		}

		private IEnumerator RunIntro()
		{
			_introMotion.TryStart();
			while (_introMotion.running)
			{
				yield return null;
			}
			_adventurerHealthBarAttacher.Show();
		}

		private void ShiftAttackArea()
		{
		}

		private IEnumerator CastEscapeTeleport()
		{
			Bounds bounds = ((!(base.target.movement.controller.collisionState.lastStandingCollider != null)) ? character.movement.controller.collisionState.lastStandingCollider.bounds : base.target.movement.controller.collisionState.lastStandingCollider.bounds);
			if (bounds.center.x < base.target.transform.position.x)
			{
				float x = Random.Range(bounds.min.x, base.target.transform.position.x - 5f);
				_teleportPoint.position = new Vector2(x, bounds.max.y);
			}
			else
			{
				float x2 = Random.Range(base.target.transform.position.x + 5f, bounds.max.x);
				_teleportPoint.position = new Vector2(x2, bounds.max.y);
			}
			yield return _teleport.CRun(this);
		}

		private IEnumerator DoHollyCross()
		{
			for (int i = 0; i < 3; i++)
			{
				character.ForceToLookAt(base.target.transform.position.x);
				ShiftAttackArea();
				yield return _hollyCross.CRun(this);
			}
		}

		private IEnumerator DoHealForPartner(Character partner)
		{
			character.ForceToLookAt(base.target.transform.position.x);
			_healToTarget.SetTarget(partner);
			yield return _healToTarget.CRun(this);
		}

		private IEnumerator DoGiveBuffForPartner()
		{
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _giveBuff.CRun(this);
		}

		private IEnumerator DoGroogy()
		{
			_groggy.TryStart();
			while (_groggy.running)
			{
				yield return null;
			}
		}

		private IEnumerator DrinkPotion()
		{
			yield return _drinkPotion.CRun(this);
		}

		private IEnumerator TryMassiveHeal()
		{
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _castingSkill.CRun(this);
			if (_castingSkill.result == Characters.AI.Behaviours.Behaviour.Result.Success)
			{
				yield return _003CTryMassiveHeal_003Eg__DoMassiveHealForPartner_007C30_0();
			}
			else
			{
				yield return DoGroogy();
			}
		}
	}
}
