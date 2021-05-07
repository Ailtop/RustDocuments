using System.Collections;
using Characters.Controllers;
using Scenes;
using UnityEngine;

namespace Tutorial
{
	public class DeadCastleWitch : NPC
	{
		[SerializeField]
		private SpriteRenderer _messageBox;

		[SerializeField]
		private int[] _duration;

		[SerializeField]
		private Sprite[] _messages;

		private bool _done;

		protected override void Activate()
		{
			if (!_done)
			{
				_done = true;
				PlayerInput.blocked.Attach(this);
				Scene<GameBase>.instance.uiManager.letterBox.Appear(1.5f);
				StartCoroutine(Converse());
			}
		}

		protected override void Deactivate()
		{
			PlayerInput.blocked.Detach(this);
			Scene<GameBase>.instance.uiManager.letterBox.Disappear(0.5f);
			_messageBox.sprite = null;
		}

		private IEnumerator Converse()
		{
			yield return Chronometer.global.WaitForSeconds(3f);
			_messageBox.sprite = _messages[0];
			yield return Skip(_duration[0]);
			_messageBox.sprite = _messages[1];
			yield return Skip(_duration[1]);
			_messageBox.sprite = _messages[2];
			yield return Skip(_duration[2]);
			PlayerInput.blocked.Detach(this);
			Scene<GameBase>.instance.uiManager.letterBox.Disappear(0.5f);
			_messageBox.sprite = null;
		}

		private IEnumerator Skip(float duration)
		{
			float elapsed = 0f;
			while (!Input.anyKeyDown)
			{
				yield return null;
				elapsed += Chronometer.global.deltaTime;
				if (elapsed > duration)
				{
					break;
				}
			}
		}
	}
}
