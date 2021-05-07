using System;
using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.Movements;
using Level;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.TwinSister
{
	public class DarkAideAI : AIController
	{
		private enum Pattern
		{
			GoldenMeteor,
			MeteorAir,
			Rush,
			DimensionPierce,
			RisingPierce,
			Homing,
			MeteorGround,
			Idle,
			SkippableIdle
		}

		[SerializeField]
		private Transform _minHeightTransform;

		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		private Transform _body;

		[SerializeField]
		private Transform _teleportDestination;

		[Header("GoldmaneMeteor")]
		[Space]
		[SerializeField]
		private Characters.Actions.Action _goldenMeteorJump;

		[SerializeField]
		private Characters.Actions.Action _goldenMeteorReady;

		[SerializeField]
		private Characters.Actions.Action _goldenMeteorAttack;

		[SerializeField]
		private Characters.Actions.Action _goldenMeteorLanding;

		private int _countOfGoldenMeteor = 3;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2Int _countOfGoldenMeteorRange;

		[SerializeField]
		private float _heightOfGoldenMeteor;

		[SerializeField]
		private float _durationOfGoldenMeteor;

		[Header("MeteorInTheAir")]
		[Space]
		[SerializeField]
		private Characters.Actions.Action _meteorInAirJumpAndReady;

		[SerializeField]
		private Characters.Actions.Action _meteorInAirAttack;

		[SerializeField]
		private Characters.Actions.Action _meteorInAirLandingAndStanding;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2Int _countOfMeteorInAirRange;

		private int _countOfMeteorInAir = 2;

		[SerializeField]
		private float _durationOfMeteorInAir;

		[SerializeField]
		private float _distanceToPlayerOfMeteorInAir;

		[SerializeField]
		[MinMaxSlider(-180f, 180f)]
		private Vector2 _angleOfMeteorInAir;

		[SerializeField]
		[Subcomponent(typeof(Teleport))]
		private Teleport _teleportForAir;

		[Header("MeteorInTheGround2")]
		[Space]
		[SerializeField]
		private Characters.Actions.Action _meteorInGround2Ready;

		[SerializeField]
		private Characters.Actions.Action _meteorInGround2Attack;

		[SerializeField]
		private Characters.Actions.Action _meteorInGround2Landing;

		[SerializeField]
		private Characters.Actions.Action _meteorInGround2Standing;

		[SerializeField]
		private float _durationOfMeteorInGround2;

		[Subcomponent(typeof(ThunderAttack))]
		[SerializeField]
		private ThunderAttack _thunderAttack;

		[Header("RangeAttackHoming")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(RangeAttack))]
		private RangeAttack _rangeAttack;

		[Header("BackStep")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Teleport))]
		private Teleport _backStepTeleport;

		[Header("Rush")]
		[SerializeField]
		[Space]
		[Subcomponent(typeof(DarkRush))]
		private DarkRush _darkRush;

		[SerializeField]
		private float _darkRushPredelay = 15f;

		private bool _darkRushPredelayEnd;

		[Header("Dimension Piece")]
		[SerializeField]
		private Characters.Actions.Action _dimensionPierce;

		[SerializeField]
		private Characters.Actions.Action _dimensionPierceCoolTimeAction;

		[SerializeField]
		private Transform _dimensionPiercePoint;

		[Space]
		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2Int _dimensionPierceCountRange;

		private int _dimensionPierceCount;

		[Header("Rising Piece")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(RisingPierce))]
		private RisingPierce _risingPierce;

		[Header("Idle")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		[Subcomponent(typeof(SkipableIdle))]
		private SkipableIdle _skippableIdle;

		[Header("Tools")]
		[SerializeField]
		private Collider2D _meleeAttackTrigger;

		private float platformWidth;

		private CharacterController2D.CollisionState _collisionState;

		[Header("Pattern Proportion")]
		[SerializeField]
		[Range(0f, 10f)]
		private int _goldenMeteorPercent;

		[SerializeField]
		[Range(0f, 10f)]
		private int _meteorAirPercent;

		[SerializeField]
		[Range(0f, 10f)]
		private int _meteorGroundPercent;

		[SerializeField]
		[Range(0f, 10f)]
		private int _homingPercent;

		private List<Pattern> _patterns;

		private new void Start()
		{
			base.Start();
			_collisionState = character.movement.controller.collisionState;
			platformWidth = _collisionState.lastStandingCollider.bounds.size.x;
			_patterns = new List<Pattern>(_goldenMeteorPercent + _meteorAirPercent + _meteorGroundPercent + _homingPercent);
			for (int i = 0; i < _goldenMeteorPercent; i++)
			{
				_patterns.Add(Pattern.GoldenMeteor);
			}
			for (int j = 0; j < _meteorAirPercent; j++)
			{
				_patterns.Add(Pattern.MeteorAir);
			}
			for (int k = 0; k < _meteorGroundPercent; k++)
			{
				_patterns.Add(Pattern.MeteorGround);
			}
			for (int l = 0; l < _homingPercent; l++)
			{
				_patterns.Add(Pattern.Homing);
			}
			character.health.onDiedTryCatch += delegate
			{
				_body.rotation = Quaternion.identity;
			};
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return Chronometer.global.WaitForSeconds(1f);
			character.movement.config.type = Movement.Config.Type.Walking;
			StartCoroutine(CStartPredelay());
			while (!base.dead)
			{
				yield return Combat();
			}
		}

		public void ApplyHealth(Character healthOwner)
		{
			if (healthOwner.health.currentHealth > 0.0)
			{
				character.health.SetCurrentHealth(healthOwner.health.currentHealth);
				character.health.PercentHeal(0.4f);
			}
			else
			{
				character.health.SetCurrentHealth(healthOwner.health.maximumHealth);
			}
		}

		private IEnumerator RunPattern(Pattern pattern)
		{
			switch (pattern)
			{
			case Pattern.GoldenMeteor:
				yield return CastGoldenMeteor();
				break;
			case Pattern.MeteorAir:
				yield return CastMeteorInAir();
				break;
			case Pattern.DimensionPierce:
				yield return CastDimensionPierce();
				break;
			case Pattern.Rush:
				yield return CastRush();
				break;
			case Pattern.MeteorGround:
				yield return CastBackstep();
				yield return CastMeteorInGround2();
				break;
			case Pattern.Homing:
				yield return CastRangeAttackHoming();
				break;
			case Pattern.RisingPierce:
				yield return CastRisingPierce();
				break;
			case Pattern.Idle:
				yield return CastIdle();
				break;
			case Pattern.SkippableIdle:
				yield return CastSkippableIdle();
				break;
			}
		}

		private IEnumerator Combat()
		{
			if (CanUseDarkDimensionRush())
			{
				yield return RunPattern(Pattern.Rush);
				yield return RunPattern(Pattern.Idle);
				yield break;
			}
			if (CanUseDarkDimensionPierce())
			{
				yield return RunPattern(Pattern.DimensionPierce);
				yield return RunPattern(Pattern.SkippableIdle);
				yield break;
			}
			Pattern pattern = _patterns.Random();
			yield return RunPattern(pattern);
			switch (pattern)
			{
			case Pattern.GoldenMeteor:
				yield return RunPattern(Pattern.Idle);
				break;
			case Pattern.MeteorAir:
				yield return RunPattern(Pattern.Idle);
				break;
			case Pattern.MeteorGround:
				yield return RunPattern(Pattern.SkippableIdle);
				break;
			case Pattern.Homing:
				yield return RunPattern(Pattern.SkippableIdle);
				break;
			}
		}

		private IEnumerator CastIdle()
		{
			yield return _idle.CRun(this);
		}

		private IEnumerator CastSkippableIdle()
		{
			yield return _skippableIdle.CRun(this);
		}

		public IEnumerator CastGoldenMeteor()
		{
			Bounds platform = _collisionState.lastStandingCollider.bounds;
			_countOfGoldenMeteor = UnityEngine.Random.Range(_countOfGoldenMeteorRange.x, _countOfGoldenMeteorRange.y);
			for (int i = 0; i < _countOfGoldenMeteor; i++)
			{
				Vector2 vector = new Vector2(base.target.transform.position.x, platform.max.y + _heightOfGoldenMeteor);
				_teleportDestination.transform.position = vector;
				yield return _teleportForAir.CRun(this);
				_goldenMeteorJump.TryStart();
				while (_goldenMeteorJump.running)
				{
					yield return null;
				}
				_goldenMeteorReady.TryStart();
				while (_goldenMeteorReady.running)
				{
					yield return null;
				}
				_goldenMeteorAttack.TryStart();
				Vector2 vector2 = character.transform.position;
				yield return MoveToDestination(dest: new Vector2(character.transform.position.x, platform.max.y), source: vector2, action: _goldenMeteorAttack, duration: _durationOfGoldenMeteor);
				_goldenMeteorLanding.TryStart();
				while (_goldenMeteorLanding.running)
				{
					yield return null;
				}
			}
		}

		public IEnumerator CastMeteorInAir()
		{
			Bounds platform = _collisionState.lastStandingCollider.bounds;
			_countOfMeteorInAir = UnityEngine.Random.Range(_countOfMeteorInAirRange.x, _countOfMeteorInAirRange.y);
			for (int i = 0; i < _countOfMeteorInAir; i++)
			{
				Vector2 meteorAirStartPosition = GetMeteorAirStartPosition();
				_teleportDestination.transform.position = meteorAirStartPosition;
				yield return _teleportForAir.CRun(this);
				Vector2 source = character.transform.position;
				Vector2 dest = new Vector2(base.target.transform.position.x, platform.max.y);
				character.ForceToLookAt(dest.x);
				_meteorInAirJumpAndReady.TryStart();
				while (_meteorInAirJumpAndReady.running)
				{
					yield return null;
				}
				_meteorInAirAttack.TryStart();
				yield return MoveToDestination(source, dest, _meteorInAirAttack, _durationOfMeteorInAir, true, false);
				while (_meteorInAirAttack.running)
				{
					yield return null;
				}
				_meteorInAirLandingAndStanding.TryStart();
				while (_meteorInAirLandingAndStanding.running)
				{
					yield return null;
				}
			}
		}

		private Vector2 GetMeteorAirStartPosition()
		{
			float distanceToPlayerOfMeteorInAir = _distanceToPlayerOfMeteorInAir;
			Vector2 vector = Clamp();
			float angle = UnityEngine.Random.Range(vector.x, vector.y);
			Vector2 vector2 = RotateVector(Vector2.right, angle) * distanceToPlayerOfMeteorInAir;
			return new Vector2(y: base.target.movement.controller.collisionState.lastStandingCollider.bounds.max.y, x: base.target.transform.position.x) + vector2;
		}

		private Vector2 Clamp()
		{
			Vector2 angleOfMeteorInAir = _angleOfMeteorInAir;
			angleOfMeteorInAir = MinClamp(angleOfMeteorInAir);
			return MaxClamp(angleOfMeteorInAir);
		}

		private Vector2 MinClamp(Vector2 angle)
		{
			float distanceToPlayerOfMeteorInAir = _distanceToPlayerOfMeteorInAir;
			Vector2 vector = RotateVector(Vector2.right, _angleOfMeteorInAir.x) * distanceToPlayerOfMeteorInAir;
			if (((Vector2)base.target.transform.position + vector).x >= Map.Instance.bounds.max.x)
			{
				return new Vector2(90f, _angleOfMeteorInAir.y);
			}
			return angle;
		}

		private Vector2 MaxClamp(Vector2 angle)
		{
			float distanceToPlayerOfMeteorInAir = _distanceToPlayerOfMeteorInAir;
			Vector2 vector = RotateVector(Vector2.right, _angleOfMeteorInAir.y) * distanceToPlayerOfMeteorInAir;
			if (((Vector2)base.target.transform.position + vector).x <= Map.Instance.bounds.min.x)
			{
				return new Vector2(_angleOfMeteorInAir.x, 90f);
			}
			return angle;
		}

		private Vector2 RotateVector(Vector2 v, float angle)
		{
			float f = angle * ((float)Math.PI / 180f);
			float x = v.x * Mathf.Cos(f) - v.y * Mathf.Sin(f);
			float y = v.x * Mathf.Sin(f) + v.y * Mathf.Cos(f);
			return new Vector2(x, y);
		}

		public IEnumerator CastMeteorInGround2()
		{
			Bounds bounds = _collisionState.lastStandingCollider.bounds;
			Vector2 source = character.transform.position;
			float num = ((source.x > bounds.center.x) ? (bounds.min.x + 3f) : (bounds.max.x - 3f));
			Vector2 dest = new Vector2(num, character.transform.position.y);
			character.ForceToLookAt(num);
			_meteorInGround2Ready.TryStart();
			while (_meteorInGround2Ready.running)
			{
				yield return null;
			}
			_meteorInGround2Attack.TryStart();
			yield return MoveToDestination(source, dest, _meteorInGround2Attack, _durationOfMeteorInGround2);
			_meteorInGround2Landing.TryStart();
			yield return _thunderAttack.CRun(this);
			while (_meteorInGround2Landing.running)
			{
				yield return null;
			}
			_meteorInGround2Standing.TryStart();
			while (_meteorInGround2Standing.running)
			{
				yield return null;
			}
		}

		public IEnumerator CastRush()
		{
			yield return _darkRush.CRun(this);
		}

		public IEnumerator CastRangeAttackHoming()
		{
			yield return _rangeAttack.CRun(this);
		}

		public IEnumerator CastBackstep()
		{
			Bounds platformBounds = _collisionState.lastStandingCollider.bounds;
			float destinationX = ((platformBounds.center.x > character.transform.position.x) ? (platformBounds.max.x - 1f) : (platformBounds.min.x + 1f));
			Vector2 vector = new Vector2(destinationX, platformBounds.max.y);
			_teleportDestination.transform.position = vector;
			character.ForceToLookAt((destinationX > platformBounds.center.x) ? platformBounds.max.x : platformBounds.min.x);
			yield return _backStepTeleport.CRun(this);
			character.ForceToLookAt((destinationX > platformBounds.center.x) ? platformBounds.max.x : platformBounds.min.x);
		}

		public IEnumerator CastDimensionPierce()
		{
			_dimensionPierceCount = UnityEngine.Random.Range(_dimensionPierceCountRange.x, _dimensionPierceCountRange.y);
			for (int i = 0; i < _dimensionPierceCount; i++)
			{
				_dimensionPierce.TryStart();
				while (_dimensionPierce.running)
				{
					yield return null;
				}
			}
			_dimensionPierceCoolTimeAction.TryStart();
		}

		private IEnumerator CastRisingPierce()
		{
			yield return _risingPierce.CRun(this);
		}

		private IEnumerator MoveToDestination(Vector3 source, Vector3 dest, Characters.Actions.Action action, float duration, bool rotate = false, bool interporate = true)
		{
			float elapsed = 0f;
			ClampDestinationY(ref dest);
			if (interporate)
			{
				float num = (source - dest).magnitude / platformWidth;
				duration *= num;
			}
			Character.LookingDirection direction = character.lookingDirection;
			if (rotate)
			{
				Vector3 vector = dest - source;
				float num2 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				if (character.lookingDirection == Character.LookingDirection.Left)
				{
					num2 += 180f;
				}
				_body.rotation = Quaternion.AngleAxis(num2, Vector3.forward);
			}
			while (action.running)
			{
				Vector2 vector2 = Vector2.Lerp(source, dest, elapsed / duration);
				character.transform.position = vector2;
				elapsed += character.chronometer.master.deltaTime;
				if (elapsed > duration)
				{
					character.CancelAction();
					break;
				}
				if ((source - dest).magnitude < 0.1f)
				{
					character.CancelAction();
					break;
				}
				yield return null;
			}
			character.transform.position = dest;
			character.lookingDirection = direction;
			if (rotate)
			{
				_body.rotation = Quaternion.identity;
			}
		}

		private bool CanUseDarkDimensionPierce()
		{
			return _dimensionPierceCoolTimeAction.canUse;
		}

		private bool CanUseDarkDimensionRush()
		{
			if (_darkRush.CanUse())
			{
				return _darkRushPredelayEnd;
			}
			return false;
		}

		private void ClampDestinationY(ref Vector3 dest)
		{
			if (dest.y <= _minHeightTransform.transform.position.y)
			{
				dest.y = _minHeightTransform.transform.position.y;
			}
		}

		private IEnumerator CStartPredelay()
		{
			_darkRushPredelayEnd = false;
			yield return character.chronometer.master.WaitForSeconds(_darkRushPredelay);
			_darkRushPredelayEnd = true;
		}
	}
}
