using Rust.Ai.HTN.Sensors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.HTN.Bear.Sensors
{
	[Serializable]
	public class AnimalsInRangeSensor : INpcSensor
	{
		public const int MaxAnimals = 128;

		public static BaseNpc[] QueryResults = new BaseNpc[128];

		public static int QueryResultCount = 0;

		public float TickFrequency
		{
			get;
			set;
		}

		public float LastTickTime
		{
			get;
			set;
		}

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			BearDomain bearDomain = npc.AiDomain as BearDomain;
			if (bearDomain == null || bearDomain.BearContext == null)
			{
				return;
			}
			BaseEntity.Query.EntityTree server = BaseEntity.Query.Server;
			Vector3 position = npc.transform.position;
			float mediumRange = npc.AiDefinition.Engagement.MediumRange;
			BaseEntity[] queryResults = QueryResults;
			QueryResultCount = server.GetInSphere(position, mediumRange, queryResults, delegate(BaseEntity entity)
			{
				BaseNpc baseNpc2 = entity as BaseNpc;
				return (!(baseNpc2 == null) && baseNpc2.isServer && !baseNpc2.IsDestroyed && !(baseNpc2.transform == null) && !baseNpc2.IsDead()) ? true : false;
			});
			List<AnimalInfo> animalsInRange = npc.AiDomain.NpcContext.AnimalsInRange;
			if (QueryResultCount > 0)
			{
				for (int i = 0; i < QueryResultCount; i++)
				{
					BaseNpc baseNpc = QueryResults[i];
					float sqrMagnitude = (baseNpc.transform.position - npc.transform.position).sqrMagnitude;
					if (sqrMagnitude > npc.AiDefinition.Engagement.SqrMediumRange)
					{
						continue;
					}
					bool flag = false;
					for (int j = 0; j < animalsInRange.Count; j++)
					{
						AnimalInfo value = animalsInRange[j];
						if (value.Animal == baseNpc)
						{
							value.Time = time;
							value.SqrDistance = sqrMagnitude;
							animalsInRange[j] = value;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						animalsInRange.Add(new AnimalInfo
						{
							Animal = baseNpc,
							Time = time,
							SqrDistance = sqrMagnitude
						});
					}
				}
			}
			for (int k = 0; k < animalsInRange.Count; k++)
			{
				AnimalInfo animalInfo = animalsInRange[k];
				if (time - animalInfo.Time > npc.AiDefinition.Memory.ForgetAnimalInRangeTime)
				{
					animalsInRange.RemoveAt(k);
					k--;
				}
			}
		}
	}
}
