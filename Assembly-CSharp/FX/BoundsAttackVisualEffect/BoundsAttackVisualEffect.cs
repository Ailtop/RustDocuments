using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;
using UnityEditor;
using UnityEngine;

namespace FX.BoundsAttackVisualEffect
{
	public abstract class BoundsAttackVisualEffect : VisualEffect
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, BoundsAttackVisualEffect.types)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<BoundsAttackVisualEffect>
		{
			public void Spawn(Character owner, Bounds bounds, [In][IsReadOnly] ref Damage damage, ITarget target)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].Spawn(owner, bounds, ref damage, target);
				}
			}
		}

		public static readonly Type[] types = new Type[1] { typeof(RandomWithinIntersect) };

		public abstract void Spawn(Character owner, Bounds bounds, [In][IsReadOnly] ref Damage damage, ITarget target);
	}
}
