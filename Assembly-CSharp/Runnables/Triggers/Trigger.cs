using System;
using UnityEditor;
using UnityEngine;

namespace Runnables.Triggers
{
	public abstract class Trigger : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Trigger.types)
			{
			}
		}

		public static readonly Type[] types = new Type[17]
		{
			typeof(Always),
			typeof(ActivatedSkulStory),
			typeof(ByPosition),
			typeof(CageOnDestroyed),
			typeof(CharacterDied),
			typeof(EnterZone),
			typeof(EqualsState),
			typeof(HasCurrency),
			typeof(Inverter),
			typeof(MapRewardActivated),
			typeof(StoppedEnemyContainer),
			typeof(Sequence),
			typeof(Timer),
			typeof(WaveOnClear),
			typeof(PlayedCutScene),
			typeof(PlayedSkulStory),
			typeof(PlayedTutorial)
		};

		protected abstract bool Check();

		public bool isSatisfied()
		{
			return Check();
		}
	}
}
