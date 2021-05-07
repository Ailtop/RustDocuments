using System;
using UnityEditor;
using UnityEngine;

namespace UI.Pause
{
	public abstract class PauseEvent : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, PauseEvent.types)
			{
			}
		}

		public static Type[] types = new Type[4]
		{
			typeof(PauseMenuPopUp),
			typeof(StorySkip),
			typeof(CreditExit),
			typeof(Empty)
		};

		public abstract void Invoke();
	}
}
