using Scenes;
using Services;
using UI;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class ControlLetterBox : Event
	{
		private enum Type
		{
			Activate,
			Deactivate
		}

		[SerializeField]
		private Type _type;

		private NpcConversation _npcConversation;

		public override void Run()
		{
			if (_type == Type.Activate)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}

		private void Start()
		{
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
		}

		private void Activate()
		{
			LetterBox.instance.Appear();
		}

		private void Deactivate()
		{
			_npcConversation.Done();
			LetterBox.instance.Disappear();
		}

		public void OnDisable()
		{
			if (!Service.quitting)
			{
				_npcConversation?.Done();
				LetterBox.instance.Disappear();
			}
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				_npcConversation?.Done();
				LetterBox.instance.Disappear();
			}
		}
	}
}
