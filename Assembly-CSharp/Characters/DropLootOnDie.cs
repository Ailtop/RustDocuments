using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Gear;
using Characters.Movements;
using Level;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters
{
	public class DropLootOnDie : MonoBehaviour
	{
		[Serializable]
		public class DarkQuartzPossibility
		{
			[Serializable]
			public class Reorderable : ReorderableArray<DarkQuartzPossibility>
			{
				public int Take()
				{
					if (values.Length == 0)
					{
						return 0;
					}
					int max = values.Sum((DarkQuartzPossibility v) => v.weight);
					int num = UnityEngine.Random.Range(0, max) + 1;
					for (int i = 0; i < values.Length; i++)
					{
						num -= values[i].weight;
						if (num <= 0)
						{
							return (int)values[i].amount.value;
						}
					}
					return 0;
				}

				public float GetAverage()
				{
					float num = values.Sum((DarkQuartzPossibility v) => v.weight);
					float num2 = 0f;
					DarkQuartzPossibility[] array = values;
					foreach (DarkQuartzPossibility darkQuartzPossibility in array)
					{
						num2 += (float)((int)darkQuartzPossibility.amount.value * darkQuartzPossibility.weight) / num;
					}
					return num2;
				}
			}

			[Range(0f, 100f)]
			public int weight;

			public CustomFloat amount;
		}

		[Serializable]
		private class GearInfo
		{
			[SerializeField]
			private Characters.Gear.Gear _gear;

			[SerializeField]
			[Range(1f, 100f)]
			private int _weight;
		}

		private const float _droppedGearHorizontalInterval = 1.5f;

		private const float _droppedGearHorizontalSpeed = 2f;

		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[FormerlySerializedAs("_amount")]
		private int _gold;

		[SerializeField]
		private DarkQuartzPossibility.Reorderable _darkQuartzes;

		[SerializeField]
		private int _count;

		[SerializeField]
		private Characters.Gear.Gear _gear;

		[SerializeField]
		[Range(0f, 100f)]
		private int _gearChance;

		[SerializeField]
		private PotionPossibilities _potionPossibilities;

		public int gold => _gold;

		public DarkQuartzPossibility.Reorderable darkQuartzes => _darkQuartzes;

		private void Awake()
		{
			_character.health.onDie += OnDie;
		}

		private void OnDie()
		{
			if (!_character.health.dead)
			{
				LevelManager levelManager = Singleton<Service>.Instance.levelManager;
				Push push = _character.movement?.push;
				Vector2 force = Vector2.zero;
				if (push != null && !push.expired)
				{
					force = push.direction * push.totalForce;
				}
				levelManager.DropGold(_gold, _count, base.transform.position, force);
				levelManager.DropDarkQuartz(_darkQuartzes.Take(), base.transform.position, force);
				List<DropMovement> list = new List<DropMovement>();
				Potion potion = _potionPossibilities.Get();
				if (potion != null)
				{
					Potion potion2 = levelManager.DropPotion(potion, base.transform.position);
					list.Add(potion2.dropMovement);
				}
				if (MMMaths.PercentChance(_gearChance))
				{
					Characters.Gear.Gear gear = Singleton<Service>.Instance.levelManager.DropGear(_gear, base.transform.position);
					list.Add(gear.dropped.dropMovement);
				}
				DropMovement.SetMultiDropHorizontalInterval(list);
			}
		}
	}
}
