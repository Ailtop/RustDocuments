using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;

public class TreeEntity : ResourceEntity, IPrefabPreProcess
{
	[Header("Falling")]
	public bool fallOnKilled = true;

	public float fallDuration = 1.5f;

	public GameObjectRef fallStartSound;

	public GameObjectRef fallImpactSound;

	public GameObjectRef fallImpactParticles;

	public SoundDefinition fallLeavesLoopDef;

	[NonSerialized]
	public bool[] usedHeights = new bool[20];

	public bool impactSoundPlayed;

	private float treeDistanceUponFalling;

	public GameObjectRef prefab;

	public bool hasBonusGame = true;

	public GameObjectRef bonusHitEffect;

	public GameObjectRef bonusHitSound;

	public Collider serverCollider;

	public Collider clientCollider;

	public SoundDefinition smallCrackSoundDef;

	public SoundDefinition medCrackSoundDef;

	private float lastAttackDamage;

	[NonSerialized]
	public BaseEntity xMarker;

	private int currentBonusLevel;

	private float lastDirection = -1f;

	private float lastHitTime;

	private int lastHitMarkerIndex = -1;

	private float nextBirdTime;

	private uint birdCycleIndex;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TreeEntity.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override float BoundsPadding()
	{
		return 1f;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		lastDirection = ((UnityEngine.Random.Range(0, 2) != 0) ? 1 : (-1));
		TreeManager.OnTreeSpawned(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		CleanupMarker();
		TreeManager.OnTreeDestroyed(this);
	}

	public bool DidHitMarker(HitInfo info)
	{
		if (xMarker == null)
		{
			return false;
		}
		object obj = Interface.CallHook("OnTreeMarkerHit", this, info);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (PrefabAttribute.server.Find<TreeMarkerData>(prefabID) != null)
		{
			if (new Bounds(xMarker.transform.position, Vector3.one * 0.2f).Contains(info.HitPositionWorld))
			{
				return true;
			}
		}
		else
		{
			Vector3 lhs = Vector3Ex.Direction2D(base.transform.position, xMarker.transform.position);
			Vector3 attackNormal = info.attackNormal;
			float num = Vector3.Dot(lhs, attackNormal);
			float num2 = Vector3.Distance(xMarker.transform.position, info.HitPositionWorld);
			if (num >= 0.3f && num2 <= 0.2f)
			{
				return true;
			}
		}
		return false;
	}

	public void StartBonusGame()
	{
		if (IsInvoking(StopBonusGame))
		{
			CancelInvoke(StopBonusGame);
		}
		Invoke(StopBonusGame, 60f);
	}

	public void StopBonusGame()
	{
		CleanupMarker();
		lastHitTime = 0f;
		currentBonusLevel = 0;
	}

	public bool BonusActive()
	{
		return xMarker != null;
	}

	private void DoBirds()
	{
		if (!base.isClient && !(UnityEngine.Time.realtimeSinceStartup < nextBirdTime) && !(bounds.extents.y < 6f))
		{
			uint seed = (uint)(int)net.ID.Value + birdCycleIndex;
			if (SeedRandom.Range(ref seed, 0, 2) == 0)
			{
				Effect.server.Run("assets/prefabs/npc/birds/birdemission.prefab", base.transform.position + Vector3.up * UnityEngine.Random.Range(bounds.extents.y * 0.65f, bounds.extents.y * 0.9f), Vector3.up);
			}
			birdCycleIndex++;
			nextBirdTime = UnityEngine.Time.realtimeSinceStartup + 90f;
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		bool canGather = info.CanGather;
		float num = UnityEngine.Time.time - lastHitTime;
		lastHitTime = UnityEngine.Time.time;
		DoBirds();
		if (!hasBonusGame || !canGather || info.Initiator == null || (BonusActive() && !DidHitMarker(info)))
		{
			base.OnAttacked(info);
			return;
		}
		if (xMarker != null && !info.DidGather && info.gatherScale > 0f)
		{
			xMarker.ClientRPC(null, "MarkerHit", currentBonusLevel);
			currentBonusLevel++;
			info.gatherScale = 1f + Mathf.Clamp((float)currentBonusLevel * 0.125f, 0f, 1f);
		}
		Vector3 vector = ((xMarker != null) ? xMarker.transform.position : info.HitPositionWorld);
		CleanupMarker();
		TreeMarkerData treeMarkerData = PrefabAttribute.server.Find<TreeMarkerData>(prefabID);
		if (treeMarkerData != null)
		{
			Vector3 nearbyPoint = treeMarkerData.GetNearbyPoint(base.transform.InverseTransformPoint(vector), ref lastHitMarkerIndex, out var normal);
			nearbyPoint = base.transform.TransformPoint(nearbyPoint);
			Quaternion rot = QuaternionEx.LookRotationNormal(base.transform.TransformDirection(normal));
			xMarker = GameManager.server.CreateEntity("assets/content/nature/treesprefabs/trees/effects/tree_marking_nospherecast.prefab", nearbyPoint, rot);
		}
		else
		{
			Vector3 vector2 = Vector3Ex.Direction2D(base.transform.position, vector);
			Vector3 vector3 = Vector3.Cross(vector2, Vector3.up);
			float num2 = lastDirection;
			Vector3 vector4 = Vector3.Lerp(t: UnityEngine.Random.Range(0.5f, 0.5f), a: -vector2, b: vector3 * num2);
			Vector3 position = base.transform.InverseTransformDirection(vector4.normalized) * 2.5f;
			position = base.transform.InverseTransformPoint(GetCollider().ClosestPoint(base.transform.TransformPoint(position)));
			Vector3 aimFrom = base.transform.TransformPoint(position);
			position.y = base.transform.InverseTransformPoint(info.HitPositionWorld).y;
			Vector3 vector5 = base.transform.InverseTransformPoint(info.Initiator.CenterPoint());
			float min = Mathf.Max(0.75f, vector5.y);
			float max = vector5.y + 0.5f;
			position.y = Mathf.Clamp(position.y + UnityEngine.Random.Range(0.1f, 0.2f) * ((UnityEngine.Random.Range(0, 2) == 0) ? (-1f) : 1f), min, max);
			Vector3 vector6 = Vector3Ex.Direction2D(base.transform.position, aimFrom);
			Vector3 vector7 = vector6;
			vector6 = base.transform.InverseTransformDirection(vector6);
			Quaternion quaternion = QuaternionEx.LookRotationNormal(-vector6, Vector3.zero);
			position = base.transform.TransformPoint(position);
			quaternion = QuaternionEx.LookRotationNormal(-vector7, Vector3.zero);
			position = GetCollider().ClosestPoint(position);
			quaternion = QuaternionEx.LookRotationNormal(-Vector3Ex.Direction(new Line(GetCollider().transform.TransformPoint(new Vector3(0f, 10f, 0f)), GetCollider().transform.TransformPoint(new Vector3(0f, -10f, 0f))).ClosestPoint(position), position));
			xMarker = GameManager.server.CreateEntity("assets/content/nature/treesprefabs/trees/effects/tree_marking.prefab", position, quaternion);
		}
		xMarker.Spawn();
		if (num > 5f)
		{
			StartBonusGame();
		}
		base.OnAttacked(info);
		if (health > 0f)
		{
			lastAttackDamage = info.damageTypes.Total();
			int num3 = Mathf.CeilToInt(health / lastAttackDamage);
			if (num3 < 2)
			{
				ClientRPC(null, "CrackSound", 1);
			}
			else if (num3 < 5)
			{
				ClientRPC(null, "CrackSound", 0);
			}
		}
	}

	public void CleanupMarker()
	{
		if ((bool)xMarker)
		{
			xMarker.Kill();
		}
		xMarker = null;
	}

	public Collider GetCollider()
	{
		if (base.isServer)
		{
			if (!(serverCollider == null))
			{
				return serverCollider;
			}
			return GetComponentInChildren<CapsuleCollider>();
		}
		if (!(clientCollider == null))
		{
			return clientCollider;
		}
		return GetComponent<Collider>();
	}

	public override void OnKilled(HitInfo info)
	{
		if (isKilled)
		{
			return;
		}
		isKilled = true;
		CleanupMarker();
		Facepunch.Rust.Analytics.Server.TreeKilled(info.WeaponPrefab);
		if (fallOnKilled)
		{
			Collider collider = GetCollider();
			if ((bool)collider)
			{
				collider.enabled = false;
			}
			Vector3 vector = info.attackNormal;
			if (vector == Vector3.zero)
			{
				vector = Vector3Ex.Direction2D(base.transform.position, info.PointStart);
			}
			ClientRPC(null, "TreeFall", vector);
			Invoke(DelayedKill, fallDuration + 1f);
		}
		else
		{
			DelayedKill();
		}
	}

	public void DelayedKill()
	{
		Kill();
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (serverside)
		{
			globalBroadcast = ConVar.Tree.global_broadcast;
		}
	}
}
