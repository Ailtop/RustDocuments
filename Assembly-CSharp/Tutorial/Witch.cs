using System.Collections;
using Characters.Actions;
using Characters.Controllers;
using Scenes;
using UnityEngine;

namespace Tutorial
{
	public class Witch : NPC
	{
		[SerializeField]
		private SpriteRenderer _messageBox;

		[SerializeField]
		private int[] _duration;

		[SerializeField]
		private Sprite[] _messages;

		[SerializeField]
		private Transform _effetctSpawnPoint;

		[SerializeField]
		private Action _attack;

		[SerializeField]
		private Action _heal;

		private bool _done;

		protected override void Activate()
		{
			if (!_done)
			{
				_done = true;
				PlayerInput.blocked.Attach(this);
				Scene<GameBase>.instance.uiManager.letterBox.Appear(1f);
				StartCoroutine(Converse());
			}
		}

		protected override void Deactivate()
		{
		}

		private IEnumerator Converse()
		{
			yield return Chronometer.global.WaitForSeconds(2f);
			_effetctSpawnPoint.position = _player.transform.position;
			_messageBox.sprite = _messages[0];
			yield return Chronometer.global.WaitForSeconds(_duration[0]);
			_messageBox.sprite = _messages[1];
			_attack.TryStart();
			Scene<GameBase>.instance.cameraController.Shake(1.3f, 0.5f);
			yield return Chronometer.global.WaitForSeconds(_duration[1]);
			_messageBox.sprite = _messages[2];
			yield return Chronometer.global.WaitForSeconds(_duration[2]);
			_messageBox.sprite = _messages[3];
			yield return Chronometer.global.WaitForSeconds(_duration[3]);
			_messageBox.sprite = _messages[4];
			yield return Chronometer.global.WaitForSeconds(_duration[4]);
			PlayerInput.blocked.Detach(this);
			Scene<GameBase>.instance.uiManager.letterBox.Disappear(0.5f);
			_heal.TryStart();
			_player.health.Heal(9999.0);
		}
	}
}
