using System.Collections;
using Characters;
using PhysicsUtils;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;

namespace Level
{
	public class Map : MonoBehaviour
	{
		public enum Type
		{
			Normal,
			Npc,
			Manual,
			Special
		}

		private static readonly NonAllocOverlapper _rewardActivatingRangeFinder;

		[SerializeField]
		private Type _type;

		[SerializeField]
		private ParallaxBackground _background;

		[SerializeField]
		private bool _displayStageName;

		[SerializeField]
		[FormerlySerializedAs("_hideRightBottomHud")]
		private bool _hideMinimap;

		[SerializeField]
		private bool _verticalLetterBox;

		[SerializeField]
		private CameraZone _cameraZone;

		[SerializeField]
		private Light2D _globalLight;

		[SerializeField]
		private Transform _playerOrigin;

		[SerializeField]
		private Transform _backgroundOrigin;

		[SerializeField]
		private bool _hasCeil;

		[SerializeField]
		private float _ceilHeight = 10f;

		[Header("Gate")]
		[SerializeField]
		private SpriteRenderer _gateWall;

		[SerializeField]
		private SpriteRenderer _gateTable;

		[SerializeField]
		private Transform _gate1Position;

		[SerializeField]
		private Transform _gate2Position;

		[Header("Reward")]
		[SerializeField]
		private MapReward _mapReward;

		[SerializeField]
		private Collider2D _mapRewardActivatingRange;

		private CoroutineReference _lightLerpReference;

		private Gate _gate1;

		private Gate _gate2;

		public static Map Instance { get; private set; }

		public Type type => _type;

		public ParallaxBackground background => _background;

		public EnemyWaveContainer waveContainer { get; private set; }

		public Light2D globalLight => _globalLight;

		public Color originalLightColor { get; private set; }

		public float originalLightIntensity { get; private set; }

		public CameraZone cameraZone
		{
			get
			{
				return _cameraZone;
			}
			set
			{
				_cameraZone = value;
			}
		}

		public Bounds bounds { get; private set; }

		public Vector3 playerOrigin => _playerOrigin.transform.position;

		public Vector3 backgroundOrigin => _backgroundOrigin.transform.position;

		public bool displayStageName => _displayStageName;

		public MapReward mapReward => _mapReward;

		static Map()
		{
			_rewardActivatingRangeFinder = new NonAllocOverlapper(1);
			_rewardActivatingRangeFinder.contactFilter.SetLayerMask(512);
		}

		private void Awake()
		{
			Instance = this;
			TilemapBaker tilemapBaker;
			_003CAwake_003Eg__FindRequiredComponents_007C60_0(out tilemapBaker);
			tilemapBaker.Bake();
			bounds = tilemapBaker.bounds;
			_003CAwake_003Eg__InitializeGates_007C60_1();
			waveContainer?.Initialize();
			SetCameraZoneOrDefault();
			_003CAwake_003Eg__MakeBorders_007C60_2();
			originalLightColor = _globalLight.color;
			originalLightIntensity = _globalLight.intensity;
			UIManager uiManager = Scene<GameBase>.instance.uiManager;
			uiManager.headupDisplay.minimapVisible = !_hideMinimap;
			uiManager.scaler.SetVerticalLetterBox(_verticalLetterBox);
			StartCoroutine(CCheckRewardActivating());
		}

		public void SetReward(MapReward.Type rewardType)
		{
			_mapReward.type = rewardType;
			Sprite sprite = null;
			if (_mapReward.hasReward)
			{
				Chapter currentChapter = Singleton<Service>.Instance.levelManager.currentChapter;
				sprite = ((rewardType == MapReward.Type.Adventurer) ? currentChapter.gateChoiceTable : currentChapter.gateTable);
			}
			_gateTable.sprite = sprite;
			_mapReward.LoadReward();
		}

