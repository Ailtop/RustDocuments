using System;
using System.Linq;
using UnityEngine;

namespace Level.Altars
{
	public class AltarSelector : MonoBehaviour
	{
		[Serializable]
		private class Altars : ReorderableArray<Altars.Property>
		{
			[Serializable]
			internal class Property
			{
				[SerializeField]
				private float _weight;

				[SerializeField]
				private Prop _altar;

				public float weight => _weight;

				public Prop altar => _altar;
			}
		}

		[SerializeField]
		private Altars _altars;

		private void Awake()
		{
			Prop prop = null;
			Altars.Property[] values = _altars.values;
			float num = UnityEngine.Random.Range(0f, values.Sum((Altars.Property a) => a.weight));
			for (int i = 0; i < values.Length; i++)
			{
				num -= values[i].weight;
				if (num <= 0f)
				{
					prop = values[i].altar;
					break;
				}
			}
			if (prop != null)
			{
				prop = UnityEngine.Object.Instantiate(prop, base.transform.parent);
				prop.transform.position = base.transform.position;
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
