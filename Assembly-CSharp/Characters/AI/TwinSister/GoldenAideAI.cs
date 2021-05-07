using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using Characters.Movements;
using Characters.Operations;
using Level;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.AI.TwinSister
{
	public class GoldenAideAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		private Action _dash;

		[SerializeField]
		private Transform _body;

		[Header("Intro")]
		[SerializeField]
		private Action _introFall;

		[SerializeField]
		private Action _introLanding;

		[SerializeField]
		private float _introFallHeight;

		[SerializeField]
		[Space]
		private Transform _landingPoint;

		[Header("Awaken")]
		[Space]
		[SerializeField]
		private ChainAction _awakening;

		[SerializeField]
		private Transform _awakeningPosition;

		[SerializeField]
		private float _durationOfawakeningAppear = 0.8f;

		[Header("TwinAppear")]
		[SerializeField]
		private Action _twinAppear;

		[SerializeField]
		private float _startHeight = 5f;

		[SerializeField]
		private float _endDistanceWithPlatformEdge = 2f;

		[Space]
		[SerializeField]
		private float _durationOfTwinAppear = 0.6f;

		[Header("TwinEscape")]
		[SerializeField]
		private float _endHeight = 3f;

		[SerializeField]
		private float _endWidth = 5f;

		[Space]
		[SerializeField]
		private float _durationOfTwinEscape = 0.8f;

		[Header("TwinMeteor")]
		[SerializeField]
		private Action _twinMeteorEscape;

		[SerializeField]
		private Action _twinMeteorPreparing;

		[SerializeField]
		private Action _twinMeteor;

		[SerializeField]
		private Action _twinMeteorEnding;

		[MinMaxSlider(-10f, 0f)]
		[SerializeField]
		private Vector2 _rangeOfPredictTwinMeteorLeft;

		[MinMaxSlider(0f, 10f)]
		[SerializeField]
		private Vector2 _rangeOfPredictTwinMeteorRight;

		[SerializeField]
		private float _minHeightOfTwinMeteor;

		[SerializeField]
		private float _maxHeightOfTwinMeteor;

		[SerializeField]
		private bool _leftOfTwinMeteor;

		[SerializeField]
		private float _durationOfTwinMeteorEscaping;

		[SerializeField]
		private float _durationOfTwinMeteorPreparing;

		[SerializeField]
		private float _durationOfTwinMeteor;

		[Space]
		[SerializeField]
		private Transform _twinMeteorDestination;

		[Header("GoldenMeteor")]
		[SerializeField]
		private Action _goldenMeteorJump;

		[SerializeField]
		private Action _goldenMeteorReady;

		[SerializeField]
		private Action _goldenMeteorAttack;

		[SerializeField]
		private Action _goldenMeteorLanding;

		[SerializeField]
		private float _heightOfGoldmaneMeteor = 6f;

		[Space]
		[SerializeField]
		private float _durationOfGoldmaneMeteor;

		[Header("MeteorInTheAir")]
		[Space]
		[SerializeField]
		private Action _meteorInAirJump;

		[SerializeField]
		private Action _meteorInAirReady;

		[SerializeField]
		private Action _meteorInAirAttack;

		[SerializeField]
		private Action _meteorInAirLanding;

		[SerializeField]
		private Action _meteorInAirStanding;

		[SerializeField]
		private float _durationOfMeteorInAir;

		[Header("MeteorInTheGround")]
		[SerializeField]
		private Action _meteorInGroundReady;

		[SerializeField]
		private Action _meteorInGroundAttack;

		[SerializeField]
		private Action _meteorInGroundLanding;

		[SerializeField]
		private Action _meteorInGroundStanding;

		[Header("MeteorInTheGround2")]
		[SerializeField]
		private Action _meteorInGround2Ready;

		[SerializeField]
		private Action _meteorInGround2Attack;

		[SerializeField]
		private Action _meteorInGround2Landing;

		[SerializeField]
		private Action _meteorInGround2Standing;

		[Space]
		[SerializeField]
		private float _durationOfMeteorInGround2;

		[Header("RangeAttackHoming")]
		[Space]
		[Attack.Subcomponent(true)]
		[SerializeField]
		private MultiCircularProjectileAttack _rangeAttackHoming;

		[Header("BackStep")]
		[SerializeField]
		private Action _backStep;

		[Space]
		[SerializeField]
		private Transform _backStepDestination;

		[Header("Rush")]
		[SerializeField]
		private ChainAction _rush;

		[SerializeField]
		private Action _rushReady;

		[SerializeField]
		private Action _rushA;

		[SerializeField]
		private Action _rushB;

		[SerializeField]
		private Action _rushC;

		[SerializeField]
		private Action _rushStanding;

		[SerializeField]
		[Subcomponent(typeof(Dash))]
		private Dash _dashOfRush;

		[Space]
		[SerializeField]
		private float _durationOfRush;

		[Header("Dimension Piece")]
		[SerializeField]
		private Action _dimensionPierce;

		[SerializeField]
		private Transform _dimensionPiercePoint;

		[Space]
		[SerializeField]
		private int _dimensionPierceCount;

		[Header("Idle")]
		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		[Subcomponent(typeof(SkipableIdle))]
		private SkipableIdle _skippableIdle;

		[Header("Rising Pierce")]
		[SerializeField]
		private float _preDelayOfRisingPierce = 10f;

		[FormerlySerializedAs("_risingPieceMotion")]
		[SerializeField]
		private Action _risingPierceReady;

		[SerializeField]
		private Action _risingPierceAttackAndEnd;

		[SerializeField]
		private OperationInfos _risingPieceStartAttackOperations;

		[SerializeField]
		private OperationInfos _risingPieceAttackOperations;

		[SerializeField]
		private Collider2D _risingPeieceLeftRange;

		[SerializeField]
		private Collider2D _risingPeieceRightRange;

		[SerializeField]
		private float _risingPeieceTerm;

		[SerializeField]
		private int _risingPeieceCount;

		[Space]
		[SerializeField]
		private float _risingPeieceDistance;

		private float _delayOfRisingPierce = 20f;

		private bool _canUseRisingPierce = true;

		private bool _preDelayOfRisingPierceEnd;

		[Header("Tools")]
		[SerializeField]
		private Collider2D _meleeAttackTrigger;

		[SerializeField]
		private Transform _minHeightTransform;

		private float _platformWidth;

		private CharacterController2D.CollisionState _collisionState;

		private static NonAllocOverlapper _nonAllocOverlapper;

		private const float _maxDistanceOfWall = 4f;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		private new void Start()
		{
			base.Start();
			_collisionState = character.movement.controller.collisionState;
			_platformWidth = _collisionState.lastStandingCollider.bounds.size.x;
			_nonAllocOverlapper = new NonAllocOverlapper(15);
			_nonAllocOverlapper.contactFilter.SetLayerMask(Layers.groundMask);
			_risingPieceAttackOperations.Initialize();
			_risingPieceStartAttackOperations.Initialize();
			character.health.onDiedTryCatch += delegate
			{
				_body.rotation = Quaternion.identity;
			};
		}

		protected override IEnumerator CProcess()
		{
			yield return CIntro();
			character.movement.config.type = Movement.Config.Type.Walking;
			character.movement.controller.terrainMask = Layers.terrainMask;
			while (!base.dead)
			{
				yield return null;
				bool flag = base.target == null;
			}
		}

		public IEnumerator CIntro()
		{
			character.transform.position = new Vector2(_landingPoint.position.x, _landingPoint.position.y + _introFallHeight);
			_introLanding.TryStart();
			while (_introLanding.running && !character.health.dead)
			{
				yield return null;
			}
		}

		public IEnumerator CastAwakening()
		{
			yield return Chronometer.global.WaitForSeconds(1.5f);
			yield return CastAwakeningAppear();
			_awakening.TryStart();
			while (_awakening.running && !character.health.dead)
			{
				yield return null;
			}
		}

		private IEnumerator CastAwakeningAppear()
		{
			character.movement.config.type = Movement.Config.Type.Flying;
			character.movement.controller.terrainMask = 0;
			Bounds bounds = _collisionState.lastStandingCollider.bounds;
			float x = ((_awakeningPosition.position.x < bounds.center.x) ? (bounds.min.x - 4f) : (bounds.max.x + 4f));
			Vector2 vector = new Vector2(x, bounds.max.y + 5f);
			Vector2 vector2 = new Vector2(_awakeningPosition.position.x, bounds.max.y);
			character.transform.position = vector;
			character.lookingDirection = ((!(_awakeningPosition.position.x < bounds.center.x)) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
			_twinAppear.TryStart();
			yield return MoveToDestination(vector, vector2, _twinAppear, _durationOfawakeningAppear, false, false);
			character.movement.config.type = Movement.Config.Type.Walking;
			character.movement.controller.terrainMask = Layers.terrainMask;
		}

		public IEnumerator CastTwinMeteorGround(bool left)
		{
			if (character.health.dead)
			{
				yield break;
			}
			_leftOfTwinMeteor = left;
			yield return CastTwinAppear();
			if (!character.health.dead)
			{
				yield return CastMeteorInGround2();
				if (!character.health.dead)
				{
					yield return EscapeForTwin();
				}
			}
		}

		public IEnumerator CastTwinMeteorChain(bool left, bool ground)
		{
			if (!character.health.dead)
			{
				_leftOfTwinMeteor = left;
				if (ground)
				{
					yield return CastTwinAppear();
					yield return CastMeteorInGround2();
				}
				else
				{
					yield return CastGoldenMeteor();
				}
				yield return EscapeForTwin();
			}
		}

		public IEnumerator CastTwinMeteorPierce(bool left)
		{
			if (!character.health.dead)
			{
				_leftOfTwinMeteor = left;
				_rangeAttackHoming.lookTarget = Map.Instance.bounds.center;
				yield return CastTwinAppear();
				yield return CastRangeAttackHoming(true);
				yield return EscapeForTwin(false);
			}
		}

		public IEnumerator CastTwinMeteor(bool left)
		{
			if (base.dead)
			{
				yield break;
			}
			_leftOfTwinMeteor = left;
			character.lookingDirection = ((!_leftOfTwinMeteor) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
			Bounds platform = character.movement.controller.collisionState.lastStandingCollider.bounds;
			float num = Random.Range(_minHeightOfTwinMeteor, _maxHeightOfTwinMeteor);
			character.transform.position = new Vector2(_leftOfTwinMeteor ? (platform.min.x - 4f) : (platform.max.x + 4f), platform.max.y + num);
			Vector2 source = character.transform.position;
			Vector2 dest = new Vector2(base.target.transform.position.x, platform.max.y);
			_twinMeteorDestination.position = dest;
			_twinMeteorPreparing.TryStart();
			while (_twinMeteorPreparing.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_twinMeteor.TryStart();
			yield return MoveToDestination(source, dest, _twinMeteor, _durationOfTwinMeteor, true, false);
			if (character.lookingDirection == Character.LookingDirection.Right)
			{
				character.transform.position = new Vector2(Mathf.Max(platform.min.x, dest.x - 1.5f), platform.max.y);
			}
			else
			{
				character.transform.position = new Vector2(Mathf.Min(platform.max.x, dest.x + 1.5f), platform.max.y);
			}
			_twinMeteorEnding.TryStart();
			while (_twinMeteorEnding.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			character.movement.config.type = Movement.Config.Type.Walking;
			character.movement.controller.terrainMask = Layers.terrainMask;
		}

		public IEnumerator CastPredictTwinMeteor(bool left)
		{
			_leftOfTwinMeteor = left;
			character.lookingDirection = ((!_leftOfTwinMeteor) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
			Bounds platform = _collisionState.lastStandingCollider.bounds;
			float num = Random.Range(_minHeightOfTwinMeteor, _maxHeightOfTwinMeteor);
			character.transform.position = new Vector2(_leftOfTwinMeteor ? (platform.min.x - 4f) : (platform.max.x + 4f), platform.max.y + num);
			Vector2 vector = (MMMaths.RandomBool() ? _rangeOfPredictTwinMeteorLeft : _rangeOfPredictTwinMeteorRight);
			float num2 = Random.Range(vector.x, vector.y);
			float x = Mathf.Clamp(base.target.transform.position.x + num2, platform.min.x, platform.max.x);
			Vector2 source = character.transform.position;
			Vector2 dest = new Vector2(x, platform.max.y);
			_twinMeteorDestination.position = dest;
			_twinMeteorPreparing.TryStart();
			while (_twinMeteorPreparing.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_twinMeteor.TryStart();
			yield return MoveToDestination(source, dest, _twinMeteor, _durationOfTwinMeteor, true, false);
			if (character.lookingDirection == Character.LookingDirection.Right)
			{
				character.transform.position = new Vector2(Mathf.Max(platform.min.x, dest.x - 2f), platform.max.y);
			}
			else
			{
				character.transform.position = new Vector2(Mathf.Min(platform.max.x, dest.x + 2f), platform.max.y);
			}
			_twinMeteorEnding.TryStart();
			while (_twinMeteorEnding.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			character.movement.config.type = Movement.Config.Type.Walking;
			character.movement.controller.terrainMask = Layers.terrainMask;
		}

		public IEnumerator EscapeForTwinMeteor()
		{
			if (!base.dead)
			{
				_leftOfTwinMeteor = !_leftOfTwinMeteor;
				character.movement.config.type = Movement.Config.Type.Flying;
				character.movement.controller.terrainMask = 0;
				float num = Random.Range(_minHeightOfTwinMeteor, _maxHeightOfTwinMeteor);
				Bounds bounds = _collisionState.lastStandingCollider.bounds;
				Vector2 source = character.transform.position;
				Vector2 dest = new Vector2(_leftOfTwinMeteor ? (bounds.min.x - _endWidth) : (bounds.max.x + _endWidth), bounds.max.y + num);
				character.ForceToLookAt(_leftOfTwinMeteor ? Character.LookingDirection.Left : Character.LookingDirection.Right);
				yield return null;
				_twinMeteorEscape.TryStart();
				yield return MoveToDestination(source, dest, _twinMeteorEscape, _durationOfTwinMeteorEscaping, false, false);
			}
		}

		private IEnumerator EscapeForTwin(bool flipDest = true)
		{
			if (!base.dead)
			{
				if (flipDest)
				{
					_leftOfTwinMeteor = !_leftOfTwinMeteor;
				}
				character.movement.config.type = Movement.Config.Type.Flying;
				character.movement.controller.terrainMask = 0;
				float endHeight = _endHeight;
				Bounds bounds = _collisionState.lastStandingCollider.bounds;
				Vector2 vector = character.transform.position;
				Vector2 vector2 = new Vector2(_leftOfTwinMeteor ? (bounds.min.x - _endWidth) : (bounds.max.x + _endWidth), bounds.max.y + endHeight);
				character.lookingDirection = (_leftOfTwinMeteor ? Character.LookingDirection.Left : Character.LookingDirection.Right);
				_twinMeteorEscape.TryStart();
				yield return MoveToDestination(vector, vector2, _twinMeteorEscape, _durationOfTwinEscape);
			}
		}

		public void PrepareTwinMeteorOfBehind()
		{
			_leftOfTwinMeteor = !_leftOfTwinMeteor;
			character.movement.config.type = Movement.Config.Type.Flying;
			character.movement.controller.terrainMask = 0;
			float num = Random.Range(_minHeightOfTwinMeteor, _maxHeightOfTwinMeteor);
			Bounds bounds = _collisionState.lastStandingCollider.bounds;
			Vector2 vector = new Vector2(_leftOfTwinMeteor ? (bounds.min.x - 4f) : (bounds.max.x + 4f), bounds.max.y + num);
			character.transform.position = vector;
			character.lookingDirection = (_leftOfTwinMeteor ? Character.LookingDirection.Left : Character.LookingDirection.Right);
		}

		public IEnumerator CastGoldenMeteor()
		{
			if (character.health.dead)
			{
				yield break;
			}
			character.movement.config.type = Movement.Config.Type.Walking;
			character.movement.controller.terrainMask = Layers.terrainMask;
			Bounds platform = _collisionState.lastStandingCollider.bounds;
			float num = base.target.transform.position.x;
			if (num + 0.5f >= platform.max.x)
			{
				num -= 0.5f;
			}
			else if (num - 0.5f <= platform.min.x)
			{
				num += 0.5f;
			}
			character.transform.position = new Vector2(num, platform.max.y + _heightOfGoldmaneMeteor);
			_goldenMeteorJump.TryStart();
			while (_goldenMeteorJump.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_goldenMeteorReady.TryStart();
			while (_goldenMeteorReady.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_goldenMeteorAttack.TryStart();
			Vector2 vector = character.transform.position;
			yield return MoveToDestination(dest: new Vector2(character.transform.position.x, platform.max.y), source: vector, action: _goldenMeteorAttack, duration: _durationOfGoldmaneMeteor);
			if (!character.health.dead)
			{
				_goldenMeteorLanding.TryStart();
				while (_goldenMeteorLanding.running && !character.health.dead)
				{
					yield return null;
				}
			}
		}

		public IEnumerator CastMeteorInAir()
		{
			if (character.health.dead)
			{
				yield break;
			}
			character.ForceToLookAt(base.target.transform.position.x);
			_meteorInAirJump.TryStart();
			while (_meteorInAirJump.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			Bounds bounds = _collisionState.lastStandingCollider.bounds;
			Vector2 source = character.transform.position;
			Vector2 dest = new Vector2(base.target.transform.position.x, bounds.max.y);
			_meteorInAirReady.TryStart();
			while (_meteorInAirReady.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			while (character.movement.verticalVelocity > 0f)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			character.ForceToLookAt(dest.x);
			_meteorInAirAttack.TryStart();
			yield return MoveToDestination(source, dest, _meteorInAirAttack, _durationOfMeteorInAir, true);
			if (character.health.dead)
			{
				yield break;
			}
			_meteorInAirLanding.TryStart();
			while (_meteorInAirLanding.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_meteorInAirStanding.TryStart();
			while (_meteorInAirStanding.running && !character.health.dead)
			{
				yield return null;
			}
		}

		public IEnumerator CastMeteorInGround()
		{
			if (character.health.dead)
			{
				yield break;
			}
			_meteorInGroundReady.TryStart();
			while (_meteorInGroundReady.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_meteorInGroundAttack.TryStart();
			while (_meteorInGroundAttack.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_meteorInGroundLanding.TryStart();
			while (_meteorInGroundLanding.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_meteorInGroundStanding.TryStart();
			while (_meteorInGroundStanding.running && !character.health.dead)
			{
				yield return null;
			}
		}

		public IEnumerator CastMeteorInGround2()
		{
			if (character.health.dead)
			{
				yield break;
			}
			Bounds bounds = _collisionState.lastStandingCollider.bounds;
			Vector2 source = character.transform.position;
			float x = ((source.x > bounds.center.x) ? (bounds.min.x + 2f) : (bounds.max.x - 2f));
			Vector2 dest = new Vector2(x, character.transform.position.y);
			_meteorInGround2Ready.TryStart();
			while (_meteorInGround2Ready.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_meteorInGround2Attack.TryStart();
			yield return MoveToDestination(source, dest, _meteorInGround2Attack, _durationOfMeteorInGround2);
			if (character.health.dead)
			{
				yield break;
			}
			_meteorInGround2Landing.TryStart();
			while (_meteorInGround2Landing.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_meteorInGround2Standing.TryStart();
			while (_meteorInGround2Standing.running && !character.health.dead)
			{
				yield return null;
			}
		}

		public IEnumerator CastRush()
		{
			if (character.health.dead)
			{
				yield break;
			}
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _dashOfRush.CRun(this);
			_rushReady.TryStart();
			while (_rushReady.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_rushA.TryStart();
			while (_rushA.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _dashOfRush.CRun(this);
			_rushReady.TryStart();
			while (_rushReady.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_rushB.TryStart();
			while (_rushB.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _dashOfRush.CRun(this);
			_rushReady.TryStart();
			while (_rushReady.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_rushC.TryStart();
			while (_rushC.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_rushStanding.TryStart();
			while (_rushStanding.running && !character.health.dead)
			{
				yield return null;
			}
		}

		public IEnumerator CastDimensionPierce()
		{
			if (!character.health.dead)
			{
				_dimensionPierce.TryStart();
				while (_dimensionPierce.running && !character.health.dead)
				{
					yield return null;
				}
			}
		}

		public IEnumerator CastRisingPierce()
		{
			if (character.health.dead || !_canUseRisingPierce)
			{
				yield break;
			}
			StartCoroutine(CCoolDownRisingPierce());
			_risingPierceReady.TryStart();
			while (_risingPierceReady.running)
			{
				if (character.health.dead)
				{
					yield break;
				}
				yield return null;
			}
			_risingPierceAttackAndEnd.TryStart();
			StartCoroutine(CastPowerWave());
			while (_risingPierceAttackAndEnd.running && !character.health.dead)
			{
				yield return null;
			}
		}

		public IEnumerator CastIdle()
		{
			yield return _idle.CRun(this);
		}

		public IEnumerator CastSkippableIdle()
		{
			yield return _skippableIdle.CRun(this);
		}

		private IEnumerator CastPowerWave()
		{
			if (character.health.dead)
			{
				yield break;
			}
			Bounds platformBounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			float cachedPositionX = character.transform.position.x;
			float sizeX = _risingPeieceLeftRange.bounds.size.x;
			float extentsX = _risingPeieceLeftRange.bounds.extents.x;
			_risingPieceStartAttackOperations.gameObject.SetActive(true);
			_risingPeieceLeftRange.transform.position = new Vector3(cachedPositionX, platformBounds.max.y);
			Physics2D.SyncTransforms();
			_risingPieceStartAttackOperations.Run(character);
			yield return character.chronometer.animation.WaitForSeconds(_risingPeieceTerm);
			for (int i = 1; i < _risingPeieceCount; i++)
			{
				if (character.health.dead)
				{
					break;
				}
				_risingPeieceLeftRange.transform.position = new Vector3(cachedPositionX + sizeX * (float)(-i) + (float)(-i) * _risingPeieceDistance - extentsX, platformBounds.max.y);
				_risingPeieceRightRange.transform.position = new Vector3(cachedPositionX + sizeX * (float)i + (float)i * _risingPeieceDistance + extentsX, platformBounds.max.y);
				Physics2D.SyncTransforms();
				_risingPieceAttackOperations.gameObject.SetActive(true);
				_risingPieceAttackOperations.Run(character);
				yield return character.chronometer.animation.WaitForSeconds(_risingPeieceTerm);
			}
		}

		public IEnumerator CastBackstep()
		{
			if (!character.health.dead)
			{
				Bounds bounds = _collisionState.lastStandingCollider.bounds;
				float num = ((base.target.transform.position.x < character.transform.position.x) ? bounds.max.x : bounds.min.x);
				Character.LookingDirection lookingDirection = ((base.target.transform.position.x < character.transform.position.x) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
				if (Mathf.Abs(character.transform.position.x - num) <= 2f)
				{
					lookingDirection = ((lookingDirection == Character.LookingDirection.Right) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
				}
				character.ForceToLookAt(lookingDirection);
				_backStep.TryStart();
				while (_backStep.running && !character.health.dead)
				{
					yield return null;
				}
			}
		}

		public IEnumerator CastRangeAttackHoming(bool centerTarget)
		{
			if (!character.health.dead)
			{
				if (centerTarget)
				{
					Bounds bounds = base.target.movement.controller.collisionState.lastStandingCollider.bounds;
					_rangeAttackHoming.lookTarget = bounds.center;
				}
				else
				{
					_rangeAttackHoming.lookTarget = base.target.transform.position;
				}
				yield return _rangeAttackHoming.CRun(this);
			}
		}

		public bool IsMeleeCombat()
		{
			return FindClosestPlayerBody(_meleeAttackTrigger) != null;
		}

		private IEnumerator MoveToDestination(Vector3 source, Vector3 dest, Action action, float duration, bool rotate = false, bool interporate = true)
		{
			float elapsed = 0f;
			ClampDestinationY(ref dest);
			if (interporate)
			{
				float num = (source - dest).magnitude / _platformWidth;
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
				yield return null;
				Vector2 vector2 = Vector2.Lerp(source, dest, elapsed / duration);
				character.movement.transform.position = vector2;
				elapsed += character.chronometer.animation.deltaTime;
				if (character.health.dead)
				{
					character.movement.config.type = Movement.Config.Type.Walking;
					character.movement.controller.terrainMask = Layers.terrainMask;
					_body.rotation = Quaternion.identity;
					yield break;
				}
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
			}
			character.transform.position = dest;
			character.lookingDirection = direction;
			if (rotate)
			{
				_body.rotation = Quaternion.identity;
			}
		}

		public IEnumerator CastDash(float stopDistance = 0f)
		{
			if (!character.health.dead)
			{
				float num = character.transform.position.x - base.target.transform.position.x;
				character.ForceToLookAt(base.target.transform.position.x);
				_dash.TryStart();
				float num2 = ((num > 0f) ? stopDistance : (0f - stopDistance));
				Vector2 vector = character.transform.position;
				yield return MoveToDestination(dest: new Vector2(base.target.transform.position.x + num2, character.transform.position.y), source: vector, action: _dash, duration: _durationOfRush);
				character.CancelAction();
			}
		}

		private IEnumerator CastTwinAppear()
		{
			if (!character.health.dead)
			{
				character.movement.config.type = Movement.Config.Type.Flying;
				character.movement.controller.terrainMask = 0;
				Bounds bounds = _collisionState.lastStandingCollider.bounds;
				Vector2 vector = new Vector2(_leftOfTwinMeteor ? (bounds.min.x - 4f) : (bounds.max.x + 4f), bounds.max.y + _startHeight);
				Vector2 vector2 = new Vector2(_leftOfTwinMeteor ? (bounds.min.x + _endDistanceWithPlatformEdge) : (bounds.max.x - _endDistanceWithPlatformEdge), bounds.max.y);
				character.transform.position = vector;
				character.lookingDirection = ((!_leftOfTwinMeteor) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
				_twinAppear.TryStart();
				yield return MoveToDestination(vector, vector2, _twinAppear, _durationOfTwinAppear, false, false);
				character.movement.config.type = Movement.Config.Type.Walking;
				character.movement.controller.terrainMask = Layers.terrainMask;
			}
		}

		public void Hide()
		{
			character.@base.gameObject.SetActive(false);
			character.attach.SetActive(false);
		}

		public void Show()
		{
			character.@base.gameObject.SetActive(true);
			character.attach.SetActive(true);
		}

		public void Dettachinvincibility()
		{
			character.invulnerable.Detach(this);
		}

		public void Attachinvincibility()
		{
			character.invulnerable.Attach(this);
		}

		public bool CanUseDimensionPierce()
		{
			return _dimensionPierce.canUse;
		}

		public bool CanUseRisingPierce()
		{
			if (_risingPierceAttackAndEnd.canUse && _preDelayOfRisingPierceEnd)
			{
				return _canUseRisingPierce;
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

		public IEnumerator CStartSinglePhasePreDelay()
		{
			_preDelayOfRisingPierceEnd = false;
			yield return character.chronometer.animation.WaitForSeconds(_preDelayOfRisingPierce);
			_preDelayOfRisingPierceEnd = true;
		}

		private IEnumerator CCoolDownRisingPierce()
		{
			_canUseRisingPierce = false;
			yield return character.chronometer.master.WaitForSeconds(_delayOfRisingPierce);
			_canUseRisingPierce = true;
		}
	}
}
