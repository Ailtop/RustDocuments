using Characters;
using Scenes;
using UnityEngine;

namespace CutScenes.Shots.Events.Customs
{
	public class OpenVeteranAdventuererHealthBar : Event
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private string _nameKey;

		[SerializeField]
		private string _titleKey;

		public override void Run()
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.OpenVeteranAdventurer(_character, _nameKey, _titleKey);
		}
	}
}
