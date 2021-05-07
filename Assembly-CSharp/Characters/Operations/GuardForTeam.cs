using System.Collections;
using System.Collections.Generic;
using Characters.AI.Adventurer;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations
{
	public class GuardForTeam : CharacterOperation
	{
		[SerializeField]
		private Commander _commander;

		[SerializeField]
		private Collider2D _guardRange;

		[SerializeField]
		private Collider2D _guardBuffRange;

		[SerializeField]
		private float _duration;

		[SerializeField]
		[Subcomponent]
		private Subcomponents _onHitToOwner;

		[SerializeField]
		private ChronoInfo _onHitToOwnerChronoInfo;

		[SerializeField]
		[Subcomponent]
		private Subcomponents _onHitToOwnerFromRangeAttack;

		[SerializeField]
		[Subcomponent]
		private Subcomponents _onHitToTarget;

		private Character _owner;

		private static readonly NonAllocOverlapper _teamOverlapper;

		private bool _running;

		private List<Character> _teamCached;

		static GuardForTeam()
		{
			_teamOverlapper = new NonAllocOverlapper(6);
			_teamOverlapper.contactFilter.SetLayerMask(1024);
		}

		private void Awake()
		{
			if (_commander == null)
			{
				_commander = GetComponentInParent<Commander>();
			}
		}

		private bool Block(ref Damage damage)
		{
			Attacker attacker = damage.attacker;
			if (damage.attackType == Damage.AttackType.Additional)
			{
				return false;
			}
			if (damage.attackType == Damage.AttackType.Ranged)
			{
				return false;
			}
			Vector3 position2 = base.transform.position;
			Vector3 position = damage.attacker.transform.position;
			if (_owner.lookingDirection == Character.LookingDirection.Right)
			{
				if (_guardRange.bounds.max.x < position.x)
				{
					GiveGuardEffect(ref damage, attacker.character);
					return true;
				}
			}
			else if (_guardRange.bounds.min.x > position.x)
			{
				GiveGuardEffect(ref damage, attacker.character);
				return true;
			}
			return false;
		}

		public override void Run(Character owner)
		{
			_owner = owner;
			_running = true;
			_teamCached = new List<Character>();
			if (_duration > 0f)
			{
				StartCoroutine(CExpire());
			}
		}

		private void Update()
		{
			if (_running)
			{
				GiveGuardBuff();
			}
		}

		private void GiveGuardEffect(ref Damage damage, Character attacker)
		{
			damage.stoppingPower = 0f;
			if (damage.attackType == Damage.AttackType.Melee)
			{
				_onHitToOwnerChronoInfo.ApplyGlobe();
				if (_onHitToOwner.components.Length != 0)
				{
					for (int i = 0; i < _onHitToOwner.components.Length; i++)
					{
						_onHitToOwner.components[i].Run(_owner);
					}
				}
				if (_onHitToTarget.components.Length != 0)
				{
					for (int j = 0; j < _onHitToTarget.components.Length; j++)
					{
						_onHitToTarget.components[j].Run(attacker);
					}
				}
			}
			else if ((damage.attackType == Damage.AttackType.Ranged || damage.attackType == Damage.AttackType.Projectile) && _onHitToOwnerFromRangeAttack.components.Length != 0)
			{
				for (int k = 0; k < _onHitToOwnerFromRangeAttack.components.Length; k++)
				{
					_onHitToOwnerFromRangeAttack.components[k].Run(_owner);
				}
			}
		}

		private IEnumerator CExpire()
		{
			_running = true;
			yield return _owner.chronometer.master.WaitForSeconds(_duration);
			_running = false;
			Stop();
		}

		public override void Stop()
		{
			_running = false;
			foreach (Combat alife in _commander.alives)
			{
				alife.who.character.health.onTakeDamage.Remove(Block);
			}
			_owner?.health.onTakeDamage.Remove(Block);
		}

		private List<Character> FindTeamBody(Collider2D collider)
		{
			collider.enabled = true;
			List<Character> components = _teamOverlapper.OverlapCollider(collider).GetComponents<Character>();
			if (components.Count == 0)
			{
				collider.enabled = false;
				return null;
			}
			return components;
		}

		private void GiveGuardBuff()
		{
			List<Character> list = FindTeamBody(_guardBuffRange);
			if (list == null)
			{
				foreach (Character item in _teamCached)
				{
					item.health.onTakeDamage.Remove(Block);
				}
			}
			foreach (Character item2 in _teamCached)
			{
				if (!list.Contains(item2))
				{
					item2.health.onTakeDamage.Remove(Block);
				}
			}
			foreach (Character item3 in list)
			{
				if (!item3.health.onTakeDamage.Contains(Block))
				{
					item3.health.onTakeDamage.Add(int.MinValue, Block);
				}
			}
			_teamCached.Clear();
			_teamCached.AddRange(list);
		}
	}
}
