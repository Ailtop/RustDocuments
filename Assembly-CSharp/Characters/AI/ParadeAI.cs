using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.Movements;
using Characters.Operations;
using Level;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class ParadeAI : AIController
	{
		[Header("Behaviours")]
		[Space]
		[Header("Appearance")]
		[SerializeField]
		[Subcomponent(typeof(MoveToDestinationWithFly))]
		private MoveToDestinationWithFly _appearance;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idleAfterAppearance;

		[Space]
		[Header("Move and Attack")]
		[SerializeField]
		[Subcomponent(typeof(MoveToDestinationWithFly))]
		private MoveToDestinationWithFly _moveToTop;

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
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSpawn;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onGround;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onAttack;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onJump;

		[Space]
		[Header("Tools")]
		[SerializeField]
		private float _attackHeight;

		[SerializeField]
		[MinMaxSlider(1f, 20f)]
		private Vector2 _moveAmountRange;

		private float _moveAmount;

		private float _originGravity;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _moveToTop, _moveToTargetGround };
			_onSpawn.Initialize();
			_onAttack.Initialize();
			_onGround.Initialize();
			_onJump.Initialize();
			_originGravity = character.movement.config.gravity;
			_moveAmount = Random.Range(_moveAmountRange.x, _moveAmountRange.y);
			LookToCenter();
		}

		private void LookToCenter()
		{
			character.lookingDirection = ((character.transform.position.x > Map.Instance.bounds.center.x) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
		}

		protected override IEnumerator CProcess()
		{
			_onSpawn.gameObject.SetActive(true);
			_onSpawn.Run(character);
			yield return CPlayStartOption();
			yield return CCombat();
		}

		private void SetJumpableState()
		{
			character.movement.config.type = Movement.Config.Type.Flying;
			character.movement.config.gravity = 0f;
			character.movement.controller.terrainMask = 0;
			character.movement.controller.oneWayPlatformMask = 0;
		}

		private void SetAttackableState()
		{
			character.movement.config.type = Movement.Config.Type.Walking;
			character.movement.config.gravity = _originGravity;
			character.movement.controller.terrainMask = Layers.terrainMask;
			character.movement.controller.oneWayPlatformMask = 131072;
		}

		private IEnumerator CCombat()
		{
			character.movement.onGrounded += delegate
			{
				SetAttackableState();
				_onGround.gameObject.SetActive(true);
				_onGround.Run(character);
			};
			SetJumpableState();
			SetDestination(0.5f);
			yield return CAttack();
			SetAttackableState();
			yield return _idle.CRun(this);
			while (!base.dead)
			{
				SetAttackableState();
				yield return null;
				if (CheckExitTimingAndSetDestination())
				{
					yield return CDisappear();
					continue;
				}
				yield return CAttack();
				SetAttackableState();
				yield return _idle.CRun(this);
			}
		}

		private bool CheckExitTimingAndSetDestination()
		{
			int num = ((character.lookingDirection == Character.LookingDirection.Right) ? 1 : (-1));
			float num2 = character.transform.position.x + (float)num * _moveAmount;
			Bounds bounds = Map.Instance.bounds;
			float y = character.transform.position.y + _attackHeight;
			base.destination = new Vector2(num2, y);
			if (num2 >= bounds.max.x || num2 <= bounds.min.x)
			{
				return true;
			}
			return false;
		}

		private void SetDestination(float moveAmount)
		{
			int num = ((character.lookingDirection == Character.LookingDirection.Right) ? 1 : (-1));
			float x = character.transform.position.x + (float)num * moveAmount;
			float y = character.transform.position.y + _attackHeight;
			base.destination = new Vector2(x, y);
		}

		private IEnumerator CAttack()
		{
			SetJumpableState();
			_onJump.gameObject.SetActive(true);
			_onJump.Run(character);
			yield return _moveToTop.CRun(this);
			yield return _attackReady.CRun(this);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, Vector2.down, _attackHeight * 2f, Layers.terrainMask);
			if (!raycastHit2D)
			{
				Debug.LogError("Parade's y position was wrong");
				yield break;
			}
			float x = base.transform.position.x;
			float y = raycastHit2D.point.y;
			base.destination = new Vector2(x, y);
			_onAttack.gameObject.SetActive(true);
			_onAttack.Run(character);
		}

		private IEnumerator CAppear()
		{
			int num = ((character.lookingDirection == Character.LookingDirection.Right) ? 1 : (-1));
			float x = character.transform.position.x + (float)num;
			base.destination = new Vector2(x, character.transform.position.y);
			yield return _appearance.CRun(this);
			yield return _idleAfterAppearance.CRun(this);
		}

		private IEnumerator CDisappear()
		{
			character.health.Kill();
			yield break;
		}
	}
}
