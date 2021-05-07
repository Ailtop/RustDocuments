using System;
using Characters;
using Characters.AI.Pope;
using CutScenes.Objects.Chapter4;
using Level.Pope;
using Runnables;
using Scenes;
using UnityEngine;
using UnityEngine.Events;

namespace Level.Chapter4
{
	public class Scenario : MonoBehaviour
	{
		[SerializeField]
		private UnityEvent _onChestOpend;

		[SerializeField]
		private GameObject _gate;

		[SerializeField]
		private BossChest _chest;

		[SerializeField]
		private PopeAI popeAI;

		[Header("Phase 1")]
		[SerializeField]
		private Barrier _barrier;

		[SerializeField]
		private FanaticFactory fanaticFactory;

		[SerializeField]
		private Character _darkCrystalLeft;

		[SerializeField]
		[Space]
		private Character _darkCrystalRight;

		[Header("Phase 2")]
		[SerializeField]
		private PlatformContainer _platformContainer;

		[SerializeField]
		private Chair _chair;

		[SerializeField]
		private Fire _fire;

		[SerializeField]
		private Cleansing _cleansing;

		public event Action OnPhase1Start;

		public event Action OnPhase1End;

		private void Start()
		{
			_darkCrystalLeft.health.onDied += TryStart2Phase_Left;
			_darkCrystalRight.health.onDied += TryStart2Phase_Right;
			_chest.OnOpen += delegate
			{
				_gate.SetActive(true);
				_onChestOpend?.Invoke();
			};
		}

		private void TryStart2Phase_Left()
		{
			_cleansing.Run();
			_darkCrystalLeft.health.onDied -= TryStart2Phase_Left;
			if (!_darkCrystalRight.health.dead)
			{
				_barrier.Crack();
				return;
			}
			this.OnPhase1End?.Invoke();
			StopDoing();
		}

		private void TryStart2Phase_Right()
		{
			_cleansing.Run();
			_darkCrystalRight.health.onDied -= TryStart2Phase_Right;
			if (!_darkCrystalLeft.health.dead)
			{
				_barrier.Crack();
				return;
			}
			this.OnPhase1End?.Invoke();
			StopDoing();
		}

		private void StopDoing()
		{
			popeAI.StopAllCoroutinesWithBehaviour();
			foreach (Character allEnemy in Map.Instance.waveContainer.GetAllEnemies())
			{
				if (!(popeAI.character == allEnemy))
				{
					allEnemy.health.Kill();
				}
			}
			fanaticFactory.StopToSummon();
		}

		public void Start1Phase()
		{
			this.OnPhase1Start?.Invoke();
			popeAI.StartCombat();
			fanaticFactory.StartToSummon();
		}

		public void Start2Phase()
		{
			_platformContainer.Appear();
			_chair.Hide();
			_fire.Appear();
			ZoomOut();
			popeAI.NextPhase();
		}

		private void ZoomOut()
		{
			Scene<GameBase>.instance.cameraController.Zoom(1.3f);
		}

		private void ZoomIn()
		{
			Scene<GameBase>.instance.cameraController.Zoom(1f, 10f);
		}

		private void OnDestroy()
		{
			ZoomIn();
		}
	}
}
