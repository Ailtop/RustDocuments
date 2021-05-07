using System;
using Runnables.Cost;
using UnityEditor;
using UnityEngine;

namespace Runnables
{
	public abstract class CurrencyAmount : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, CurrencyAmount.types)
			{
			}
		}

		public static readonly Type[] types = new Type[2]
		{
			typeof(CostEvent),
			typeof(Constant)
		};

		public abstract int GetAmount();
	}
}
