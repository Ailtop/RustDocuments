using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/BaseMission")]
public class BaseMission : BaseScriptableObject
{
	[Serializable]
	public class MissionDependancy
	{
		public string targetMissionShortname;

		public MissionStatus targetMissionDesiredStatus;

		public bool everAttempted;

		public uint targetMissionID => targetMissionShortname.ManifestHash();
	}

	public enum MissionStatus
	{
		Default = 0,
		Active = 1,
		Accomplished = 2,
		Failed = 3,
		Completed = 4
	}

	public enum MissionEventType
	{
		CUSTOM = 0,
		HARVEST = 1,
		CONVERSATION = 2,
		KILL_ENTITY = 3,
		ACQUIRE_ITEM = 4,
		FREE_CRATE = 5
	}

	[Serializable]
	public class MissionObjectiveEntry
	{
		public Translate.Phrase description;

		public int[] startAfterCompletedObjectives;

		public int[] autoCompleteOtherObjectives;

		public bool onlyProgressIfStarted = true;

		public MissionObjective objective;

		public MissionObjective Get()
		{
			return objective;
		}
	}

	public class MissionInstance : Pool.IPooled
	{
		[Serializable]
		public class ObjectiveStatus
		{
			public bool started;

			public bool completed;

			public bool failed;

			public int genericInt1;

			public float genericFloat1;
		}

		public enum ObjectiveType
		{
			MOVE = 0,
			KILL = 1
		}

		private BaseEntity _cachedProviderEntity;

		private BaseMission _cachedMission;

		public uint providerID;

		public uint missionID;

		public MissionStatus status;

		public float completionScale;

		public float startTime;

		public float endTime;

		public Vector3 missionLocation;

		public float timePassed;

		public Dictionary<string, Vector3> missionPoints = new Dictionary<string, Vector3>();

		public ObjectiveStatus[] objectiveStatuses;

		public List<MissionEntity> createdEntities;

		public ItemAmount[] rewards;

		public BaseEntity ProviderEntity()
		{
			if (_cachedProviderEntity == null)
			{
				_cachedProviderEntity = BaseNetworkable.serverEntities.Find(providerID) as BaseEntity;
			}
			return _cachedProviderEntity;
		}

		public BaseMission GetMission()
		{
			if (_cachedMission == null)
			{
				_cachedMission = MissionManifest.GetFromID(missionID);
			}
			return _cachedMission;
		}

		public bool ShouldShowOnMap()
		{
			if (status == MissionStatus.Active || status == MissionStatus.Accomplished)
			{
				return missionLocation != Vector3.zero;
			}
			return false;
		}

		public bool ShouldShowOnCompass()
		{
			return ShouldShowOnMap();
		}

		public virtual void ProcessMissionEvent(BasePlayer playerFor, MissionEventType type, string identifier, float amount)
		{
			if (status == MissionStatus.Active)
			{
				BaseMission mission = GetMission();
				for (int i = 0; i < mission.objectives.Length; i++)
				{
					mission.objectives[i].objective.ProcessMissionEvent(playerFor, this, i, type, identifier, amount);
				}
			}
		}

		public void Think(BasePlayer assignee, float delta)
		{
			if (status != MissionStatus.Failed && status != MissionStatus.Completed)
			{
				BaseMission mission = GetMission();
				timePassed += delta;
				mission.Think(this, assignee, delta);
				if (mission.timeLimitSeconds > 0f && timePassed >= mission.timeLimitSeconds)
				{
					mission.MissionFailed(this, assignee, MissionFailReason.TimeOut);
				}
			}
		}

		public Vector3 GetMissionPoint(string identifier, BasePlayer playerFor)
		{
			if (missionPoints.ContainsKey(identifier))
			{
				return missionPoints[identifier];
			}
			if ((bool)playerFor)
			{
				GetMission().SetupPositions(this, playerFor);
				Debug.Log("Mission point not found, regenerating");
				if (missionPoints.ContainsKey(identifier))
				{
					return missionPoints[identifier];
				}
				return Vector3.zero;
			}
			Debug.Log("Massive mission failure to get point, correct mission definition of : " + GetMission().shortname);
			return Vector3.zero;
		}

