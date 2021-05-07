using Characters.Operations;
using Scenes;
using UI;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Chimera
{
	public class Intro : MonoBehaviour
	{
		[Header("Ready")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _readyOperations;

		[Header("Landing")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _landingOperations;

		[Header("FallRocks")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _fallRocksOperations;

		[Header("Explosion")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _explosionOperations;

		[Header("RoarReady")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _roarReadyOperations;

		[Header("Roar")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _roarOperations;

		private void Awake()
		{
			_readyOperations.Initialize();
			_landingOperations.Initialize();
			_explosionOperations.Initialize();
			_fallRocksOperations.Initialize();
			_roarReadyOperations.Initialize();
			_roarOperations.Initialize();
		}

		private void OnDestroy()
		{
			Scene<GameBase>.instance.cameraController.Zoom(1f, float.MaxValue);
		}

		public void Ready(Character character)
		{
			_readyOperations.gameObject.SetActive(true);
			_readyOperations.Run(character);
		}

		public void Landing(Character character, Chapter3Script script)
		{
			script.DisplayBossName();
			_landingOperations.gameObject.SetActive(true);
			_landingOperations.Run(character);
		}

		public void FallingRocks(Character character)
		{
			_fallRocksOperations.gameObject.SetActive(true);
			_fallRocksOperations.Run(character);
		}

		public void Explosion(Character character)
		{
			_explosionOperations.gameObject.SetActive(true);
			_explosionOperations.Run(character);
		}

		public void CameraZoomOut()
		{
			Scene<GameBase>.instance.cameraController.Zoom(1.3f);
		}

		public void RoarReady(Character character)
		{
			_roarReadyOperations.gameObject.SetActive(true);
			_roarReadyOperations.Run(character);
		}

		public void Roar(Character character)
		{
			_roarOperations.gameObject.SetActive(true);
			_roarOperations.Run(character);
		}

		public void LetterBoxOff()
		{
			Scene<GameBase>.instance.uiManager.letterBox.Disappear();
		}

		public void HealthBarOn(Character character)
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.Open(BossHealthbarController.Type.Chapter3_Phase2, character);
		}
	}
}
