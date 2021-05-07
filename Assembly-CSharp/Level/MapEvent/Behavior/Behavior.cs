using System;
using UnityEditor;
using UnityEngine;

namespace Level.MapEvent.Behavior
{
	public abstract class Behavior : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Behavior.types)
			{
			}
		}

		public static readonly Type[] types = new Type[10]
		{
			typeof(ChangeCameraZone),
			typeof(ControlTrap),
			typeof(MovePosition),
			typeof(RunOperation),
			typeof(SetActiveGameObject),
			typeof(UpdateLight),
			typeof(SpawnWave),
			typeof(StartEvent),
			typeof(UpdateSpriteColor),
			typeof(UpdateTilemapColor)
		};

		public abstract void Run();
	}
}