		public void EnterPool()
		{
			providerID = 0u;
			missionID = 0u;
			status = MissionStatus.Default;
			completionScale = 0f;
			startTime = -1f;
			endTime = -1f;
			missionLocation = Vector3.zero;
			_cachedMission = null;
			timePassed = 0f;
			rewards = null;
			missionPoints.Clear();
			if (createdEntities != null)
			{
				Pool.FreeList(ref createdEntities);
			}
		}

		public void LeavePool()
		{
			createdEntities = Pool.GetList<MissionEntity>();
		}
	}

	[Serializable]
	public class PositionGenerator
	{
		public enum PositionType
		{
			MissionPoint = 0,
			WorldPositionGenerator = 1
		}

		public string identifier;

		public float minDistForMovePoint;

		public float maxDistForMovePoint = 25f;

		public bool centerOnProvider;

		public bool centerOnPlayer;

		public PositionType positionType;

		[Header("MissionPoint")]
		[InspectorFlags]
		public MissionPoint.MissionPointEnum Flags = (MissionPoint.MissionPointEnum)(-1);

		[InspectorFlags]
		public MissionPoint.MissionPointEnum ExclusionFlags;

		[Header("WorldPositionGenerator")]
		public WorldPositionGenerator worldPositionGenerator;

		public string GetIdentifier()
		{
			return identifier;
		}

		public bool Validate(BasePlayer assignee)
		{
			Vector3 position;
			if (positionType == PositionType.MissionPoint)
			{
				List<MissionPoint> points = Pool.GetList<MissionPoint>();
				bool missionPoints = MissionPoint.GetMissionPoints(ref points, assignee.transform.position, minDistForMovePoint, maxDistForMovePoint, (int)Flags, (int)ExclusionFlags);
				Pool.FreeList(ref points);
				if (!missionPoints)
				{
					Debug.Log("FAILED TO FIND MISSION POINTS");
					return false;
				}
			}
			else if (positionType == PositionType.WorldPositionGenerator && worldPositionGenerator != null && !worldPositionGenerator.TrySample(assignee.transform.position, minDistForMovePoint, maxDistForMovePoint, out position, blockedPoints))
			{
				Debug.Log("FAILED TO GENERATE WORLD POSITION!!!!!");
				return false;
			}
			return true;
		}

		public Vector3 GetPosition(BasePlayer assignee)
		{
			Vector3 position;
			if (positionType == PositionType.MissionPoint)
			{
				List<MissionPoint> points = Pool.GetList<MissionPoint>();
				if (MissionPoint.GetMissionPoints(ref points, assignee.transform.position, minDistForMovePoint, maxDistForMovePoint, (int)Flags, (int)ExclusionFlags))
				{
					position = points[UnityEngine.Random.Range(0, points.Count)].GetPosition();
				}
				else
				{
					Debug.LogError("UNABLE TO FIND MISSIONPOINT FOR MISSION!");
					position = assignee.transform.position;
				}
				Pool.FreeList(ref points);
			}
			else if (positionType == PositionType.WorldPositionGenerator && worldPositionGenerator != null)
			{
				if (!worldPositionGenerator.TrySample(assignee.transform.position, minDistForMovePoint, maxDistForMovePoint, out position, blockedPoints))
				{
					Debug.LogError("UNABLE TO FIND WORLD POINT FOR MISSION!");
					position = assignee.transform.position;
				}
			}
			else
			{
				Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
				onUnitSphere.y = 0f;
				onUnitSphere.Normalize();
				position = (centerOnPlayer ? assignee.transform.position : assignee.transform.position) + onUnitSphere * UnityEngine.Random.Range(minDistForMovePoint, maxDistForMovePoint);
				float b = position.y;
				float a = position.y;
				if (TerrainMeta.WaterMap != null)
				{
					a = TerrainMeta.WaterMap.GetHeight(position);
				}
				if (TerrainMeta.HeightMap != null)
				{
					b = TerrainMeta.HeightMap.GetHeight(position);
				}
				position.y = Mathf.Max(a, b);
			}
			return position;
		}
	}

