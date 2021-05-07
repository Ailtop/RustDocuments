using System;
using UnityEditor;
using UnityEngine;

namespace CutScenes
{
	public abstract class State : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, State.types)
			{
			}
		}

		protected static object key = new object();

		public static readonly Type[] types = new Type[3]
		{
			typeof(PlayerInputBlock),
			typeof(PlayerMovementBlock),
			typeof(CharacterInvulnerable)
		};

		public abstract void Attach();

		public abstract void Detach();
	}
}
