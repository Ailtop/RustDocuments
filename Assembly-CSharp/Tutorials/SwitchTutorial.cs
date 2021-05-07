using System.Collections;
using Characters;
using Level;
using Scenes;
using UnityEngine;

namespace Tutorials
{
	public class SwitchTutorial : Tutorial
	{
		[SerializeField]
		private DeterminedGrave _grave;

		[SerializeField]
		private Transform _conversationPoint;

		[SerializeField]
		private Transform _interactiveGravePoint;

		[SerializeField]
		private Transform _getHeadPoint;

		protected override IEnumerator Process()
		{
			yield return MoveTo(_conversationPoint.position);
			_player.lookingDirection = Character.LookingDirection.Right;
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
			yield return MoveTo(_interactiveGravePoint.position);
			yield return Chronometer.global.WaitForSeconds(0.3f);
			_grave.InteractWith(_player);
			yield return Chronometer.global.WaitForSeconds(1.7f);
			yield return MoveTo(_getHeadPoint.position);
			yield return Chronometer.global.WaitForSeconds(0.3f);
			_grave.droppedWeapon.dropped.InteractWith(_player);
			Deactivate();
		}
	}
}