	[Serializable]
	public class MissionEntityEntry
	{
		public GameObjectRef entityRef;

		public string spawnPositionToUse;

		public bool cleanupOnMissionFailed;

		public bool cleanupOnMissionSuccess;
	}

	public enum MissionFailReason
	{
		TimeOut = 0,
		Disconnect = 1,
		ResetPlayerState = 2,
		Abandon = 3
	}

	[ServerVar]
	public static bool missionsenabled = true;

	public string shortname;

	public Translate.Phrase missionName;

	public Translate.Phrase missionDesc;

	public MissionObjectiveEntry[] objectives;

	public static List<Vector3> blockedPoints = new List<Vector3>();

	public const string MISSION_COMPLETE_STAT = "missions_completed";

	public GameObjectRef acceptEffect;

	public GameObjectRef failedEffect;

	public GameObjectRef victoryEffect;

	public int repeatDelaySecondsSuccess = -1;

	public int repeatDelaySecondsFailed = -1;

	public float timeLimitSeconds;

	public Sprite icon;

	public Sprite providerIcon;

	public MissionDependancy[] acceptDependancies;

	public MissionDependancy[] completionDependancies;

	public MissionEntityEntry[] missionEntities;

	public PositionGenerator[] positionGenerators;

	public ItemAmount[] baseRewards;

	public uint id => shortname.ManifestHash();

	public bool isRepeatable
	{
		get
		{
			if (repeatDelaySecondsSuccess == -1)
			{
				return repeatDelaySecondsFailed != -1;
			}
			return true;
		}
	}

	public static void PlayerDisconnected(BasePlayer player)
	{
		if (player.IsNpc)
		{
			return;
		}
		int activeMission = player.GetActiveMission();
		if (activeMission != -1 && activeMission < player.missions.Count)
		{
			MissionInstance missionInstance = player.missions[activeMission];
			BaseMission mission = missionInstance.GetMission();
			if (mission.missionEntities.Length != 0)
			{
				mission.MissionFailed(missionInstance, player, MissionFailReason.Disconnect);
			}
		}
	}

	public static void PlayerKilled(BasePlayer player)
	{
	}

	public virtual Sprite GetIcon(MissionInstance instance)
	{
		return icon;
	}

	public virtual void SetupPositions(MissionInstance instance, BasePlayer assignee)
	{
		PositionGenerator[] array = positionGenerators;
		foreach (PositionGenerator positionGenerator in array)
		{
			instance.missionPoints.Add(positionGenerator.GetIdentifier(), positionGenerator.GetPosition(assignee));
		}
	}

	public void AddBlockers(MissionInstance instance)
	{
		foreach (KeyValuePair<string, Vector3> missionPoint in instance.missionPoints)
		{
			if (!blockedPoints.Contains(missionPoint.Value))
			{
				blockedPoints.Add(missionPoint.Value);
			}
		}
	}

	public void RemoveBlockers(MissionInstance instance)
	{
		foreach (KeyValuePair<string, Vector3> missionPoint in instance.missionPoints)
		{
			if (blockedPoints.Contains(missionPoint.Value))
			{
				blockedPoints.Remove(missionPoint.Value);
			}
		}
	}

	public virtual void SetupRewards(MissionInstance instance, BasePlayer assignee)
	{
		if (baseRewards.Length != 0)
		{
			instance.rewards = new ItemAmount[baseRewards.Length];
			for (int i = 0; i < baseRewards.Length; i++)
			{
				instance.rewards[i] = new ItemAmount(baseRewards[i].itemDef, baseRewards[i].amount);
			}
		}
	}

