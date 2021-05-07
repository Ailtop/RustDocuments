using UnityEngine;

namespace Characters.Operations.Customs
{
	public class ControlRepeatPlaySound : CharacterOperation
	{
		private enum Type
		{
			Play,
			Stop
		}

		[SerializeField]
		private RepeatPlaySound _repeatPlaySound;

		[SerializeField]
		private Type _type;

		public override void Run(Character owner)
		{
			switch (_type)
			{
			case Type.Play:
				_repeatPlaySound.Play();
				break;
			case Type.Stop:
				_repeatPlaySound.Stop();
				break;
			}
		}
	}
}
