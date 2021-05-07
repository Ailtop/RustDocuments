using System.Collections;
using Characters;
using FX;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	public class HarpyWarrior : FieldNpc
	{
		[SerializeField]
		private SoundInfo _sound;

		protected override NpcType _type => NpcType.HarpyWarrior;

		private int _bones => Singleton<Service>.Instance.levelManager.currentChapter.currentStage.fieldNpcSettings.harpyWarriorBones;

		protected override void Interact(Character character)
		{
			base.Interact(character);
			switch (_phase)
			{
			case Phase.Initial:
			case Phase.Greeted:
				StartCoroutine(CGiveBones(character));
				break;
			case Phase.Gave:
				StartCoroutine(CChat());
				break;
			}
		}

		private IEnumerator CGiveBones(Character character)
		{
			yield return LetterBox.instance.CAppear();
			yield return CGreeting();
			Singleton<Service>.Instance.levelManager.DropBone(_bones, 10);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sound, base.transform.position);
			_phase = Phase.Gave;
			_npcConversation.skippable = true;
			bool flag2 = (_npcConversation.visible = false);
			yield return flag2;
			LetterBox.instance.Disappear();
		}
	}
}