	public static void DoMissionEffect(string effectString, BasePlayer assignee)
	{
		Effect effect = new Effect();
		effect.Init(Effect.Type.Generic, assignee, StringPool.Get("head"), Vector3.zero, Vector3.forward);
		effect.pooledString = effectString;
		EffectNetwork.Send(effect, assignee.net.connection);
	}

	public virtual void MissionStart(MissionInstance instance, BasePlayer assignee)
	{
		if (Interface.CallHook("OnMissionStart", this, instance, assignee) != null)
		{
			return;
		}
		SetupRewards(instance, assignee);
		SetupPositions(instance, assignee);
		AddBlockers(instance);
		for (int i = 0; i < objectives.Length; i++)
		{
			objectives[i].Get().MissionStarted(i, instance);
		}
		DoMissionEffect(acceptEffect.resourcePath, assignee);
		MissionEntityEntry[] array = missionEntities;
		foreach (MissionEntityEntry missionEntityEntry in array)
		{
			if (missionEntityEntry.entityRef.isValid)
			{
				Vector3 missionPoint = instance.GetMissionPoint(missionEntityEntry.spawnPositionToUse, assignee);
				BaseEntity baseEntity = GameManager.server.CreateEntity(missionEntityEntry.entityRef.resourcePath, missionPoint, Quaternion.identity);
				MissionEntity missionEntity = baseEntity.gameObject.AddComponent<MissionEntity>();
				missionEntity.Setup(assignee, instance, missionEntityEntry.cleanupOnMissionSuccess, missionEntityEntry.cleanupOnMissionFailed);
				instance.createdEntities.Add(missionEntity);
				baseEntity.Spawn();
			}
		}
		foreach (MissionEntity createdEntity in instance.createdEntities)
		{
			createdEntity.MissionStarted(assignee, instance);
		}
		Interface.CallHook("OnMissionStarted", this, instance, assignee);
	}

	public void CheckObjectives(MissionInstance instance, BasePlayer assignee)
	{
		bool flag = true;
		for (int i = 0; i < objectives.Length; i++)
		{
			if (!instance.objectiveStatuses[i].completed || instance.objectiveStatuses[i].failed)
			{
				flag = false;
			}
		}
		if (flag && instance.status == MissionStatus.Active)
		{
			MissionSuccess(instance, assignee);
		}
	}

	public virtual void Think(MissionInstance instance, BasePlayer assignee, float delta)
	{
		for (int i = 0; i < objectives.Length; i++)
		{
			objectives[i].Get().Think(i, instance, assignee, delta);
		}
		CheckObjectives(instance, assignee);
	}

	public virtual void MissionComplete(MissionInstance instance, BasePlayer assignee)
	{
		DoMissionEffect(victoryEffect.resourcePath, assignee);
		assignee.ChatMessage("You have completed the mission : " + missionName.english);
		if (instance.rewards != null && instance.rewards.Length != 0)
		{
			ItemAmount[] rewards = instance.rewards;
			foreach (ItemAmount itemAmount in rewards)
			{
				if (itemAmount.itemDef == null || itemAmount.amount == 0f)
				{
					Debug.LogError("BIG REWARD SCREWUP, NULL ITEM DEF");
				}
				Item item = ItemManager.Create(itemAmount.itemDef, Mathf.CeilToInt(itemAmount.amount), 0uL);
				if (item != null)
				{
					assignee.GiveItem(item, BaseEntity.GiveItemReason.PickedUp);
				}
			}
		}
		Facepunch.Rust.Analytics.Server.MissionComplete(this);
		instance.status = MissionStatus.Completed;
		assignee.SetActiveMission(-1);
		assignee.MissionDirty();
		if (Rust.GameInfo.HasAchievements)
		{
			assignee.stats.Add("missions_completed", 1, Stats.All);
			assignee.stats.Save(forceSteamSave: true);
		}
	}

