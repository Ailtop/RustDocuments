using System.Collections;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using Characters.Operations.Attack;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class GoldmaneManAtArmsAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Wander.Subcomponent(true)]
		private Wander _wander;

		[SerializeField]
		[Chase.Subcomponent(true)]
		private Chase _chase;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _attack;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _tackle;

		[SerializeField]
		private SweepAttack2 _goldenWave;

		[SerializeField]
		private PlaySound _goldenWaveSound;

		[SerializeField]
		private SpawnEffect _goldenWaveEffect;

		[SerializeField]
		[Subcomponent(typeof(CameraShake))]
		private CameraShake _cameraShake;

		[SerializeField]
		[Range(0f, 1f)]
		private float _idleChanceAfterAttack;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[Header("GoldenWave")]
		[Space]
		[SerializeField]
		private Transform _goldenWaveStartPoint;

		[SerializeField]
		private Collider2D _goldenWaveArea;

		[SerializeField]
		private float _goldenWaveTerm;

		[SerializeField]
		private int _goldenWaveCount;

		[SerializeField]
		private float _goldenWaveDistance;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			_goldenWave.Initialize();
			_goldenWaveSound.Initialize();
			yield return CPlayStartOption();
			yield return _wander.CRun(this);
			yield return _idle.CRun(this);
			yield return Combat();
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				yield return null;
				if (base.stuned || base.target == null || !character.movement.controller.isGrounded)
				{
					continue;
				}
				if (FindClosestPlayerBody(stopTrigger) != null)
				{
					if (_tackle.CanUse())
					{
						yield return _tackle.CRun(this);
					}
					yield return CAttack();
				}
				else
				{
					yield return _chase.CRun(this);
					if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
					{
						yield return CAttack();
					}
				}
			}
		}

		private IEnumerator CAttack()
		{
			float num = character.transform.position.x - base.target.transform.position.x;
			character.lookingDirection = ((num > 0f) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
			StartCoroutine(_attack.CRun(this));
			if (_attack.result == Characters.AI.Behaviours.Behaviour.Result.Doing)
			{
				StartCoroutine(DoGoldenWave());
			}
			while (_attack.result == Characters.AI.Behaviours.Behaviour.Result.Doing)
			{
				yield return null;
			}
			if (MMMaths.Chance(_idleChanceAfterAttack))
			{
				yield return _idle.CRun(this);
			}
		}

		private IEnumerator DoGoldenWave()
		{
			float elapsed = 0f;
			while (elapsed < 1f)
			{
				elapsed += character.chronometer.animation.deltaTime;
				yield return null;
			}
			if (base.stuned)
			{
				yield break;
			}
			Bounds platformBounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			float xPosition = _goldenWaveStartPoint.position.x;
			float sizeX = _goldenWaveArea.bounds.size.x;
			float extentsX = _goldenWaveArea.bounds.extents.x;
			int sign = ((character.lookingDirection == Character.LookingDirection.Right) ? 1 : (-1));
			int count = _goldenWaveCount;
			for (int j = 0; j < _goldenWaveCount; j++)
			{
				float num = xPosition + (sizeX * (float)j + extentsX) * (float)sign + (float)j * _goldenWaveDistance * (float)sign;
				if (num >= platformBounds.max.x || num <= platformBounds.min.x)
				{
					count = j + 1;
					break;
				}
			}
			for (int i = 0; i < count; i++)
			{
				_goldenWaveArea.transform.position = new Vector3(xPosition + (sizeX * (float)i + extentsX) * (float)sign + (float)i * _goldenWaveDistance * (float)sign, platformBounds.max.y);
				yield return null;
				_goldenWaveEffect.Run(character);
				_goldenWave.Run(character);
				_goldenWaveSound.Run(character);
				_cameraShake.Run(character);
				yield return character.chronometer.master.WaitForSeconds(_goldenWaveTerm);
				if (base.stuned)
				{
					break;
				}
			}
		}

		private void OnDestroy()
		{
			Object.Destroy(_goldenWaveArea.gameObject);
		}
	}
}
