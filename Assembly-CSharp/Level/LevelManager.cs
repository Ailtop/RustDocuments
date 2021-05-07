using System;
using System.Collections.Generic;
using Characters;
using Characters.Gear;
using Characters.Gear.Items;
using Characters.Gear.Quintessences;
using Characters.Gear.Weapons;
using Characters.Player;
using Data;
using Level.Npc.Prophecies;
using Scenes;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Level
{
	public sealed class LevelManager : MonoBehaviour
	{
		public delegate void OnMapChangedDelegate(Map old, Map @new);

		private Dictionary<string, Character> _monsters;

		private Map _oldMap;

		private Chapter[] _chapters;

		private List<PoolObject> _drops = new List<PoolObject>();

		private int _currentChapterIndex;

		public bool earlyAccessEnding { get; set; }

		public Character player { get; private set; }

		public Chapter currentChapter => _chapters[_currentChapterIndex];

		public bool clearing { get; private set; }

		public event Action onMapLoaded;

		public event Action onMapLoadedAndFadedIn;

		public event OnMapChangedDelegate onMapChangedAndFadedIn;

		public event Action onChapterLoaded;

		private void Awake()
		{
			_chapters = Resource.instance.chapters;
			for (int i = 0; i < _chapters.Length; i++)
			{
				_chapters[i].Initialize(EnumValues<Chapter.Type>.Values[i]);
			}
		}

		public void ClearDrops()
		{
			clearing = true;
			for (int num = _drops.Count - 1; num >= 0; num--)
			{
				_drops[num].Despawn();
			}
			_drops.Clear();
			clearing = false;
		}

		public void RegisterDrop(PoolObject poolObject)
		{
			_drops.Add(poolObject);
		}

		public void DeregisterDrop(PoolObject poolObject)
		{
			if (!clearing)
			{
				_drops.Remove(poolObject);
			}
		}

		public void DropGold(int amount, int count)
		{
			DropGold(amount, count, player.transform.position);
		}

		public void DropGold(int amount, int count, Vector3 position)
		{
			DropCurrency(GameData.Currency.Type.Gold, amount, count, position);
		}

		public void DropGold(int amount, int count, Vector3 position, Vector2 force)
		{
			DropCurrency(GameData.Currency.Type.Gold, amount, count, position, force);
		}

		public void DropDarkQuartz(int amount)
		{
			DropDarkQuartz(amount, player.transform.position);
		}

		public void DropDarkQuartz(int amount, Vector3 position)
		{
			DropDarkQuartz(amount, Mathf.Min(amount, 20), position);
		}

		public void DropDarkQuartz(int amount, Vector3 position, Vector2 force)
		{
			DropDarkQuartz(amount, Mathf.Min(amount, 20), position, force);
		}

		public void DropDarkQuartz(int amount, int count, Vector3 position)
		{
			DropCurrency(GameData.Currency.Type.DarkQuartz, amount, count, position);
		}

		public void DropDarkQuartz(int amount, int count, Vector3 position, Vector2 force)
		{
			DropCurrency(GameData.Currency.Type.DarkQuartz, amount, count, position, force);
		}

		public void DropBone(int amount, int count)
		{
			DropBone(amount, count, player.transform.position);
		}

		public void DropBone(int amount, int count, Vector3 position)
		{
			DropCurrency(GameData.Currency.Type.Bone, amount, count, position);
		}

		public void DropBone(int amount, int count, Vector3 position, Vector2 force)
		{
			DropCurrency(GameData.Currency.Type.Bone, amount, count, position, force);
		}

		public void DropCurrency(GameData.Currency.Type type, int amount, int count, Vector3 position)
		{
			if (count == 0)
			{
				return;
			}
			int currencyAmount = amount / count;
			PoolObject currencyParticle = Resource.instance.GetCurrencyParticle(type);
			for (int i = 0; i < count; i++)
			{
				CurrencyParticle component = currencyParticle.Spawn(position).GetComponent<CurrencyParticle>();
				component.currencyType = type;
				component.currencyAmount = currencyAmount;
				if (i == 0)
				{
					component.currencyAmount += amount % count;
				}
			}
		}

		public void DropCurrency(GameData.Currency.Type type, int amount, int count, Vector3 position, Vector2 force)
		{
			if (count == 0)
			{
				return;
			}
			int currencyAmount = amount / count;
			PoolObject currencyParticle = Resource.instance.GetCurrencyParticle(type);
			for (int i = 0; i < count; i++)
			{
				CurrencyParticle component = currencyParticle.Spawn(position).GetComponent<CurrencyParticle>();
				component.currencyType = type;
				component.currencyAmount = currencyAmount;
				component.SetForce(force);
				if (i == 0)
				{
					component.currencyAmount += amount % count;
				}
			}
		}

		public Weapon DropWeapon(Weapon weapon)
		{
			return DropWeapon(weapon, player.transform.position);
		}

		public Weapon DropWeapon(Weapon weapon, Vector3 position)
		{
			if (weapon == null)
			{
				return null;
			}
			Weapon weapon2 = UnityEngine.Object.Instantiate(weapon, position, Quaternion.identity);
			weapon2.name = weapon.name;
			weapon2.transform.parent = Map.Instance.transform;
			weapon2.Initialize();
			Singleton<Service>.Instance.gearManager.SpawnFx(weapon2);
			return weapon2;
		}

		public Item DropItem(Item item)
		{
			return DropItem(item, player.transform.position);
		}

		public Item DropItem(Item item, Vector3 position)
		{
			if (item == null)
			{
				return null;
			}
			Item item2 = UnityEngine.Object.Instantiate(item, position, Quaternion.identity);
			item2.name = item.name;
			item2.transform.parent = Map.Instance.transform;
			item2.Initialize();
			Singleton<Service>.Instance.gearManager.SpawnFx(item2);
			return item2;
		}

		public Quintessence DropQuintessence(Quintessence quintessence)
		{
			return DropQuintessence(quintessence, player.transform.position);
		}

		public Quintessence DropQuintessence(Quintessence quintessence, Vector3 position)
		{
			if (quintessence == null)
			{
				return null;
			}
			Quintessence quintessence2 = UnityEngine.Object.Instantiate(quintessence, position, Quaternion.identity);
			quintessence2.name = quintessence.name;
			quintessence2.transform.parent = Map.Instance.transform;
			quintessence2.Initialize();
			Singleton<Service>.Instance.gearManager.SpawnFx(quintessence2);
			return quintessence2;
		}

		public Gear DropGear(Gear gear)
		{
			return DropGear(gear, player.transform.position);
		}

		public Gear DropGear(Gear gear, Vector3 position)
		{
			Gear gear2 = UnityEngine.Object.Instantiate(gear, position, Quaternion.identity);
			gear2.name = gear.name;
			gear2.transform.parent = Map.Instance.transform;
			gear2.Initialize();
			return gear2;
		}

		public Potion DropPotion(Potion potion)
		{
			return DropPotion(potion, player.transform.position);
		}

		public Potion DropPotion(Potion potion, Vector3 position)
		{
			Potion potion2 = UnityEngine.Object.Instantiate(potion, position, Quaternion.identity);
			potion2.name = potion.name;
			potion2.transform.parent = Map.Instance.transform;
			potion2.Initialize();
			return potion2;
		}

		public void Load(Chapter.Type chapter)
		{
			Load((int)chapter);
		}

		private void Load(int chapterIndex)
		{
			if (chapterIndex == 2)
			{
				GameData.Generic.tutorial.Start();
			}
			if (chapterIndex == 1 || chapterIndex == 2)
			{
				GameData.Currency.gold.Reset();
				GameData.Currency.bone.Reset();
				GameData.Currency.darkQuartz.income = 0;
				GameData.Progress.ResetNonpermaAll();
				Prophecy.ResetAll();
			}
			base.transform.Empty();
			_chapters[_currentChapterIndex].Clear();
			_currentChapterIndex = chapterIndex;
			if (Scene<GameBase>.instance == null)
			{
				SceneManager.LoadSceneAsync("gameBase", LoadSceneMode.Single).completed += delegate
				{
					_chapters[_currentChapterIndex].Enter();
				};
			}
			else
			{
				_chapters[_currentChapterIndex].Enter();
			}
			this.onChapterLoaded?.Invoke();
		}

		private void Reset()
		{
			PoolObject.DespawnAllOrphans();
			Scene<GameBase>.instance.cameraController.shake.Clear();
			Singleton<Service>.Instance.controllerVibation.Stop();
			GameBase instance = Scene<GameBase>.instance;
			instance.gameFadeInOut.Deactivate();
			instance.cameraController.Zoom(1f);
			instance.cameraController.StopTrack();
			instance.uiManager.curseOfLightVignette.Hide();
			if (player != null)
			{
				UnityEngine.Object.Destroy(player.gameObject);
				player = null;
			}
		}

		public void Unload()
		{
			Reset();
			base.transform.Empty();
			_chapters[_currentChapterIndex].Clear();
		}

		public void ResetGame()
		{
			Reset();
			Load(GameData.Generic.tutorial.isPlayed() ? Chapter.Type.Castle : Chapter.Type.Tutorial);
		}

		public void ResetGame(Chapter.Type chapter)
		{
			Reset();
			Load(chapter);
		}

		public void SpawnPlayerIfNotExist()
		{
			_003C_003Ec__DisplayClass65_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass65_0();
			CS_0024_003C_003E8__locals0._003C_003E4__this = this;
			CS_0024_003C_003E8__locals0.gameBase = Scene<GameBase>.instance;
			if (player == null)
			{
				player = UnityEngine.Object.Instantiate(Resource.instance.player, CS_0024_003C_003E8__locals0.gameBase.transform);
				CS_0024_003C_003E8__locals0.gameBase.uiManager.headupDisplay.Initialize(player);
				CS_0024_003C_003E8__locals0.gameBase.cameraController.StartTrack(player.transform);
				CS_0024_003C_003E8__locals0.gameBase.minimapCameraController.StartTrack(player.transform);
				player.health.onDied += delegate
				{
					GameData.Progress.deaths++;
					PlayerDieHeadParts component = Resource.instance.playerDieHeadParts.parts.poolObject.Spawn().GetComponent<PlayerDieHeadParts>();
					component.transform.parent = Map.Instance.transform;
					DroppedParts parts = component.parts;
					component.transform.position = CS_0024_003C_003E8__locals0._003C_003E4__this.player.transform.position;
					component.sprite = CS_0024_003C_003E8__locals0._003C_003E4__this.player.playerComponents.inventory.weapon.polymorphOrCurrent.icon;
					parts.Initialize(CS_0024_003C_003E8__locals0._003C_003E4__this.player.movement.push);
					CS_0024_003C_003E8__locals0.gameBase.cameraController.StartTrack(component.transform);
					CS_0024_003C_003E8__locals0.gameBase.cameraController.Zoom(0.8f);
					CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003CSpawnPlayerIfNotExist_003Eg__CProcessPlayerDie_007C0());
					CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003CSpawnPlayerIfNotExist_003Eg__CAttachTimeScale_007C1());
				};
			}
			UnityEngine.Object.DontDestroyOnLoad(player);
		}

		public void LoadNextStage()
		{
			currentChapter.NextStage();
		}

		public void LoadNextMap()
		{
			Chapter chapter = _chapters[_currentChapterIndex];
			_oldMap = chapter.map;
			if (!chapter.Next())
			{
				chapter.Clear();
				Resources.UnloadUnusedAssets();
				Load(_currentChapterIndex + 1);
			}
		}

		public void LoadNextMap(PathNode pathNode)
		{
			Chapter chapter = _chapters[_currentChapterIndex];
			_oldMap = chapter.map;
			if (!chapter.Next(pathNode))
			{
				chapter.Clear();
				Load(_currentChapterIndex + 1);
			}
		}

		public void InvokeOnMapChanged()
		{
			this.onMapLoaded?.Invoke();
		}

		public void InvokeOnMapChangedAndFadeIn(Map newMap)
		{
			this.onMapLoadedAndFadedIn?.Invoke();
			this.onMapChangedAndFadedIn?.Invoke(_oldMap, newMap);
		}
	}
}
