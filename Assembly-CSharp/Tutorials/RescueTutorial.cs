using System.Collections;
using Characters;
using FX;
using Level;
using Scenes;
using Singletons;
using UnityEngine;

namespace Tutorials
{
	public class RescueTutorial : Tutorial
	{
		[SerializeField]
		private EnemyWave _wave;

		[SerializeField]
		private Cage _cage;

		[SerializeField]
		private Transform _conversationPoint;

		[SerializeField]
		private Witch _witch;

		[SerializeField]
		private SoundInfo _soundInfo;

		[SerializeField]
		private Animator _cageNextSkeleton;

		[SerializeField]
		private LineText _cageNextSkeletonLineText;

		[SerializeField]
		private TextMessageInfo _cageNextSkeletonTextMessage;

		protected override void Start()
		{
			base.Start();
			_wave.onClear += delegate
			{
				TextMessageInfo.Message message = _cageNextSkeletonTextMessage.messages[0];
				_cage.collider.enabled = true;
			};
			_cage.onDestroyed += delegate
			{
				_cageNextSkeleton.Play("Dead");
				TextMessageInfo.Message message2 = _cageNextSkeletonTextMessage.messages[1];
				StartCoroutine(_003CStart_003Eg__EscapeCage_007C8_2());
			};
		}

		protected override IEnumerator Process()
		{
			yield return MoveTo(_conversationPoint.position);
			_player.lookingDirection = Character.LookingDirection.Right;
			for (int i = 0; i < _messageInfo.messages.Length - 1; i++)
			{
				Scene<GameBase>.instance.uiManager.npcConversation.Done();
			}
			PersistentSingleton<SoundManager>.Instance.PlaySound(_soundInfo, _witch.transform.position);
			yield return _witch.TurnIntoCat();
			_witch.gameObject.SetActive(false);
			TextMessageInfo.Message message = _messageInfo.messages[_messageInfo.messages.Length - 1];
			Object.Destroy(_lineText.gameObject);
			Deactivate();
		}
	}
}
