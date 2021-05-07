using System.Collections;
using Characters;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Tutorials
{
	public abstract class Tutorial : MonoBehaviour
	{
		protected enum State
		{
			Wait,
			Progress,
			Done
		}

		protected enum StartCondition
		{
			EnterZone,
			Manually
		}

		[SerializeField]
		protected TextMessageInfo _messageInfo;

		[SerializeField]
		protected LineText _lineText;

		protected Character _player;

		protected State _state;

		[SerializeField]
		protected StartCondition _startCondition;

		protected NpcConversation _npcConversation;

		protected string _displayNameKey;

		protected State state
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
			}
		}

		protected virtual void Start()
		{
			_player = Singleton<Service>.Instance.levelManager.player;
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (_startCondition == StartCondition.EnterZone)
			{
				Character component = collision.GetComponent<Character>();
				if (!(component == null) && !(component != _player) && _state == State.Wait)
				{
					Activate();
				}
			}
		}

		protected abstract IEnumerator Process();

		public virtual void Activate()
		{
			_state = State.Progress;
			LetterBox.instance.Appear();
			StartCoroutine(Process());
		}

		public virtual void Deactivate()
		{
			state = State.Done;
			_npcConversation.Done();
			LetterBox.instance.Disappear();
		}

		protected IEnumerator CDeactivate()
		{
			yield return Scene<GameBase>.instance.uiManager.letterBox.CDisappear();
		}

		protected virtual IEnumerator Converse(float delay = 2f)
		{
			yield return null;
		}

		protected virtual IEnumerator CConversation(TextMessageInfo message)
		{
			string nameKey = message.nameKey;
			string messageKey = message.messageKey;
			_npcConversation.name = Lingua.GetLocalizedString(nameKey);
			_npcConversation.portrait = null;
			_npcConversation.skippable = true;
			yield return _npcConversation.CConversation(Lingua.GetLocalizedString(messageKey));
		}

		protected IEnumerator MoveTo(Vector3 destination)
		{
			while (true)
			{
				float num = destination.x - _player.transform.position.x;
				if (Mathf.Abs(num) < 0.1f)
				{
					break;
				}
				Vector2 move = ((num > 0f) ? Vector2.right : Vector2.left);
				_player.movement.move = move;
				yield return null;
			}
			_player.transform.position = destination;
		}

		protected virtual void OnDisable()
		{
			_npcConversation?.Done();
			LetterBox.instance.Disappear();
		}
	}
}
