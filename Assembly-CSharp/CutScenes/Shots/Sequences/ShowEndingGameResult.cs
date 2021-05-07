using System.Collections;
using Scenes;
using UserInput;

namespace CutScenes.Shots.Sequences
{
	public class ShowEndingGameResult : Sequence
	{
		public override IEnumerator CRun()
		{
			GameBase gameBase = Scene<GameBase>.instance;
			gameBase.uiManager.endingGameResult.Show();
			while (!gameBase.uiManager.endingGameResult.animationFinished || (!KeyMapper.Map.Attack.WasPressed && !KeyMapper.Map.Submit.WasPressed))
			{
				yield return null;
			}
			Hide();
		}

		private void Hide()
		{
			Scene<GameBase>.instance.uiManager.endingGameResult.Hide();
		}

		private void OnDisable()
		{
			Hide();
		}
	}
}
