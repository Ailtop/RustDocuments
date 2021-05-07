using System;
using UnityEditor;
using UnityEngine;

namespace SkulStories
{
	public abstract class Event : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Event.types)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<Event>
		{
			public void Run()
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					base.components[i].Run();
				}
			}
		}

		public static Type[] types = new Type[2]
		{
			typeof(ExecuteRunnable),
			typeof(SaveSkulStoryData)
		};

		public abstract void Run();
	}
}