		public void SetExits(PathNode node1, PathNode node2)
		{
			if (_gate1 == null || _gate2 == null)
			{
				return;
			}
			_gate1.GetComponent<SpriteRenderer>().sortingOrder = -1;
			_gate2.GetComponent<SpriteRenderer>().sortingOrder = -2;
			if (node1.gate == node2.gate && node1.gate != Gate.Type.Boss)
			{
				if (MMMaths.RandomBool())
				{
					_gate1.pathNode = node1;
					_gate2.ShowDestroyed(node1.gate == Gate.Type.Terminal);
				}
				else
				{
					_gate1.ShowDestroyed(node1.gate == Gate.Type.Terminal);
					_gate2.pathNode = node2;
				}
				return;
			}
			if (node1.gate == Gate.Type.None)
			{
				_gate1.ShowDestroyed(node1.gate == Gate.Type.Terminal);
			}
			else
			{
				_gate1.pathNode = node1;
			}
			if (node2.gate == Gate.Type.None)
			{
				_gate2.ShowDestroyed(node2.gate == Gate.Type.Terminal);
			}
			else
			{
				_gate2.pathNode = node2;
			}
		}

		public bool IsInMap(Vector3 position)
		{
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			bool num = position.x > min.x && position.x < max.x;
			bool flag = position.y > min.y && ((_hasCeil && position.y < max.y + _ceilHeight) || !_hasCeil);
			return num && flag;
		}

		public void ChangeLight(Color color, float intensity, float seconds)
		{
			_lightLerpReference.Stop();
			_lightLerpReference = this.StartCoroutineWithReference(CLerp(_globalLight.color, _globalLight.intensity, color, intensity, seconds));
		}

		public void SetCameraZoneOrDefault()
		{
			GameBase instance = Scene<GameBase>.instance;
			if (_cameraZone == null)
			{
				_cameraZone = instance.cameraController.gameObject.AddComponent<CameraZone>();
				_cameraZone.bounds = bounds;
				_cameraZone.hasCeil = _hasCeil;
				Vector3 max = _cameraZone.bounds.max;
				max.y += _ceilHeight;
				_cameraZone.bounds.max = max;
				instance.cameraController.zone = _cameraZone;
				instance.minimapCameraController.zone = _cameraZone;
			}
			instance.cameraController.zone = _cameraZone;
			instance.minimapCameraController.zone = _cameraZone;
		}

		public void ResetCameraZone()
		{
			GameBase instance = Scene<GameBase>.instance;
			if (instance.cameraController.gameObject.GetComponent<CameraZone>() == null)
			{
				_cameraZone = instance.cameraController.gameObject.AddComponent<CameraZone>();
			}
			_cameraZone.bounds = bounds;
			_cameraZone.hasCeil = _hasCeil;
			Vector3 max = _cameraZone.bounds.max;
			max.y += _ceilHeight;
			_cameraZone.bounds.max = max;
			instance.cameraController.zone = _cameraZone;
			instance.minimapCameraController.zone = _cameraZone;
		}

		public void RestoreLight(float seconds)
		{
			_lightLerpReference.Stop();
			_lightLerpReference = this.StartCoroutineWithReference(CLerp(globalLight.color, globalLight.intensity, originalLightColor, originalLightIntensity, seconds));
		}

		private IEnumerator CLerp(Color colorA, float intensityA, Color colorB, float intensityB, float seconds)
		{
			for (float t = 0f; t < 1f; t += Chronometer.global.deltaTime / seconds)
			{
				yield return null;
				globalLight.color = Color.Lerp(colorA, colorB, t);
				globalLight.intensity = Mathf.Lerp(intensityA, intensityB, t);
			}
		}

		private IEnumerator CCheckRewardActivating()
		{
			yield return null;
			while (waveContainer.enemyWaves.Length != 0 && (waveContainer.state == EnemyWaveContainer.State.Remain || _rewardActivatingRangeFinder.OverlapCollider(_mapRewardActivatingRange).GetComponent<Target>() == null))
			{
				yield return null;
			}
			waveContainer.Stop();
			if (_mapReward.Activate())
			{
				_mapReward.onLoot += _003CCCheckRewardActivating_003Eg__ActivateGates_007C69_0;
			}
			else
			{
				_003CCCheckRewardActivating_003Eg__ActivateGates_007C69_0();
			}
		}
	}
}
