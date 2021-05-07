using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.Movements;
using Characters.Operations;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class BraveAI : AIController
	{
		[Header("Behaviours")]
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(MoveToDestinationWithFly))]
		private MoveToDestinationWithFly _moveToTargetHead;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _attackReady;

		[SerializeField]
		[Subcomponent(typeof(MoveToDestinationWithFly))]
		private MoveToDestinationWithFly _moveToTargetGround;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _attackOperations;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _landingOperations;

		[Space]
		[Header("Tools")]
		[SerializeField]
		private Collider2D _attackTrigger;

		[SerializeField]
		private float _attackHeight;

		private const float _widthCheckRange = 0.2f;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _moveToTargetHead, _moveToTargetGround };
			_attackOperations.Initialize();
			_landingOperations.Initialize();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(_checkWithinSight.CRun(this));
			StartCoroutine(CProcess());
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return CCombat();
		}

		private IEnumerator CCombat()
		{
			character.movement.onGrounded += delegate
			{
				character.movement.config.type = Movement.Config.Type.Walking;
				character.movement.controller.oneWayPlatformMask = 131072;
				StartCoroutine(_landingOperations.CRun(character));
			};
			while (!base.dead)
			{
				character.movement.config.type = Movement.Config.Type.Walking;
				character.movement.config.gravity = -300f;
				character.movement.controller.terrainMask = Layers.terrainMask;
				yield return null;
				if (!(base.target == null))
				{
					if (base.stuned)
					{
						Debug.Log(base.name + " is stuned");
					}
					else if (CheckAttackable())
					{
						yield return CAttack();
						yield return _idle.CRun(this);
					}
				}
			}
		}

		private bool CheckAttackable()
		{
			if (FindClosestPlayerBody(_attackTrigger) == null)
			{
				return false;
			}
			if (base.target.movement.controller.collisionState.lastStandingCollider == null)
			{
				return false;
			}
			if (!base.target.movement.controller.isGrounded)
			{
				return false;
			}
			Bounds bounds = base.target.movement.controller.collisionState.lastStandingCollider.bounds;
			Bounds bounds2 = character.movement.controller.collisionState.lastStandingCollider.bounds;
			if (bounds.max.y != bounds2.max.y)
			{
				return false;
			}
			Bounds bounds3 = base.target.movement.controller.collisionState.lastStandingCollider.bounds;
			float x = base.target.transform.position.x;
			float y = bounds3.max.y + _attackHeight;
			Vector2 vector = new Vector2(x, y);
			Bounds bounds4 = character.collider.bounds;
			bounds4.size = new Vector2(0.2f, bounds4.size.y);
			bounds4.center = new Vector2(vector.x, vector.y + (bounds4.center.y - bounds4.min.y));
			return !TerrainColliding(bounds4);
		}

		private bool TerrainColliding(Bounds range)
		{
			NonAllocOverlapper.shared.contactFilter.SetLayerMask(Layers.terrainMask);
			return NonAllocOverlapper.shared.OverlapBox(range.center, range.size, 0f).results.Count != 0;
		}

		private IEnumerator CAttack()
		{
			Bounds platform = base.target.movement.controller.collisionState.lastStandingCollider.bounds;
			float x = base.target.transform.position.x;
			float y = platform.max.y + _attackHeight;
			base.destination = new Vector2(x, y);
			character.movement.config.type = Movement.Config.Type.Flying;
			character.movement.controller.terrainMask = 0;
			character.movement.controller.oneWayPlatformMask = 0;
			yield return _moveToTargetHead.CRun(this);
			yield return _attackReady.CRun(this);
			y = platform.max.y;
			base.destination = new Vector2(x, y);
			StartCoroutine(_attackOperations.CRun(character));
			character.movement.config.type = Movement.Config.Type.Walking;
			character.movement.controller.terrainMask = Layers.terrainMask;
			character.movement.controller.oneWayPlatformMask = 131072;
		}
	}
}