	public virtual void MissionSuccess(MissionInstance instance, BasePlayer assignee)
	{
		instance.status = MissionStatus.Accomplished;
		MissionEnded(instance, assignee);
		MissionComplete(instance, assignee);
		Interface.CallHook("OnMissionSucceeded", this, instance, assignee);
	}

	public virtual void MissionFailed(MissionInstance instance, BasePlayer assignee, MissionFailReason failReason)
	{
		assignee.ChatMessage("You have failed the mission : " + missionName.english);
		DoMissionEffect(failedEffect.resourcePath, assignee);
		Facepunch.Rust.Analytics.Server.MissionFailed(this, failReason);
		instance.status = MissionStatus.Failed;
		MissionEnded(instance, assignee);
		Interface.CallHook("OnMissionFailed", this, instance, assignee, failReason);
	}

	public virtual void MissionEnded(MissionInstance instance, BasePlayer assignee)
	{
		if (instance.createdEntities != null)
		{
			for (int num = instance.createdEntities.Count - 1; num >= 0; num--)
			{
				MissionEntity missionEntity = instance.createdEntities[num];
				if (!(missionEntity == null))
				{
					missionEntity.MissionEnded(assignee, instance);
				}
			}
		}
		RemoveBlockers(instance);
		instance.endTime = Time.time;
		assignee.SetActiveMission(-1);
		assignee.MissionDirty();
	}

	public void OnObjectiveCompleted(int objectiveIndex, MissionInstance instance, BasePlayer playerFor)
	{
		MissionObjectiveEntry missionObjectiveEntry = objectives[objectiveIndex];
		if (missionObjectiveEntry.autoCompleteOtherObjectives.Length != 0)
		{
			int[] autoCompleteOtherObjectives = missionObjectiveEntry.autoCompleteOtherObjectives;
			foreach (int num in autoCompleteOtherObjectives)
			{
				MissionObjectiveEntry missionObjectiveEntry2 = objectives[num];
				if (!instance.objectiveStatuses[num].completed)
				{
					missionObjectiveEntry2.objective.CompleteObjective(num, instance, playerFor);
				}
			}
		}
		CheckObjectives(instance, playerFor);
	}

	public static bool AssignMission(BasePlayer assignee, IMissionProvider provider, BaseMission mission)
	{
		if (!missionsenabled)
		{
			return false;
		}
		if (!mission.IsEligableForMission(assignee, provider))
		{
			return false;
		}
		object obj = Interface.CallHook("CanAssignMission", assignee, mission, provider);
		if (obj is bool)
		{
			return (bool)obj;
		}
		MissionInstance missionInstance = Pool.Get<MissionInstance>();
		missionInstance.missionID = mission.id;
		missionInstance.startTime = Time.time;
		missionInstance.providerID = provider.ProviderID();
		missionInstance.status = MissionStatus.Active;
		missionInstance.createdEntities = Pool.GetList<MissionEntity>();
		missionInstance.objectiveStatuses = new MissionInstance.ObjectiveStatus[mission.objectives.Length];
		for (int i = 0; i < mission.objectives.Length; i++)
		{
			missionInstance.objectiveStatuses[i] = new MissionInstance.ObjectiveStatus();
		}
		assignee.AddMission(missionInstance);
		mission.MissionStart(missionInstance, assignee);
		assignee.SetActiveMission(assignee.missions.Count - 1);
		assignee.MissionDirty();
		Interface.CallHook("OnMissionAssigned", mission, provider, assignee);
		return true;
	}

	public bool IsEligableForMission(BasePlayer player, IMissionProvider provider)
	{
		if (!missionsenabled)
		{
			return false;
		}
		foreach (MissionInstance mission in player.missions)
		{
			if (mission.status == MissionStatus.Accomplished || mission.status == MissionStatus.Active)
			{
				return false;
			}
		}
		return true;
	}
}
