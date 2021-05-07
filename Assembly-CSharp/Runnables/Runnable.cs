using System;
using UnityEditor;
using UnityEngine;

namespace Runnables
{
	public abstract class Runnable : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Runnable.types)
			{
			}
		}

		public static readonly Type[] types = new Type[22]
		{
			typeof(Attacher),
			typeof(CharacterSetPositionTo),
			typeof(ChangeCameraZone),
			typeof(ChangeBackground),
			typeof(ClearStatus),
			typeof(ControlUI),
			typeof(ConsumeCurrency),
			typeof(Branch),
			typeof(DestroyObject),
			typeof(DropCurrency),
			typeof(DropCustomGear),
			typeof(DropGear),
			typeof(InvokeUnityEvent),
			typeof(LoadNextMap),
			typeof(PrintDebugLog),
			typeof(RunOperations),
			typeof(SetSoundEffectVolume),
			typeof(ShowStageInfo),
			typeof(TransitTo),
			typeof(TakeHealth),
			typeof(KillAllEnemy),
			typeof(Zoom)
		};

		public abstract void Run();
	}
}
