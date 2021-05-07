using System.Collections;
using System.Collections.Generic;
using FX;
using Level;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations
{
	public class HealEnemy : CharacterOperation
	{
		private enum TargetType
		{
			LowestHealth,
			Random
		}

		private enum HealType
		{
			Percent,
			Constnat
		}

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private TargetType _targetType;

		[SerializeField]
		private HealType _healType;

		[SerializeField]
		private CustomFloat _amount;

		[SerializeField]
		private float _count;

		[SerializeField]
		private float _delay;

		[SerializeField]
		private EffectInfo _background;

		[SerializeField]
		private EffectInfo _effect;

		private EnemyWaveContainer _enemyWaveContainer;

		private static readonly NonAllocOverlapper _enemyOverlapper;

		static HealEnemy()
		{
			_enemyOverlapper = new NonAllocOverlapper(31);
			_enemyOverlapper.contactFilter.SetLayerMask(1024);
		}

		private void Start()
		{
			_enemyWaveContainer = Map.Instance.waveContainer;
		}

		public override void Run(Character owner)
		{
			Character character = SelectTarget(owner);
			_effect.Spawn(character.transform.position, character).transform.SetParent(character.transform);
			_background.Spawn(character.transform.position, character).transform.SetParent(character.transform);
			StartCoroutine(CRun(owner, character));
		}

		private IEnumerator CRun(Character owner, Character target)
		{
			for (int i = 0; (float)i < _count; i++)
			{
				target.health.Heal(GetAmount(target));
				yield return owner.chronometer.master.WaitForSeconds(_delay);
			}
		}

		private double GetAmount(Character target)
		{
			switch (_healType)
			{
			case HealType.Percent:
				return (double)_amount.value * target.health.maximumHealth * 0.01;
			case HealType.Constnat:
				return _amount.value;
			default:
				return 0.0;
			}
		}

		private Character SelectTarget(Character owner)
		{
			List<Character> list = FindEnemiesInRange(_range);
			if (list.Count <= 1)
			{
				return owner;
			}
			switch (_targetType)
			{
			case TargetType.Random:
				return list.Random();
			case TargetType.LowestHealth:
			{
				Character character = owner;
				{
					foreach (Character item in list)
					{
						if (item.gameObject.activeSelf && !item.health.dead && item.health.percent < character.health.percent)
						{
							character = item;
						}
					}
					return character;
				}
			}
			default:
				return null;
			}
		}

		private List<Character> FindEnemiesInRange(Collider2D collider)
		{
			collider.enabled = true;
			List<Character> components = _enemyOverlapper.OverlapCollider(collider).GetComponents<Character>();
			collider.enabled = false;
			return components;
		}
	}
}
