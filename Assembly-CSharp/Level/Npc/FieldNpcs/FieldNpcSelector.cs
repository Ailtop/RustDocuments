using System;
using System.Linq;
using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	public class FieldNpcSelector : MonoBehaviour
	{
		[Serializable]
		private class Npcs : ReorderableArray<Npcs.Property>
		{
			[Serializable]
			internal class Property
			{
				[SerializeField]
				private float _weight;

				[SerializeField]
				private FieldNpc _npc;

				public float weight => _weight;

				public FieldNpc npc => _npc;
			}
		}

		[SerializeField]
		private Npcs _npcs;

		private Cage _cage;

		private void Awake()
		{
			_cage = GetComponentInParent<Cage>();
			FieldNpc fieldNpc = null;
			Npcs.Property[] values = _npcs.values;
			float num = UnityEngine.Random.Range(0f, values.Sum((Npcs.Property a) => a.weight));
			foreach (Npcs.Property property in values)
			{
				num -= property.weight;
				if (num <= 0f && (property.npc == null || !property.npc.encountered))
				{
					fieldNpc = property.npc;
					break;
				}
			}
			if (fieldNpc == null)
			{
				_cage.Destroy();
				_cage.gameObject.SetActive(false);
			}
			else
			{
				fieldNpc = UnityEngine.Object.Instantiate(fieldNpc, base.transform);
				fieldNpc.transform.position = base.transform.position;
				fieldNpc.SetCage(_cage);
			}
			UnityEngine.Object.Destroy(this);
		}
	}
}
