using System.Collections;
using Characters.Controllers;
using Data;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	[CreateAssetMenu]
	public class Chapter : ScriptableObject
	{
		public enum Type
		{
			Test,
			Castle,
			Tutorial,
			Chapter1,
			Chapter2,
			Chapter3,
			Chapter4,
			Chapter5
		}

		public IStageInfo[] stages;

		[Space]
		[SerializeField]
		private int _smallPotionPrice;

		[SerializeField]
		private int _mediumPotionPrice;

		[SerializeField]
		private int _largePotionPrice;

		[Space]
		[SerializeField]
		private int[] _collectorRefreshCosts;

		[Space]
		[SerializeField]
		private Sprite _gateWall;

		[SerializeField]
		private Sprite _gateTable;

		[SerializeField]
		private Sprite _gateChoiceTable;

		[SerializeField]
		private Gate _gatePrefab;

		private int _stageIndex;

		public string chapterName => Lingua.GetLocalizedString($"map/{type}");

		public string stageTag => Lingua.GetLocalizedString($"map/{type}/{_stageIndex}/tag");

		public string stageName => Lingua.GetLocalizedString($"map/{type}/{_stageIndex}");

		public int smallPotionPrice => _smallPotionPrice;

		public int mediumPotionPrice => _mediumPotionPrice;

		public int largePotionPrice => _largePotionPrice;

		public int[] collectorRefreshCosts => _collectorRefreshCosts;

		public Sprite gateWall => _gateWall;

		public Sprite gateTable => _gateTable;

		public Sprite gateChoiceTable => _gateChoiceTable;

		public Gate gatePrefab => _gatePrefab;

		public Type type { get; private set; }

		public IStageInfo currentStage => stages[_stageIndex];

		public int stageIndex => _stageIndex;

		public Map map { get; private set; }

		public void Initialize(Type type)
		{
			this.type = type;
			for (int i = 0; i < stages.Length; i++)
			{
				stages[i].Initialize();
			}
		}

		public void Enter()
		{
			_stageIndex = 0;
			LoadStage();
		}

		public bool Next()
		{
			PathNode item = currentStage.nextMapTypes.Item1;
			if (item.reference.IsNullOrEmpty() || !currentStage.Next())
			{
				if (NextStage())
				{
					Resources.UnloadUnusedAssets();
					return true;
				}
				return false;
			}
			ChangeMap(item);
			return true;
		}

		public bool Next(PathNode pathNode)
		{
			if (pathNode.reference.IsNullOrEmpty() || !currentStage.Next())
			{
				return NextStage();
			}
			ChangeMap(pathNode);
			return true;
		}

		public void Clear()
		{
			PoolObject.DespawnAllOrphans();
			Singleton<Service>.Instance.levelManager.ClearDrops();
			if (map != null)
			{
				Object.Destroy(map.gameObject);
				map = null;
			}
		}

		public bool NextStage()
		{
			_stageIndex++;
			if (_stageIndex == stages.Length)
			{
				return false;
			}
			LoadStage();
			return true;
		}

		private void LoadStage()
		{
			if (currentStage.music == null)
			{
				PersistentSingleton<SoundManager>.Instance.FadeOutBackgroundMusic();
			}
			else
			{
				PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(currentStage.music);
			}
			currentStage.Reset();
			PathNode item = currentStage.nextMapTypes.Item1;
			currentStage.Next();
			CoroutineProxy.instance.StartCoroutine(CChangeMap(item));
		}

		public void ChangeMap(PathNode pathNode)
		{
			CoroutineProxy.instance.StartCoroutine(CChangeMap(pathNode));
		}

		private IEnumerator CChangeMap(PathNode pathNode)
		{
			_003C_003Ec__DisplayClass53_0 _003C_003Ec__DisplayClass53_ = new _003C_003Ec__DisplayClass53_0();
			_003C_003Ec__DisplayClass53_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass53_.levelManager = Singleton<Service>.Instance.levelManager;
			TrueOnlyLogicalSumList invulnerable = _003C_003Ec__DisplayClass53_.levelManager.player?.invulnerable;
			invulnerable?.Attach(this);
			if (map == null)
			{
				Clear();
				yield return null;
				map = Object.Instantiate(pathNode.reference.Load(), Vector3.zero, Quaternion.identity);
			}
			else
			{
				Resource.Request<Map> newMapRequest = pathNode.reference.LoadAsync();
				PlayerInput.blocked.Attach(this);
				yield return Singleton<Service>.Instance.fadeInOut.CFadeOut();
				Clear();
				PlayerInput.blocked.Detach(this);
				do
				{
					yield return null;
				}
				while (!newMapRequest.isDone);
				map = Object.Instantiate(newMapRequest.asset, Vector3.zero, Quaternion.identity);
			}
			if (!string.IsNullOrWhiteSpace(chapterName) && !string.IsNullOrWhiteSpace(stageName) && map.displayStageName)
			{
				Scene<GameBase>.instance.uiManager.stageName.Show(chapterName, stageTag, stageName);
			}
			map.SetReward(pathNode.reward);
			map.SetExits(currentStage.nextMapTypes.Item1, currentStage.nextMapTypes.Item2);
			invulnerable?.Detach(this);
			_003C_003Ec__DisplayClass53_.levelManager.SpawnPlayerIfNotExist();
			_003C_003Ec__DisplayClass53_._003CCChangeMap_003Eg__ResetPlayerPosition_007C0();
			_003C_003Ec__DisplayClass53_.levelManager.ExcuteInNextFrame(_003C_003Ec__DisplayClass53_._003CCChangeMap_003Eg__ResetPlayerPosition_007C0);
			GameBase instance = Scene<GameBase>.instance;
			Vector3 position = instance.cameraController.transform.position;
			Vector3 vector = -map.backgroundOrigin;
			vector.z = position.z;
			instance.cameraController.Move(map.playerOrigin);
			instance.minimapCameraController.Move(map.playerOrigin);
			instance.SetBackground((map.background == null) ? currentStage.background : map.background, map.playerOrigin.y - map.backgroundOrigin.y);
			_003C_003Ec__DisplayClass53_.levelManager.InvokeOnMapChanged();
			TrueOnlyLogicalSumList playerInvulnerable = _003C_003Ec__DisplayClass53_.levelManager.player.invulnerable;
			playerInvulnerable.Attach(this);
			yield return Singleton<Service>.Instance.fadeInOut.CFadeIn();
			playerInvulnerable.Detach(this);
			GameData.Currency.SaveAll();
			GameData.Progress.SaveAll();
			_003C_003Ec__DisplayClass53_.levelManager.InvokeOnMapChangedAndFadeIn(map);
		}
	}
}
