using System;
using UnityEditor;
using UnityEngine;

namespace Level.MapEvent.Condition
{
	public abstract class Condition : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Condition.types)
			{
			}
		}

		public static readonly Type[] types = new Type[4]
		{
			typeof(Always),
			typeof(EnterZone),
			typeof(PropDestroyed),
			typeof(WaveEvent)
		};

		[SerializeField]
		public event Action onSatisfy;

		protected void Run()
		{
			this.onSatisfy?.Invoke();
		}
	}
}
