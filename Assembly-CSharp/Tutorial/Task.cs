using System;
using System.Collections;
using Characters;
using Characters.Controllers;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Tutorial
{
	public abstract class Task : MonoBehaviour
	{
		[Serializable]
		private class Option
		{
			internal enum LookingDirectionOption
			{
				None,
				Right,
				Left
			}

			[SerializeField]
			internal bool letterBox;

			[SerializeField]
			internal bool blockInput;

			[SerializeField]
			internal Transform reservedPosition;

			[SerializeField]
			internal LookingDirectionOption finalDirection;
		}

		private enum StartCondition
		{
			TimeOutAfterSpawn,
			RemainMonsters,
			EnterZone
		}

		[SerializeField]
		private Message[] messages;

		[SerializeField]
		private Option _option;

		private Character _player;

		private void Start()
		{
			_player = Singleton<Service>.Instance.levelManager.player;
		}

		public IEnumerator Play()
		{
			yield return null;
		}

		private IEnumerator ProcessOption()
		{
			if (_option.blockInput)
			{
				PlayerInput.blocked.Attach(this);
			}
			if (_option.letterBox)
			{
				Scene<GameBase>.instance.uiManager.letterBox.Appear(1.7f);
			}
			bool flag = _option.reservedPosition != null;
			if (_option.finalDirection != 0)
			{
				_player.lookingDirection = ((_option.finalDirection != Option.LookingDirectionOption.Right) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
			}
			yield return null;
		}
	}
}
