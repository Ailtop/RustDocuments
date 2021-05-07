using System.Collections;
using Characters;
using Characters.Abilities;
using Characters.Actions;
using Characters.AI;
using Characters.AI.Behaviours;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	public class TentacleAI : AIController
	{
		[SerializeField]
		private SpriteRenderer _corpseRenderer;

		[SerializeField]
		private CharacterAnimation _animation;

		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[Space]
		[Header("Appearance")]
		[SerializeField]
		private Action _appearance;

		[Space]
		[Header("Attack")]
		[SerializeField]
		private Action _attackAction;

		[SerializeField]
		private Collider2D _attackTrigger;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[Space]
		[SerializeField]
		[AbilityAttacher.Subcomponent]
		private AbilityAttacher _abilityAttacher;

		private float _elapsedTime;

		private readonly NonAllocOverlapper _overlapper = new NonAllocOverlapper(1);

		private void Awake()
		{
			_attackAction.Initialize(character);
			_appearance.Initialize(character);
			_abilityAttacher.Initialize(character);
			_abilityAttacher.StartAttach();
		}

		private new void OnEnable()
		{
			base.OnEnable();
			_elapsedTime = 0f;
			base.transform.parent = Map.Instance.transform;
			StartCoroutine(_checkWithinSight.CRun(this));
			StartCoroutine(CProcess());
		}

		public void Appear(Transform point, Sprite corpse, bool flip)
		{
			_corpseRenderer.sprite = corpse;
			if (flip)
			{
				_corpseRenderer.flipX = true;
			}
			character.health.onDied += delegate
			{
				_corpseRenderer.transform.SetParent(Map.Instance.transform);
				_corpseRenderer.sortingOrder = character.sortingGroup.sortingOrder;
			};
			base.transform.position = point.position;
			base.gameObject.SetActive(true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
		}

		private void FindPlayer()
		{
			_attackTrigger.enabled = true;
			_overlapper.contactFilter.SetLayerMask(512);
			_overlapper.OverlapCollider(_attackTrigger);
			_attackTrigger.enabled = false;
		}

		private void OnDestroy()
		{
			_abilityAttacher.StopAttach();
		}

		protected override IEnumerator CProcess()
		{
			yield return null;
			_appearance.TryStart();
			while (_appearance.running)
			{
				yield return null;
			}
			while (!base.dead)
			{
				if (base.target == null)
				{
					yield return null;
					continue;
				}
				do
				{
					yield return null;
					FindPlayer();
				}
				while (_overlapper.results.Count == 0);
				_attackAction.TryStart();
				while (_attackAction.running)
				{
					yield return null;
				}
				yield return _idle.CRun(this);
			}
		}
	}
}
