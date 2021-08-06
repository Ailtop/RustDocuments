using System.Collections.Generic;
using UnityEngine;

public class FishBrain : BaseAIBrain<BaseFishNPC>
{
	public class IdleState : BaseIdleState
	{
		private StateStatus status = StateStatus.Error;

		private List<Vector3> idlePoints;

		private int currentPointIndex;

		private Vector3 idleRootPos;

		public override void StateLeave()
		{
			base.StateLeave();
			Stop();
		}

		public override void StateEnter()
		{
			base.StateEnter();
			idleRootPos = brain.Navigator.transform.position;
			GenerateIdlePoints(20f, 0f);
			currentPointIndex = 0;
			status = StateStatus.Error;
			if (brain.PathFinder != null)
			{
				if (brain.Navigator.SetDestination(idleRootPos + idlePoints[0], BaseNavigator.NavigationSpeed.Normal))
				{
					status = StateStatus.Running;
				}
				else
				{
					status = StateStatus.Error;
				}
			}
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (Vector3.Distance(brain.Navigator.transform.position, idleRootPos + idlePoints[currentPointIndex]) < 4f)
			{
				currentPointIndex++;
			}
			if (currentPointIndex >= idlePoints.Count)
			{
				currentPointIndex = 0;
			}
			if (brain.Navigator.SetDestination(idleRootPos + idlePoints[currentPointIndex], BaseNavigator.NavigationSpeed.Normal))
			{
				status = StateStatus.Running;
			}
			else
			{
				status = StateStatus.Error;
			}
			return status;
		}

		private void GenerateIdlePoints(float radius, float heightOffset)
		{
			if (idlePoints == null)
			{
				idlePoints = new List<Vector3>();
				float num = 0f;
				int num2 = 32;
				float height = TerrainMeta.WaterMap.GetHeight(brain.Navigator.transform.position);
				float height2 = TerrainMeta.HeightMap.GetHeight(brain.Navigator.transform.position);
				for (int i = 0; i < num2; i++)
				{
					num += 360f / (float)num2;
					Vector3 pointOnCircle = BasePathFinder.GetPointOnCircle(Vector3.zero, radius, num);
					pointOnCircle.y += Random.Range(0f - heightOffset, heightOffset);
					pointOnCircle.y = Mathf.Clamp(pointOnCircle.y, height2, height);
					idlePoints.Add(pointOnCircle);
				}
			}
		}
	}

	public class RoamState : BasicAIState
	{
		private StateStatus status = StateStatus.Error;

		public RoamState()
			: base(AIState.Roam)
		{
		}

		public override void StateLeave()
		{
			base.StateLeave();
			Stop();
		}

		public override void StateEnter()
		{
			base.StateEnter();
			status = StateStatus.Error;
			if (brain.PathFinder != null)
			{
				Vector3 fallbackPos = brain.Events.Memory.Position.Get(4);
				Vector3 bestRoamPosition = brain.PathFinder.GetBestRoamPosition(brain.Navigator, fallbackPos, 5f, brain.Navigator.MaxRoamDistanceFromHome);
				if (brain.Navigator.SetDestination(bestRoamPosition, BaseNavigator.NavigationSpeed.Normal))
				{
					status = StateStatus.Running;
				}
				else
				{
					status = StateStatus.Error;
				}
			}
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (status == StateStatus.Error)
			{
				return status;
			}
			if (brain.Navigator.Moving)
			{
				return StateStatus.Running;
			}
			return StateStatus.Finished;
		}
	}

	public static int Count;

	public override void AddStates()
	{
		base.AddStates();
		AddState(new IdleState());
		AddState(new RoamState());
		AddState(new BaseFleeState());
		AddState(new BaseChaseState());
		AddState(new BaseMoveTorwardsState());
		AddState(new BaseAttackState());
		AddState(new BaseCooldownState());
	}

	public override void InitializeAI()
	{
		base.InitializeAI();
		base.ThinkMode = AIThinkMode.Interval;
		thinkRate = 0.25f;
		base.PathFinder = new UnderwaterPathFinder();
		((UnderwaterPathFinder)base.PathFinder).Init(GetEntity());
		Count++;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Count--;
	}
}
