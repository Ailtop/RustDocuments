using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;
using Data;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	public abstract class FieldNpc : InteractiveObject
	{
		protected enum Phase
		{
			Initial,
			Greeted,
			Gave
		}

		protected static readonly int _idleHash = Animator.StringToHash("Idle");

		protected static readonly int _idleCageHash = Animator.StringToHash("Idle_Cage");

		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private Collider2D _interactRange;

		[SerializeField]
		private PressingButton _releasePressingButton;

		[SerializeField]
		private GameObject _talkUiObject;

		protected Phase _phase;

		protected NpcConversation _npcConversation;

		private bool _release;

		public bool encountered
		{
			get
			{
				return GameData.Progress.fieldNpcEncountered.GetData(_type);
			}
			set
			{
				GameData.Progress.fieldNpcEncountered.SetData(_type, value);
			}
		}

		protected abstract NpcType _type { get; }

		protected string _displayName => Lingua.GetLocalizedString($"npc/{_type}/name");

		protected string[] _greeting => Lingua.GetLocalizedStringArray($"npc/{_type}/greeting");

		protected string[] _regreeting => Lingua.GetLocalizedStringArray($"npc/{_type}/regreeting");

		protected string[] _confirmed => Lingua.GetLocalizedStringArray($"npc/{_type}/confirmed");

		protected string[] _noMoney => Lingua.GetLocalizedStringArray($"npc/{_type}/noMoney");

		protected string[] _chat => Lingua.GetLocalizedStringArrays($"npc/{_type}/chat").Random();

		protected override void Awake()
		{
			base.Awake();
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			_npcConversation.name = _displayName;
			_npcConversation.skippable = true;
			_npcConversation.portrait = _portrait;
			_animator.Play(_idleCageHash, 0, 0f);
			_interactRange.enabled = false;
		}

		public void SetCage(Cage cage)
		{
			cage.onDestroyed += delegate
			{
				_interactRange.enabled = true;
			};
		}

		public void Flip()
		{
			_animator.transform.localScale = new Vector3(-1f, 1f, 1f);
		}

		private void Release()
		{
			encountered = true;
			_release = true;
			_animator.Play(_idleHash, 0, 0f);
			_uiObject.SetActive(false);
			_talkUiObject.SetActive(true);
			_uiObject = _talkUiObject;
		}

		private void OnEnable()
		{
			Singleton<Service>.Instance.levelManager.player.health.onTookDamage += StopConversation;
		}

		private void OnDisable()
		{
			if (!Service.quitting)
			{
				Singleton<Service>.Instance.levelManager.player.health.onTookDamage -= StopConversation;
				Close();
			}
		}

		private void StopConversation([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_releasePressingButton.StopPressing();
			StopAllCoroutines();
			Close();
		}

		protected void Close()
		{
			_npcConversation.portrait = null;
			_npcConversation.visible = false;
			LetterBox.instance.Disappear();
		}

		public override void InteractWithByPressing(Character character)
		{
			Release();
			Interact(character);
		}

		public override void InteractWith(Character character)
		{
			if (_release)
			{
				Interact(character);
			}
		}

		protected virtual void Interact(Character character)
		{
			_npcConversation.name = _displayName;
			_npcConversation.portrait = _portrait;
		}

		protected IEnumerator CGreeting()
		{
			_npcConversation.skippable = true;
			yield return _npcConversation.CConversation(_greeting);
		}

		protected IEnumerator CChat()
		{
			yield return LetterBox.instance.CAppear();
			_npcConversation.skippable = true;
			yield return _npcConversation.CConversation(_chat);
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
