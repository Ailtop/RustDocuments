using System;
using System.Collections;
using System.Linq;
using Characters.Operations.Fx;
using FX;
using Level;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class Chapter1Script : MonoBehaviour
	{
		[Serializable]
		private class DarkQuartzPossibility
		{
			[Serializable]
			internal class Reorderable : ReorderableArray<DarkQuartzPossibility>
			{
				public int Take()
				{
					if (values.Length == 0)
					{
						return 0;
					}
					int max = values.Sum((DarkQuartzPossibility v) => v.weight);
					int num = UnityEngine.Random.Range(0, max) + 1;
					for (int i = 0; i < values.Length; i++)
					{
						num -= values[i].weight;
						if (num <= 0)
						{
							return (int)values[i].amount.value;
						}
					}
					return 0;
				}
			}

			[Range(0f, 100f)]
			public int weight;

			public CustomFloat amount;
		}

		private class Intro
		{
			private Chapter1Script _script;

			public Intro(Chapter1Script script)
			{
				_script = script;
			}

			public void StartIntro()
			{
				_script.HideHUD();
				LetterBox.instance.Appear();
				Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
			}

			public void EndIntro()
			{
				_script.ShowHUD();
				LetterBox.instance.Disappear();
				Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.Open(BossHealthbarController.Type.Chpater1_Phase1, _script.yggdrasill.character);
				PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(_script.bacgkroundMusicInfo);
			}
		}

		private class InGame
		{
			private IEnumerator CCombatPhase1()
			{
				yield return null;
			}

			private IEnumerator CCombatPhase2()
			{
				yield return null;
			}
		}

		private class Outro
		{
			private Chapter1Script _script;

			public Outro(Chapter1Script script)
			{
				_script = script;
			}

			public void StartOutro()
			{
				_script.flash.Run(_script.yggdrasill.character);
				Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
				_script.HideHUD();
				Scene<GameBase>.instance.cameraController.StartTrack(_script.outroCameraPoint);
				PersistentSingleton<SoundManager>.Instance.StopBackGroundMusic();
			}

			public void EndOutro()
			{
				Map.Instance.cameraZone = null;
				Map.Instance.SetCameraZoneOrDefault();
				try
				{
					_script.SpawnChestEffect.Run(_script.yggdrasill.character);
					_script.chest.gameObject.SetActive(true);
					Singleton<Service>.Instance.levelManager.DropDarkQuartz(_script.darkQuartzes.Take(), 30, _script.chest.transform.position, Vector2.up);
					_script.block.Deactivate();
				}
				catch (Exception ex)
				{
					Debug.Log(ex.ToString());
					_script.block.Deactivate();
				}
				_script.ShowHUD();
				Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
			}
		}

		[SerializeField]
		private MusicInfo bacgkroundMusicInfo;

		[SerializeField]
		private YggdrasillElderEntAI yggdrasill;

		[Header("Outro")]
		[SerializeField]
		private Characters.Operations.Fx.ScreenFlash flash;

		[SerializeField]
		private SpawnEffect SpawnChestEffect;

		[SerializeField]
		private Block block;

		[SerializeField]
		private BossChest chest;

		[Header("Camera")]
		[SerializeField]
		private Transform outroCameraPoint;

		[Header("Reward")]
		[SerializeField]
		private DarkQuartzPossibility.Reorderable darkQuartzes;

		private static readonly string _nameKey = "CutScene/name/ElderEnt";

		private static readonly string _textKey = "CutScene/Ch1BossOutro/ElderEnt/0";

		private Intro _intro;

		private InGame _inGame;

		private Outro _outro;

		private void Awake()
		{
			_intro = new Intro(this);
			_inGame = new InGame();
			_outro = new Outro(this);
			StartIntro();
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Scene<GameBase>.instance.uiManager.npcConversation.Done();
				LetterBox.instance.Disappear();
				Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
				Scene<GameBase>.instance.uiManager.headupDisplay.visible = true;
				if (!(Scene<GameBase>.instance.cameraController == null) && !(Singleton<Service>.Instance.levelManager.player == null))
				{
					Scene<GameBase>.instance.cameraController.StartTrack(Singleton<Service>.Instance.levelManager.player.transform);
				}
			}
		}

		public void StartIntro()
		{
		}

		public void EndIntro()
		{
		}

		public void StartOutro()
		{
		}

		public void EndOutro()
		{
		}

		public void HideHUD()
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = false;
		}

		public void ShowHUD()
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = true;
			if (!(Scene<GameBase>.instance.cameraController == null) && !(Singleton<Service>.Instance.levelManager.player == null))
			{
				Scene<GameBase>.instance.cameraController.StartTrack(Singleton<Service>.Instance.levelManager.player.transform);
			}
		}

		private IEnumerator CutScene()
		{
			Singleton<Service>.Instance.fadeInOut.SetFadeColor(Color.black);
			yield return Singleton<Service>.Instance.fadeInOut.CFadeOut();
			LetterBox.instance.Appear();
			yield return Chronometer.global.WaitForSeconds(1.5f);
			yield return Singleton<Service>.Instance.fadeInOut.CFadeIn();
			yield return Chronometer.global.WaitForSeconds(1.5f);
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			npcConversation.skippable = true;
			yield return npcConversation.CConversation(Lingua.GetLocalizedStringArray(_textKey));
			LetterBox.instance.Disappear();
			_outro.EndOutro();
		}
	}
}
