using System;
using System.Linq;
using Characters.Gear;
using Services;
using Singletons;
using UnityEngine;

namespace Runnables
{
	public sealed class DropCustomGear : Runnable
	{
		[Serializable]
		private class CustomGears : ReorderableArray<CustomGears.Property>
		{
			[Serializable]
			internal class Property
			{
				[SerializeField]
				private float _weight;

				[SerializeField]
				private Gear _gear;

				public float weight => _weight;

				public Gear gear => _gear;
			}
		}

		[SerializeField]
		private CustomGears _customGears;

		public override void Run()
		{
			Gear gear = Load();
			Singleton<Service>.Instance.levelManager.DropGear(gear, base.transform.position);
		}

		private Gear Load()
		{
			CustomGears.Property[] values = _customGears.values;
			float num = UnityEngine.Random.Range(0f, values.Sum((CustomGears.Property a) => a.weight));
			for (int i = 0; i < values.Length; i++)
			{
				num -= values[i].weight;
				if (num <= 0f)
				{
					return values[i].gear;
				}
			}
			return values[0].gear;
		}
	}
}
