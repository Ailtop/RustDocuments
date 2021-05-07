using System;
using System.Linq;
using CutScenes.Shots.Events;
using Runnables;
using UnityEditor;

namespace CutScenes.Shots
{
	public abstract class Event : Runnable
	{
		public new class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Runnable.types.Concat(Event.types).ToArray())
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<Runnable>
		{
			public void Run()
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					base.components[i].Run();
				}
			}
		}

		public new static readonly Type[] types = new Type[17]
		{
			typeof(Attacher),
			typeof(CancelAction),
			typeof(CameraMoveTo),
			typeof(CameraMoveToCharacter),
			typeof(ControlLetterBox),
			typeof(ControlUI),
			typeof(DestroyObject),
			typeof(Knockback),
			typeof(PlayAnimation),
			typeof(ResetGame),
			typeof(RenderEndingCut),
			typeof(RunSequences),
			typeof(SaveCutSceneData),
			typeof(SaveRescueNPCData),
			typeof(SaveTutorialData),
			typeof(SetFadeColor),
			typeof(PlayAdventurerMusic)
		};
	}
}
