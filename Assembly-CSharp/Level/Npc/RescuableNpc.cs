using System.Collections;
using Characters;
using Data;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class RescuableNpc : InteractiveObject
	{
		protected static readonly int _idleHash = Animator.StringToHash("Idle");

		protected static readonly int _idleCageHash = Animator.StringToHash("Idle_Cage");

		[SerializeField]
		private NpcType _npcType;

		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private Transform _conversationPosition;

		[SerializeField]
		private Cage _cage;

		[SerializeField]
		private Collider2D _interactRange;

		private NpcConversation _npcConversation;

		private string displayName => Lingua.GetLocalizedString($"npc/{_npcType}/name");

		private string[][] rescuedScripts => Lingua.GetLocalizedStringArrays($"npc/{_npcType}/rescue/rescued");

		private string[][] chatScripts => Lingua.GetLocalizedStringArrays($"npc/{_npcType}/rescue/chat");

		protected override void Awake()
		{
			base.Awake();
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			_npcConversation.name = displayName;
			_npcConversation.skippable = true;
			_npcConversation.portrait = _portrait;
			_animator.Play(_idleCageHash, 0, 0f);
			_interactRange.enabled = false;
			_cage.onDestroyed += delegate
			{
				_interactRange.enabled = true;
				GameData.Progress.SetRescued(_npcType, true);
				_animator.Play(_idleHash, 0, 0f);
				StartCoroutine(CRescueConversation());
			};
		}

		private void OnDisable()
		{
			_npcConversation.portrait = null;
			LetterBox.instance.visible = false;
		}

		public override void InteractWith(Character character)
		{
			StartCoroutine(CChat());
		}

		private IEnumerator CChat()
		{
			LetterBox.instance.Appear();
			yield return _npcConversation.CConversation(chatScripts.Random());
			LetterBox.instance.Disappear();
		}

		private IEnumerator CRescueConversation()
		{
			LetterBox.instance.Appear();
			yield return MoveTo(_conversationPosition.position);
			yield return _npcConversation.CConversation(rescuedScripts.Random());
			LetterBox.instance.Disappear();
		}

		private IEnumerator MoveTo(Vector3 destination)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			player.ForceToLookAt(base.transform.position.x);
			while (true)
			{
				float num = destination.x - player.transform.position.x;
				if (Mathf.Abs(num) < 0.1f)
				{
					break;
				}
				Vector2 move = ((num > 0f) ? Vector2.right : Vector2.left);
				player.movement.move = move;
				yield return null;
			}
			player.ForceToLookAt(Character.LookingDirection.Right);
		}
	}
}
