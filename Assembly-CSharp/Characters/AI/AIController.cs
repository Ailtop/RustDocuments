using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.AI.Behaviours;
using FX;
using PhysicsUtils;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI
{
	public abstract class AIController : MonoBehaviour
	{
		private class Assets
		{
			internal static readonly EffectInfo effect = new EffectInfo(Resource.instance.enemyInSightEffect);
		}

		protected enum StartOption
		{
			None,
			IdleUntilFindTarget,
			SetPlayerAsTarget
		}

		private static readonly NonAllocOverlapper _playerOverlapper;

		private static readonly NonAllocOverlapper _groundOverlapper;

		private static readonly NonAllocOverlapper _enemyOverlapper;

		public Character character;

		public Collider2D stopTrigger;

		[SerializeField]
		private Collider2D _notifyCollider;

		[SerializeField]
		private Transform _findEffectTransform;

		[SerializeField]
		private bool _hideFindEffect;

		[SerializeField]
		private StartOption _startOption;

		public Character target { get; set; }

		public Character lastAttacker { get; set; }

		public List<Characters.AI.Behaviours.Behaviour> behaviours { private get; set; }

		public Vector2 destination { get; set; }

		public bool dead => character.health.dead;

		public bool stuned => character.status.stuned;

		public event Action onFind;

		static AIController()
		{
			_playerOverlapper = new NonAllocOverlapper(15);
			_playerOverlapper.contactFilter.SetLayerMask(512);
			_groundOverlapper = new NonAllocOverlapper(15);
			_groundOverlapper.contactFilter.SetLayerMask(Layers.groundMask);
			_enemyOverlapper = new NonAllocOverlapper(31);
			_enemyOverlapper.contactFilter.SetLayerMask(1024);
		}

		public void RunProcess()
		{
			StartCoroutine(CProcess());
		}

		protected abstract IEnumerator CProcess();

		public void FoundEnemy()
		{
			this.onFind?.Invoke();
			NotifyHitEvent();
			if (!_hideFindEffect)
			{
				SpawnFindTargetEffect();
			}
		}

		private void SpawnFindTargetEffect()
		{
			Vector2 vector;
			if (_findEffectTransform == null)
			{
				Bounds bounds = character.collider.bounds;
				vector = new Vector3(bounds.center.x, bounds.max.y);
			}
			else
			{
				vector = _findEffectTransform.position;
			}
			Assets.effect.Spawn(vector);
		}

		public Character FindClosestPlayerBody(Collider2D collider)
		{
			collider.enabled = true;
			List<Target> components = _playerOverlapper.OverlapCollider(collider).GetComponents<Target>();
			if (components.Count == 0)
			{
				collider.enabled = false;
				return null;
			}
			if (components.Count == 1)
			{
				collider.enabled = false;
				return components[0].character;
			}
			float num = float.MaxValue;
			int index = 0;
			for (int i = 1; i < components.Count; i++)
			{
				float distance = Physics2D.Distance(components[i].character.collider, character.collider).distance;
				if (num > distance)
				{
					index = i;
					num = distance;
				}
			}
			collider.enabled = false;
			return components[index].character;
		}

		public List<Character> FindEnemiesInRange(Collider2D collider)
		{
			collider.enabled = true;
			List<Character> components = _enemyOverlapper.OverlapCollider(collider).GetComponents<Character>();
			collider.enabled = false;
			return components;
		}

		public Collider2D FindClosestGround(Collider2D collider)
		{
			collider.enabled = true;
			ReadonlyBoundedList<Collider2D> results = _groundOverlapper.OverlapCollider(collider).results;
			if (results.Count == 0)
			{
				collider.enabled = false;
				return null;
			}
			if (results.Count == 1)
			{
				collider.enabled = false;
				return results[0];
			}
			float num = float.MaxValue;
			int index = 0;
			for (int i = 1; i < results.Count; i++)
			{
				float distance = Physics2D.Distance(results[i], character.collider).distance;
				if (num > distance)
				{
					index = i;
					num = distance;
				}
			}
			collider.enabled = false;
			return results[index];
		}

		public List<Character> FindRandomEnemies(Collider2D collider, Character except, int amount)
		{
			collider.enabled = true;
			List<Character> components = _enemyOverlapper.OverlapCollider(collider).GetComponents<Character>();
			foreach (Character item in components)
			{
				if (item == except)
				{
					components.Remove(item);
					break;
				}
			}
			if (components.Count <= 0)
			{
				collider.enabled = false;
				return null;
			}
			int[] array = Enumerable.Range(0, components.Count).ToArray();
			array.PseudoShuffle();
			IEnumerable<int> enumerable = array.Take(amount);
			List<Character> list = new List<Character>(components.Count);
			foreach (int item2 in enumerable)
			{
				list.Add(components[item2]);
			}
			collider.enabled = false;
			return list;
		}

		protected virtual void OnEnable()
		{
			if (!(character.health == null))
			{
				character.health.onTookDamage += onTookDamage;
			}
		}

		protected virtual void OnDisable()
		{
			if (!(character.health == null))
			{
				character.health.onTookDamage -= onTookDamage;
			}
		}

		protected void Start()
		{
			StartCoroutine(CCheckStun());
		}

		private void onTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!(damageDealt <= 0.0) && !(originalDamage.attacker.character == null) && !(originalDamage.attacker.character.collider == null) && !(originalDamage.attacker.character.health == null) && originalDamage.attacker.character.gameObject.layer == 9)
			{
				if (target == null)
				{
					FoundEnemy();
				}
				target = originalDamage.attacker.character;
				lastAttacker = originalDamage.attacker.character;
			}
		}

		private void NotifyHitEvent()
		{
			if (_notifyCollider == null)
			{
				return;
			}
			List<Character> list = FindEnemiesInRange(_notifyCollider);
			Collider2D lastStandingCollider = character.movement.controller.collisionState.lastStandingCollider;
			foreach (Character item in list)
			{
				Collider2D lastStandingCollider2 = item.movement.controller.collisionState.lastStandingCollider;
				if (!(lastStandingCollider != lastStandingCollider2))
				{
					AIController componentInChildren = item.GetComponentInChildren<AIController>();
					if (!(componentInChildren == null))
					{
						componentInChildren.target = target;
					}
				}
			}
		}

		protected IEnumerator CPlayStartOption()
		{
			StartOption startOption = _startOption;
			if (startOption != StartOption.IdleUntilFindTarget)
			{
				if (startOption == StartOption.SetPlayerAsTarget)
				{
					while (Singleton<Service>.Instance.levelManager.player == null)
					{
						yield return null;
					}
					target = Singleton<Service>.Instance.levelManager.player;
				}
			}
			else
			{
				while (target == null)
				{
					yield return null;
				}
				FoundEnemy();
			}
		}

		public void StopAllCoroutinesWithBehaviour()
		{
			StopAllCoroutines();
			character.CancelAction();
			if (behaviours == null)
			{
				return;
			}
			foreach (Characters.AI.Behaviours.Behaviour behaviour in behaviours)
			{
				behaviour.StopPropagation();
			}
		}

		public void StopAllBehaviour()
		{
			if (behaviours == null)
			{
				return;
			}
			foreach (Characters.AI.Behaviours.Behaviour behaviour in behaviours)
			{
				behaviour.StopPropagation();
			}
		}

		private IEnumerator CCheckStun()
		{
			if (character.status == null)
			{
				yield break;
			}
			while (!dead)
			{
				yield return null;
				if (stuned)
				{
					StopAllBehaviour();
				}
			}
		}
	}
}
