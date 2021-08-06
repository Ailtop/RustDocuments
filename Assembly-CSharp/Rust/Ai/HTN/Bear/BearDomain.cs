using System;
using System.Collections;
using System.Collections.Generic;
using Apex.AI;
using Apex.AI.Components;
using Apex.Ai.HTN;
using Apex.Serialization;
using ConVar;
using Rust.Ai.HTN.Bear.Reasoners;
using Rust.Ai.HTN.Bear.Sensors;
using Rust.Ai.HTN.Reasoning;
using Rust.Ai.HTN.Sensors;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.HTN.Bear
{
	public class BearDomain : HTNDomain
	{
		public class BearWorldStateEffect : EffectBase<BearContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(BearContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Fact, Value, temporary);
				}
				else
				{
					context.SetFact(Fact, Value);
				}
			}

			public override void Reverse(BearContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class BearWorldStateBoolEffect : EffectBase<BearContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override void Apply(BearContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Fact, Value, temporary);
				}
				else
				{
					context.SetFact(Fact, Value);
				}
			}

			public override void Reverse(BearContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class BearWorldStateIncrementEffect : EffectBase<BearContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(BearContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					byte b = context.PeekFactChangeDuringPlanning(Fact);
					context.PushFactChangeDuringPlanning(Fact, b + Value, temporary);
				}
				else
				{
					context.SetFact(Fact, context.GetFact(Fact) + Value);
				}
			}

			public override void Reverse(BearContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class BearHealEffect : EffectBase<BearContext>
		{
			[ApexSerialization]
			public HealthState Health;

			public override void Apply(BearContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.HealthState, Health, temporary);
				}
				else
				{
					context.SetFact(Facts.HealthState, Health);
				}
			}

			public override void Reverse(BearContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.HealthState);
				}
				else
				{
					context.SetFact(Facts.HealthState, context.GetPreviousFact(Facts.HealthState));
				}
			}
		}

		public class BearIsNavigatingEffect : EffectBase<BearContext>
		{
			public override void Apply(BearContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.IsNavigating, 1, temporary);
					return;
				}
				context.PreviousWorldState[5] = context.WorldState[5];
				context.WorldState[5] = 1;
			}

			public override void Reverse(BearContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.IsNavigating);
				}
				else
				{
					context.WorldState[5] = context.PreviousWorldState[5];
				}
			}
		}

		public class BearIsNotNavigatingEffect : EffectBase<BearContext>
		{
			public override void Apply(BearContext context, bool fromPlanner, bool temporary)
			{
				ApplyStatic(context, fromPlanner, temporary);
			}

			public override void Reverse(BearContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.IsNavigating);
				}
				else
				{
					context.WorldState[5] = context.PreviousWorldState[5];
				}
			}

			public static void ApplyStatic(BearContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.IsNavigating, (byte)0, temporary);
					return;
				}
				context.PreviousWorldState[5] = context.WorldState[5];
				context.WorldState[5] = 0;
			}
		}

		public abstract class BaseNavigateTo : OperatorBase<BearContext>
		{
			[ApexSerialization]
			public bool RunUntilArrival = true;

			protected abstract Vector3 _GetDestination(BearContext context);

			protected virtual void OnPreStart(BearContext context)
			{
			}

			protected virtual void OnStart(BearContext context)
			{
			}

			protected virtual void OnPathFailed(BearContext context)
			{
			}

			protected virtual void OnPathComplete(BearContext context)
			{
			}

			public override void Execute(BearContext context)
			{
				OnPreStart(context);
				context.Domain.SetDestination(_GetDestination(context));
				if (!RunUntilArrival)
				{
					context.OnWorldStateChangedEvent = (BearContext.WorldStateChangedEvent)Delegate.Combine(context.OnWorldStateChangedEvent, new BearContext.WorldStateChangedEvent(TrackWorldState));
				}
				OnStart(context);
			}

			private void TrackWorldState(BearContext context, Facts fact, byte oldValue, byte newValue)
			{
				if (fact == Facts.PathStatus)
				{
					switch (newValue)
					{
					case 2:
						context.OnWorldStateChangedEvent = (BearContext.WorldStateChangedEvent)Delegate.Remove(context.OnWorldStateChangedEvent, new BearContext.WorldStateChangedEvent(TrackWorldState));
						ApplyExpectedEffects(context, context.CurrentTask);
						context.Domain.StopNavigating();
						OnPathComplete(context);
						break;
					case 3:
						context.OnWorldStateChangedEvent = (BearContext.WorldStateChangedEvent)Delegate.Remove(context.OnWorldStateChangedEvent, new BearContext.WorldStateChangedEvent(TrackWorldState));
						context.Domain.StopNavigating();
						OnPathFailed(context);
						break;
					}
				}
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				switch (context.GetFact(Facts.PathStatus))
				{
				default:
					context.Domain.StopNavigating();
					OnPathFailed(context);
					return OperatorStateType.Aborted;
				case 0:
				case 2:
					ApplyExpectedEffects(context, task);
					context.Domain.StopNavigating();
					OnPathComplete(context);
					return OperatorStateType.Complete;
				case 1:
					if (RunUntilArrival)
					{
						return OperatorStateType.Running;
					}
					return OperatorStateType.Complete;
				}
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
				context.Domain.StopNavigating();
			}
		}

		public class BearNavigateToPreferredFightingRange : BaseNavigateTo
		{
			public static Vector3 GetPreferredFightingPosition(BearContext context)
			{
				if (UnityEngine.Time.time - context.Memory.CachedPreferredDistanceDestinationTime < 0.01f)
				{
					return context.Memory.CachedPreferredDistanceDestination;
				}
				NpcPlayerInfo primaryEnemyPlayerTarget = context.GetPrimaryEnemyPlayerTarget();
				if (primaryEnemyPlayerTarget.Player != null)
				{
					float closeRange = context.Body.AiDefinition.Engagement.CloseRange;
					float num = closeRange * closeRange;
					Vector3 vector = ((!(primaryEnemyPlayerTarget.SqrDistance < num)) ? (primaryEnemyPlayerTarget.Player.transform.position - context.Body.transform.position).normalized : (context.Body.transform.position - primaryEnemyPlayerTarget.Player.transform.position).normalized);
					Vector3 vector2 = context.Body.transform.position + vector * closeRange;
					Vector3 vector3 = vector2;
					for (int i = 0; i < 10; i++)
					{
						NavMeshHit hit;
						if (NavMesh.SamplePosition(vector3 + Vector3.up * 0.1f, out hit, 2f * context.Domain.NavAgent.height, -1))
						{
							Vector3 vector4 = context.Domain.ToAllowedMovementDestination(hit.position);
							if (context.Memory.IsValid(vector4))
							{
								context.Memory.CachedPreferredDistanceDestination = vector4;
								context.Memory.CachedPreferredDistanceDestinationTime = UnityEngine.Time.time;
								return vector4;
							}
						}
						else
						{
							context.Memory.AddFailedDestination(vector3);
						}
						Vector2 vector5 = UnityEngine.Random.insideUnitCircle * 5f;
						vector3 = vector2 + new Vector3(vector5.x, 0f, vector5.y);
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(BearContext context)
			{
				return GetPreferredFightingPosition(context);
			}
		}

		public class BearNavigateToLastKnownLocationOfPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(BearContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && !context.HasVisitedLastKnownEnemyPlayerLocation && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.LastKnownPosition, out hit, context.Domain.NavAgent.areaMask))
				{
					return hit.position;
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(BearContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(BearContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(BearContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(BearContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
				context.HasVisitedLastKnownEnemyPlayerLocation = false;
			}

			protected override void OnPathComplete(BearContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
				context.HasVisitedLastKnownEnemyPlayerLocation = true;
			}
		}

		public class BearNavigateInDirectionOfLastKnownHeadingOfPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(BearContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.LastKnownPosition + primaryKnownEnemyPlayer.LastKnownHeading * 2f, out hit, context.Domain.NavAgent.areaMask))
				{
					Vector3 position = hit.position;
					context.Memory.LastClosestEdgeNormal = hit.normal;
					return position;
				}
				return context.Body.transform.position;
			}

			public static Vector3 GetContinuousDestinationFromBody(BearContext context)
			{
				if (context.Memory.LastClosestEdgeNormal.sqrMagnitude < 0.01f)
				{
					return context.Body.transform.position;
				}
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					Vector3 vector = context.Body.estimatedVelocity.normalized;
					if (vector.sqrMagnitude < 0.01f)
					{
						vector = context.Body.estimatedVelocity.normalized;
					}
					if (vector.sqrMagnitude < 0.01f)
					{
						vector = primaryKnownEnemyPlayer.LastKnownHeading;
					}
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(context.Body.transform.position + vector * 2f, out hit, context.Domain.NavAgent.areaMask))
					{
						if (Vector3.Dot(context.Memory.LastClosestEdgeNormal, hit.normal) > 0.9f)
						{
							return hit.position;
						}
						context.Memory.LastClosestEdgeNormal = hit.normal;
						return hit.position + hit.normal * 0.25f;
					}
				}
				return context.Body.transform.position;
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				OperatorStateType operatorStateType = base.Tick(context, task);
				if (operatorStateType == OperatorStateType.Running)
				{
					if (context.Domain.NavAgent.remainingDistance < context.Domain.NavAgent.stoppingDistance + 0.5f)
					{
						OnContinuePath(context, task);
					}
					return operatorStateType;
				}
				return operatorStateType;
			}

			private void OnContinuePath(BearContext context, PrimitiveTaskSelector task)
			{
				Vector3 continuousDestinationFromBody = GetContinuousDestinationFromBody(context);
				if (!((context.Body.transform.position - continuousDestinationFromBody).sqrMagnitude <= 0.2f))
				{
					OnPreStart(context);
					context.Domain.SetDestination(continuousDestinationFromBody);
					OnStart(context);
				}
			}

			protected override Vector3 _GetDestination(BearContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(BearContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(BearContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(BearContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(BearContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class BearNavigateToPositionWhereWeLastSawPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(BearContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.OurLastPositionWhenLastSeen, out hit, context.Domain.NavAgent.areaMask))
				{
					return context.Domain.ToAllowedMovementDestination(hit.position);
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(BearContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(BearContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(BearContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(BearContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(BearContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class BearNavigateAwayFromAnimal : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsAvoidingAnimalOnComplete = true;

			public static Vector3 GetDestination(BearContext context)
			{
				if (context.Memory.PrimaryKnownAnimal.Animal != null)
				{
					Vector3 normalized = (context.BodyPosition - context.Memory.PrimaryKnownAnimal.Animal.transform.position).normalized;
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(context.BodyPosition + normalized * 10f, out hit, context.Domain.NavAgent.areaMask))
					{
						context.Memory.LastClosestEdgeNormal = hit.normal;
						return hit.position;
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(BearContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(BearContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(BearContext context)
			{
				context.SetFact(Facts.IsAvoidingAnimal, true);
			}

			protected override void OnPathFailed(BearContext context)
			{
				context.SetFact(Facts.IsAvoidingAnimal, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(BearContext context)
			{
				if (DisableIsAvoidingAnimalOnComplete)
				{
					context.SetFact(Facts.IsAvoidingAnimal, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class BearArrivedAtLocation : OperatorBase<BearContext>
		{
			public override void Execute(BearContext context)
			{
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class BearStopMoving : OperatorBase<BearContext>
		{
			public override void Execute(BearContext context)
			{
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class BearIdle_JustStandAround : OperatorBase<BearContext>
		{
			public override void Execute(BearContext context)
			{
				ResetWorldState(context);
				context.SetFact(Facts.IsIdle, true);
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				return OperatorStateType.Running;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsIdle, false);
			}

			private void ResetWorldState(BearContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.SetFact(Facts.IsNavigating, false);
				context.SetFact(Facts.IsLookingAround, false);
			}
		}

		public class BearLookAround : OperatorBase<BearContext>
		{
			[ApexSerialization]
			private float _lookAroundTime = 1f;

			public override void Execute(BearContext context)
			{
				context.SetFact(Facts.IsLookingAround, true);
				context.Body.StartCoroutine(LookAroundAsync(context));
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsLookingAround))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			private IEnumerator LookAroundAsync(BearContext context)
			{
				yield return CoroutineEx.waitForSeconds(_lookAroundTime);
				if (context.IsFact(Facts.CanSeeEnemy))
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.SetFact(Facts.IsLookingAround, false);
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsSearching, false);
				context.SetFact(Facts.IsLookingAround, false);
			}
		}

		public class BearApplyFrustration : OperatorBase<BearContext>
		{
			public override void Execute(BearContext context)
			{
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class BearStandUp : OperatorBase<BearContext>
		{
			[ApexSerialization]
			private float _standUpTime = 0.5f;

			public override void Execute(BearContext context)
			{
				context.Domain.StopNavigating();
				context.Body.ClientRPC(null, "PlayAnimationTrigger", "standUp");
				context.Body.StartCoroutine(AsyncTimer(context, _standUpTime));
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsTransitioning))
				{
					return OperatorStateType.Running;
				}
				context.Body.ClientRPC(null, "PlayAnimationBool", "standing", 1);
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
				context.Body.ClientRPC(null, "PlayAnimationBool", "standing", 0);
				context.Body.ClientRPC(null, "PlayAnimationTrigger", "standDown");
				context.SetFact(Facts.IsTransitioning, false);
			}

			private IEnumerator AsyncTimer(BearContext context, float time)
			{
				context.SetFact(Facts.IsTransitioning, true);
				yield return CoroutineEx.waitForSeconds(time);
				context.SetFact(Facts.IsTransitioning, false);
			}
		}

		public class BearStandDown : OperatorBase<BearContext>
		{
			[ApexSerialization]
			private float _standDownTime = 0.5f;

			public override void Execute(BearContext context)
			{
				context.Domain.StopNavigating();
				context.Body.ClientRPC(null, "PlayAnimationTrigger", "standDown");
				context.Body.StartCoroutine(AsyncTimer(context, _standDownTime));
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsTransitioning))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsTransitioning, false);
			}

			private IEnumerator AsyncTimer(BearContext context, float time)
			{
				context.SetFact(Facts.IsTransitioning, true);
				yield return CoroutineEx.waitForSeconds(time);
				context.SetFact(Facts.IsTransitioning, false);
			}
		}

		public class BearPlayAnimationTrigger : OperatorBase<BearContext>
		{
			[ApexSerialization]
			private float _timeout = 0.5f;

			[ApexSerialization]
			private string animationStr = "";

			public override void Execute(BearContext context)
			{
				context.Domain.StopNavigating();
				context.Body.ClientRPC(null, "PlayAnimationTrigger", animationStr);
				context.Body.StartCoroutine(AsyncTimer(context, _timeout));
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsTransitioning))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsTransitioning, false);
			}

			private IEnumerator AsyncTimer(BearContext context, float time)
			{
				context.SetFact(Facts.IsTransitioning, true);
				yield return CoroutineEx.waitForSeconds(time);
				context.SetFact(Facts.IsTransitioning, false);
			}
		}

		public class BearPlayAnimationBool : OperatorBase<BearContext>
		{
			[ApexSerialization]
			private float _timeout = 0.5f;

			[ApexSerialization]
			private string animationStr = "";

			[ApexSerialization]
			private bool animationValue;

			public override void Execute(BearContext context)
			{
				context.Domain.StopNavigating();
				context.Body.ClientRPC(null, "PlayAnimationBool", animationStr, animationValue);
				context.Body.StartCoroutine(AsyncTimer(context, _timeout));
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsTransitioning))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsTransitioning, false);
			}

			private IEnumerator AsyncTimer(BearContext context, float time)
			{
				context.SetFact(Facts.IsTransitioning, true);
				yield return CoroutineEx.waitForSeconds(time);
				context.SetFact(Facts.IsTransitioning, false);
			}
		}

		public class BearPlayAnimationInt : OperatorBase<BearContext>
		{
			[ApexSerialization]
			private float _timeout = 0.5f;

			[ApexSerialization]
			private string animationStr = "";

			[ApexSerialization]
			private int animationValue;

			public override void Execute(BearContext context)
			{
				context.Domain.StopNavigating();
				context.Body.ClientRPC(null, "PlayAnimationInt", animationStr, animationValue);
				context.Body.StartCoroutine(AsyncTimer(context, _timeout));
			}

			public override OperatorStateType Tick(BearContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsTransitioning))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(BearContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsTransitioning, false);
			}

			private IEnumerator AsyncTimer(BearContext context, float time)
			{
				context.SetFact(Facts.IsTransitioning, true);
				yield return CoroutineEx.waitForSeconds(time);
				context.SetFact(Facts.IsTransitioning, false);
			}
		}

		public delegate void OnPlanAborted(BearDomain domain);

		public delegate void OnPlanCompleted(BearDomain domain);

		public class BearHasWorldState : ContextualScorerBase<BearContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(BearContext c)
			{
				if (c.GetWorldState(Fact) != Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearHasWorldStateBool : ContextualScorerBase<BearContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override float Score(BearContext c)
			{
				byte b = (byte)(Value ? 1u : 0u);
				if (c.GetWorldState(Fact) != b)
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearHasWorldStateGreaterThan : ContextualScorerBase<BearContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(BearContext c)
			{
				if (c.GetWorldState(Fact) <= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearHasWorldStateLessThan : ContextualScorerBase<BearContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(BearContext c)
			{
				if (c.GetWorldState(Fact) >= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearHasWorldStateEnemyRange : ContextualScorerBase<BearContext>
		{
			[ApexSerialization]
			public EnemyRange Value;

			public override float Score(BearContext c)
			{
				if ((uint)c.GetWorldState(Facts.EnemyRange) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearHasWorldStateHealth : ContextualScorerBase<BearContext>
		{
			[ApexSerialization]
			public HealthState Value;

			public override float Score(BearContext c)
			{
				if ((uint)c.GetWorldState(Facts.HealthState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearCanNavigateToPreferredFightingRange : ContextualScorerBase<BearContext>
		{
			[ApexSerialization]
			private bool CanNot;

			public override float Score(BearContext c)
			{
				Vector3 preferredFightingPosition = BearNavigateToPreferredFightingRange.GetPreferredFightingPosition(c);
				if ((preferredFightingPosition - c.Body.transform.position).sqrMagnitude < 0.01f)
				{
					if (!CanNot)
					{
						return 0f;
					}
					return score;
				}
				bool flag = c.Memory.IsValid(preferredFightingPosition);
				if (CanNot)
				{
					if (!flag)
					{
						return score;
					}
					return 0f;
				}
				if (!flag)
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearCanRememberPrimaryEnemyTarget : ContextualScorerBase<BearContext>
		{
			public override float Score(BearContext c)
			{
				if (!(c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null))
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearCanNavigateToLastKnownPositionOfPrimaryEnemyTarget : ContextualScorerBase<BearContext>
		{
			public override float Score(BearContext c)
			{
				if (c.HasVisitedLastKnownEnemyPlayerLocation)
				{
					return score;
				}
				Vector3 destination = BearNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(c);
				if (!c.Domain.AllowedMovementDestination(destination))
				{
					return 0f;
				}
				if ((destination - c.Body.transform.position).sqrMagnitude < 0.1f)
				{
					return 0f;
				}
				if (!c.Memory.IsValid(destination))
				{
					return 0f;
				}
				return score;
			}
		}

		public class BearCanNavigateAwayFromAnimal : ContextualScorerBase<BearContext>
		{
			public override float Score(BearContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(BearContext c)
			{
				Vector3 destination = BearNavigateAwayFromAnimal.GetDestination(c);
				if (!c.Domain.AllowedMovementDestination(destination))
				{
					return false;
				}
				if ((destination - c.Body.transform.position).sqrMagnitude < 0.1f)
				{
					return false;
				}
				return c.Memory.IsValid(destination);
			}
		}

		public class BearCanAttackAtCurrentRange : ContextualScorerBase<BearContext>
		{
			public override float Score(BearContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(BearContext c)
			{
				if (c.GetFact(Facts.EnemyRange) == 0)
				{
					return true;
				}
				return false;
			}
		}

		[ReadOnly]
		[SerializeField]
		private bool _isRegisteredWithAgency;

		private static Vector3[] pathCornerCache = new Vector3[128];

		private static NavMeshPath _pathCache = null;

		public OnPlanAborted OnPlanAbortedEvent;

		public OnPlanCompleted OnPlanCompletedEvent;

		[Header("Context")]
		[SerializeField]
		private BearContext _context;

		[Header("Navigation")]
		[ReadOnly]
		[SerializeField]
		private NavMeshAgent _navAgent;

		[ReadOnly]
		[SerializeField]
		private Vector3 _spawnPosition;

		[Header("Sensors")]
		[ReadOnly]
		[SerializeField]
		private List<INpcSensor> _sensors = new List<INpcSensor>
		{
			new BearPlayersInRangeSensor
			{
				TickFrequency = 0.5f
			},
			new BearPlayersOutsideRangeSensor
			{
				TickFrequency = 0.1f
			},
			new BearPlayersDistanceSensor
			{
				TickFrequency = 0.1f
			},
			new BearPlayersViewAngleSensor
			{
				TickFrequency = 0.1f
			},
			new BearEnemyPlayersInRangeSensor
			{
				TickFrequency = 0.1f
			},
			new BearEnemyPlayersLineOfSightSensor
			{
				TickFrequency = 0.25f
			},
			new BearEnemyPlayersHearingSensor
			{
				TickFrequency = 0.1f
			},
			new AnimalsInRangeSensor
			{
				TickFrequency = 1f
			},
			new AnimalDistanceSensor
			{
				TickFrequency = 0.25f
			}
		};

		[Header("Reasoners")]
		[ReadOnly]
		[SerializeField]
		private List<INpcReasoner> _reasoners = new List<INpcReasoner>
		{
			new EnemyPlayerLineOfSightReasoner
			{
				TickFrequency = 0.1f
			},
			new EnemyPlayerHearingReasoner
			{
				TickFrequency = 0.1f
			},
			new EnemyTargetReasoner
			{
				TickFrequency = 0.1f
			},
			new PlayersInRangeReasoner
			{
				TickFrequency = 0.1f
			},
			new OrientationReasoner
			{
				TickFrequency = 0.01f
			},
			new PreferredFightingRangeReasoner
			{
				TickFrequency = 0.1f
			},
			new AtLastKnownEnemyPlayerLocationReasoner
			{
				TickFrequency = 0.1f
			},
			new HealthReasoner
			{
				TickFrequency = 0.1f
			},
			new VulnerabilityReasoner
			{
				TickFrequency = 0.1f
			},
			new FrustrationReasoner
			{
				TickFrequency = 0.25f
			},
			new ReturnHomeReasoner
			{
				TickFrequency = 5f
			},
			new AtHomeLocationReasoner
			{
				TickFrequency = 5f
			},
			new AnimalReasoner
			{
				TickFrequency = 0.25f
			},
			new AlertnessReasoner
			{
				TickFrequency = 0.1f
			},
			new EnemyRangeReasoner
			{
				TickFrequency = 0.1f
			}
		};

		private HTNUtilityAiClient _aiClient;

		private BearDefinition _bearDefinition;

		public BearDefinition BearDefinition
		{
			get
			{
				if (_bearDefinition == null)
				{
					_bearDefinition = _context.Body.AiDefinition as BearDefinition;
				}
				return _bearDefinition;
			}
		}

		public Vector3 SpawnPosition => _spawnPosition;

		public BearContext BearContext => _context;

		public override BaseNpcContext NpcContext => _context;

		public override IHTNContext PlannerContext => _context;

		public override IUtilityAI PlannerAi => _aiClient.ai;

		public override IUtilityAIClient PlannerAiClient => _aiClient;

		public override NavMeshAgent NavAgent => _navAgent;

		public override List<INpcSensor> Sensors => _sensors;

		public override List<INpcReasoner> Reasoners => _reasoners;

		private void InitializeAgency()
		{
			if (!(SingletonComponent<AiManager>.Instance == null) && SingletonComponent<AiManager>.Instance.enabled && ConVar.AI.npc_enable && !_isRegisteredWithAgency)
			{
				_isRegisteredWithAgency = true;
				SingletonComponent<AiManager>.Instance.HTNAgency.Add(_context.Body);
			}
		}

		private void RemoveAgency()
		{
			if (!(SingletonComponent<AiManager>.Instance == null) && _isRegisteredWithAgency)
			{
				_isRegisteredWithAgency = false;
				SingletonComponent<AiManager>.Instance.HTNAgency.Remove(_context.Body);
			}
		}

		public override void Resume()
		{
			ResumeNavigation();
		}

		public override void Pause()
		{
			PauseNavigation();
		}

		public void PauseNavigation()
		{
			if (NavAgent != null && NavAgent.enabled)
			{
				NavAgent.enabled = false;
			}
		}

		public void ResumeNavigation()
		{
			if (!(NavAgent == null))
			{
				if (!NavAgent.isOnNavMesh)
				{
					StartCoroutine(TryForceToNavmesh());
					return;
				}
				NavAgent.enabled = true;
				NavAgent.stoppingDistance = 1f;
				UpdateNavmeshOffset();
			}
		}

		public override Vector3 GetNextPosition(float delta)
		{
			if (NavAgent == null || !NavAgent.isOnNavMesh || !NavAgent.hasPath)
			{
				return _context.BodyPosition;
			}
			return NavAgent.nextPosition;
		}

		private void UpdateNavmeshOffset()
		{
			float num = _spawnPosition.y - _context.BodyPosition.y;
			if (num < 0f)
			{
				num = Mathf.Max(num, -0.25f);
				NavAgent.baseOffset = num;
			}
		}

		private IEnumerator TryForceToNavmesh()
		{
			yield return null;
			int numTries = 0;
			float waitForRetryTime2 = 1f;
			float maxDistanceMultiplier = 2f;
			if (SingletonComponent<DynamicNavMesh>.Instance != null)
			{
				while (SingletonComponent<DynamicNavMesh>.Instance.IsBuilding)
				{
					yield return CoroutineEx.waitForSecondsRealtime(waitForRetryTime2);
					waitForRetryTime2 += 0.5f;
				}
			}
			waitForRetryTime2 = 1f;
			for (; numTries < 3; numTries++)
			{
				if (NavAgent != null && !NavAgent.isOnNavMesh)
				{
					NavMeshHit hit;
					if (NavMesh.SamplePosition(_context.Body.transform.position, out hit, NavAgent.height * maxDistanceMultiplier, NavAgent.areaMask))
					{
						_context.Body.transform.position = hit.position;
						NavAgent.Warp(_context.Body.transform.position);
						NavAgent.enabled = true;
						NavAgent.stoppingDistance = 1f;
						UpdateNavmeshOffset();
						yield break;
					}
					yield return CoroutineEx.waitForSecondsRealtime(waitForRetryTime2);
					maxDistanceMultiplier *= 1.5f;
					continue;
				}
				NavAgent.enabled = true;
				NavAgent.stoppingDistance = 1f;
				yield break;
			}
			int areaFromName = NavMesh.GetAreaFromName("Walkable");
			if ((NavAgent.areaMask & (1 << areaFromName)) == 0)
			{
				NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(1);
				NavAgent.agentTypeID = settingsByIndex.agentTypeID;
				NavAgent.areaMask = 1 << areaFromName;
				yield return TryForceToNavmesh();
			}
			else if (_context.Body.transform != null && !_context.Body.IsDestroyed)
			{
				Debug.LogWarningFormat("Failed to spawn {0} on a valid navmesh.", base.name);
				_context.Body.Kill();
			}
		}

		public bool SetDestination(Vector3 destination)
		{
			if (NavAgent == null || !NavAgent.isOnNavMesh)
			{
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
				return false;
			}
			destination = ToAllowedMovementDestination(destination);
			_context.Memory.HasTargetDestination = true;
			_context.Memory.TargetDestination = destination;
			_context.Domain.NavAgent.destination = destination;
			if (!IsPathValid())
			{
				_context.Memory.AddFailedDestination(_context.Memory.TargetDestination);
				_context.Domain.NavAgent.isStopped = true;
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
				return false;
			}
			_context.Domain.NavAgent.isStopped = false;
			_context.SetFact(Facts.PathStatus, (byte)1, true, false, true);
			return true;
		}

		public override void TickDestinationTracker()
		{
			if (NavAgent == null || !NavAgent.isOnNavMesh)
			{
				_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
				return;
			}
			if (!IsPathValid())
			{
				_context.Memory.AddFailedDestination(_context.Memory.TargetDestination);
				_context.Domain.NavAgent.isStopped = true;
				_context.Memory.HasTargetDestination = false;
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
			}
			if (_context.Memory.HasTargetDestination && _context.Domain.NavAgent.remainingDistance <= _context.Domain.NavAgent.stoppingDistance)
			{
				_context.Memory.HasTargetDestination = false;
				_context.SetFact(Facts.PathStatus, (byte)2, true, false, true);
			}
			if (_context.Memory.HasTargetDestination && NavAgent.hasPath)
			{
				_context.SetFact(Facts.PathStatus, (byte)1, true, false, true);
			}
			else
			{
				_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
			}
		}

		public bool IsPathValid()
		{
			if (!_context.IsBodyAlive())
			{
				return false;
			}
			if (_context.Memory.HasTargetDestination && !_context.Domain.NavAgent.pathPending && (_context.Domain.NavAgent.pathStatus != 0 || (_context.Domain.NavAgent.destination - _context.Memory.TargetDestination).sqrMagnitude > 0.01f))
			{
				return false;
			}
			if (!AllowedMovementDestination(_context.Memory.TargetDestination))
			{
				return false;
			}
			return true;
		}

		public override Vector3 GetHeadingDirection()
		{
			if (NavAgent != null && NavAgent.isOnNavMesh && _context.GetFact(Facts.IsNavigating) > 0)
			{
				return NavAgent.desiredVelocity.normalized;
			}
			return _context.Body.transform.forward;
		}

		public override Vector3 GetHomeDirection()
		{
			Vector3 v = SpawnPosition - _context.BodyPosition;
			if (v.SqrMagnitudeXZ() < 0.01f)
			{
				return _context.Body.transform.forward;
			}
			return v.normalized;
		}

		public void StopNavigating()
		{
			if (NavAgent != null && NavAgent.isOnNavMesh)
			{
				NavAgent.isStopped = true;
			}
			_context.Memory.HasTargetDestination = false;
			_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
		}

		public bool PathDistanceIsValid(Vector3 from, Vector3 to, bool allowCloseRange = false)
		{
			float sqrMagnitude = (from - to).sqrMagnitude;
			if (sqrMagnitude > BearContext.Body.AiDefinition.Engagement.SqrMediumRange || (!allowCloseRange && sqrMagnitude < BearContext.Body.AiDefinition.Engagement.SqrCloseRange))
			{
				return true;
			}
			float num = Mathf.Sqrt(sqrMagnitude);
			if (_pathCache == null)
			{
				_pathCache = new NavMeshPath();
			}
			if (NavMesh.CalculatePath(from, to, NavAgent.areaMask, _pathCache))
			{
				int cornersNonAlloc = _pathCache.GetCornersNonAlloc(pathCornerCache);
				if (_pathCache.status == NavMeshPathStatus.PathComplete && cornersNonAlloc > 1)
				{
					float num2 = PathDistance(cornersNonAlloc, ref pathCornerCache, num + ConVar.AI.npc_cover_path_vs_straight_dist_max_diff);
					if (Mathf.Abs(num - num2) > ConVar.AI.npc_cover_path_vs_straight_dist_max_diff)
					{
						return false;
					}
				}
			}
			return true;
		}

		private float PathDistance(int count, ref Vector3[] path, float maxDistance)
		{
			if (count < 2)
			{
				return 0f;
			}
			Vector3 a = path[0];
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				Vector3 vector = path[i];
				num += Vector3.Distance(a, vector);
				a = vector;
				if (num > maxDistance)
				{
					return num;
				}
			}
			return num;
		}

		public override float SqrDistanceToSpawn()
		{
			return (_context.BodyPosition - SpawnPosition).sqrMagnitude;
		}

		public override bool AllowedMovementDestination(Vector3 destination)
		{
			if (Movement == MovementRule.FreeMove)
			{
				return true;
			}
			if (Movement == MovementRule.NeverMove)
			{
				return false;
			}
			return (SpawnPosition - destination).sqrMagnitude <= base.SqrMovementRadius + 0.1f;
		}

		public Vector3 ToAllowedMovementDestination(Vector3 destination)
		{
			if (!AllowedMovementDestination(destination))
			{
				Vector3 normalized = (destination - _context.Domain.SpawnPosition).normalized;
				destination = _context.Domain.SpawnPosition + normalized * MovementRadius;
			}
			return destination;
		}

		protected override void AbortPlan()
		{
			base.AbortPlan();
			OnPlanAbortedEvent?.Invoke(this);
			if (BearContext.IsFact(Facts.IsStandingUp))
			{
				BearContext.Body.ClientRPC(null, "PlayAnimationBool", "standing", 0);
				BearContext.Body.ClientRPC(null, "PlayAnimationTrigger", "standDown");
				BearContext.SetFact(Facts.IsStandingUp, false);
			}
		}

		protected override void CompletePlan()
		{
			base.CompletePlan();
			OnPlanCompletedEvent?.Invoke(this);
			if (BearContext.IsFact(Facts.IsStandingUp))
			{
				BearContext.Body.ClientRPC(null, "PlayAnimationBool", "standing", 0);
				BearContext.Body.ClientRPC(null, "PlayAnimationTrigger", "standDown");
				BearContext.SetFact(Facts.IsStandingUp, false);
			}
		}

		protected override void TickReasoner(INpcReasoner reasoner, float deltaTime, float time)
		{
			reasoner.Tick(_context.Body, deltaTime, time);
		}

		public override void OnSensation(Sensation sensation)
		{
			switch (sensation.Type)
			{
			case SensationType.Gunshot:
				OnGunshotSensation(ref sensation);
				break;
			case SensationType.ThrownWeapon:
				OnThrownWeaponSensation(ref sensation);
				break;
			case SensationType.Explosion:
				OnExplosionSensation(ref sensation);
				break;
			}
		}

		private void OnGunshotSensation(ref Sensation info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberGunshot(ref info, item, initiatorPlayer))
				{
					if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || _context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == initiatorPlayer)
					{
						_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
					}
					_context.IncrementFact(Facts.Vulnerability, (!_context.IsFact(Facts.CanSeeEnemy)) ? 1 : 0);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
				_context.IncrementFact(Facts.Alertness, 1);
				_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
				{
					Player = initiatorPlayer,
					Time = UnityEngine.Time.time
				});
			}
		}

		private void OnThrownWeaponSensation(ref Sensation info)
		{
			RememberEntityOfInterest(ref info);
			if (!_context.IsFact(Facts.CanSeeEnemy) || !_context.IsFact(Facts.CanHearEnemy))
			{
				return;
			}
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberThrownItem(ref info, item, initiatorPlayer))
				{
					if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
					{
						_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
					}
					_context.IncrementFact(Facts.Vulnerability, (!_context.IsFact(Facts.CanSeeEnemy)) ? 1 : 0);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
				_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
				{
					Player = initiatorPlayer,
					Time = UnityEngine.Time.time
				});
			}
		}

		private void OnExplosionSensation(ref Sensation info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberExplosion(ref info, item, initiatorPlayer))
				{
					_context.IncrementFact(Facts.Vulnerability, _context.IsFact(Facts.CanSeeEnemy) ? 1 : 2);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
				_context.IncrementFact(Facts.Alertness, 1);
			}
		}

		private void RememberEntityOfInterest(ref Sensation info)
		{
			if (info.UsedEntity != null)
			{
				_context.Memory.RememberEntityOfInterest(_context.Body, info.UsedEntity, UnityEngine.Time.time, info.UsedEntity.name);
			}
		}

		private bool RememberGunshot(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float uncertainty = info.Radius * 0.05f;
				_context.Memory.RememberEnemyPlayer(_context.Body, ref player, UnityEngine.Time.time, uncertainty, "GUNSHOT!");
				return true;
			}
			return false;
		}

		private bool RememberExplosion(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			return false;
		}

		private bool RememberThrownItem(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float uncertainty = info.Radius * 0.05f;
				_context.Memory.RememberEnemyPlayer(_context.Body, ref player, UnityEngine.Time.time, uncertainty, "THROW!");
				return true;
			}
			return false;
		}

		protected override void TickSensor(INpcSensor sensor, float deltaTime, float time)
		{
			sensor.Tick(_context.Body, deltaTime, time);
		}

		public override IAIContext GetContext(Guid aiId)
		{
			return _context;
		}

		public override void Initialize(BaseEntity body)
		{
			if (_aiClient == null || _aiClient.ai == null || _aiClient.ai.id != AINameMap.HTNDomainAnimalBear)
			{
				_aiClient = new HTNUtilityAiClient(AINameMap.HTNDomainAnimalBear, this);
			}
			if (_context == null || _context.Body != body)
			{
				_context = new BearContext(body as HTNAnimal, this);
			}
			if (_navAgent == null)
			{
				_navAgent = GetComponent<NavMeshAgent>();
			}
			if ((bool)_navAgent)
			{
				_navAgent.updateRotation = false;
				_navAgent.updatePosition = false;
			}
			_spawnPosition = body.transform.position;
			_aiClient.Initialize();
			_context.Body.Resume();
			InitializeAgency();
		}

		public override void Dispose()
		{
			_aiClient?.Kill();
			RemoveAgency();
		}

		public override void ResetState()
		{
			base.ResetState();
		}

		public override void Tick(float time)
		{
			base.Tick(time);
			_context.Memory.Forget(_context.Body.AiDefinition.Memory.ForgetTime);
		}

		public override void OnHurt(HitInfo info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberPlayerThatHurtUs(item, initiatorPlayer))
				{
					if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
					{
						_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
					}
					_context.IncrementFact(Facts.Vulnerability, _context.IsFact(Facts.CanSeeEnemy) ? 1 : 10);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 10);
				_context.IncrementFact(Facts.Alertness, 1);
				_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
				{
					Player = initiatorPlayer,
					Time = UnityEngine.Time.time
				});
			}
		}

		private bool RememberPlayerThatHurtUs(NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float num = 0f;
				NpcPlayerInfo info = player;
				BaseProjectile baseProjectile = initiator.GetHeldEntity() as BaseProjectile;
				if (baseProjectile != null)
				{
					num = baseProjectile.NoiseRadius * 0.1f;
					if (baseProjectile.IsSilenced())
					{
						num *= 3f;
					}
				}
				_context.Memory.RememberEnemyPlayer(_context.Body, ref info, UnityEngine.Time.time, num, "HURT!");
				return true;
			}
			return false;
		}

		public override void ForceProjectileOrientation()
		{
		}
	}
}
