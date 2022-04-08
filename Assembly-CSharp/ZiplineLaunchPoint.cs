#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ZiplineLaunchPoint : BaseEntity
{
	public Transform LineDeparturePoint;

	public LineRenderer ZiplineRenderer;

	public Collider MountCollider;

	public BoxCollider BuildingBlock;

	public GameObjectRef MountableRef;

	public float LineSlackAmount = 2f;

	public bool RegenLine;

	private List<Vector3> ziplineTargets = new List<Vector3>();

	private List<Vector3> linePoints;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ZiplineLaunchPoint.OnRpcMessage"))
		{
			if (rpc == 2256922575u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - MountPlayer "));
				}
				using (TimeWarning.New("MountPlayer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2256922575u, "MountPlayer", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2256922575u, "MountPlayer", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							MountPlayer(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in MountPlayer");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
		ziplineTargets.Clear();
		linePoints = null;
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		FindZiplineTarget(ref ziplineTargets);
		CalculateZiplinePoints(ziplineTargets, ref linePoints);
		if (ziplineTargets.Count == 0)
		{
			Kill();
		}
		else
		{
			SendNetworkUpdate();
		}
	}

	private void FindZiplineTarget(ref List<Vector3> foundPositions)
	{
		foundPositions.Clear();
		Vector3 position = LineDeparturePoint.position;
		List<ZiplineTarget> list = Facepunch.Pool.GetList<ZiplineTarget>();
		GamePhysics.OverlapSphere(position + base.transform.forward * 200f, 200f, list, 1218511105);
		ZiplineTarget ziplineTarget = null;
		float num = float.MaxValue;
		float num2 = 0f;
		foreach (ZiplineTarget item in list)
		{
			if (item.IsChainPoint)
			{
				continue;
			}
			Vector3 position2 = item.transform.position;
			float num3 = Vector3.Dot((position2.WithY(position.y) - position).normalized, base.transform.forward);
			float num4 = Vector3.Distance(position, position2);
			if (!(num3 > 0.2f) || !item.IsValidPosition(position) || !(position.y > position2.y + num2) || !(num4 > 10f) || !(num4 < num))
			{
				continue;
			}
			if (CheckLineOfSight(position, position2))
			{
				num = num4;
				ziplineTarget = item;
				foundPositions.Clear();
				foundPositions.Add(ziplineTarget.transform.position);
				continue;
			}
			foreach (ZiplineTarget item2 in list)
			{
				if (item2.IsChainPoint && item2.IsValidPosition(position) && !item2.IsValidPosition(position2))
				{
					bool num5 = CheckLineOfSight(position, item2.transform.position);
					bool flag = CheckLineOfSight(item2.transform.position, position2);
					if (num5 && flag)
					{
						num = num4;
						ziplineTarget = item;
						foundPositions.Clear();
						foundPositions.Add(item2.transform.position);
						foundPositions.Add(ziplineTarget.transform.position);
					}
				}
			}
		}
	}

	private bool CheckLineOfSight(Vector3 from, Vector3 to)
	{
		Vector3 vector = CalculateLineMidPoint(from, to) - Vector3.up * 0.5f;
		if (GamePhysics.LineOfSightRadius(from, to, 1218511105, 0.5f, 2f) && GamePhysics.LineOfSightRadius(from, vector, 1218511105, 0.5f, 2f))
		{
			return GamePhysics.LineOfSightRadius(vector, to, 1218511105, 0.5f, 2f);
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(2uL)]
	private void MountPlayer(RPCMessage msg)
	{
		if (IsBusy() || msg.player == null || msg.player.Distance(LineDeparturePoint.position) > 3f || !IsPlayerFacingValidDirection(msg.player) || ziplineTargets.Count == 0)
		{
			return;
		}
		Vector3 position = LineDeparturePoint.position;
		Quaternion startRot = Quaternion.LookRotation((ziplineTargets[0].WithY(position.y) - position).normalized);
		ZiplineMountable ziplineMountable = base.gameManager.CreateEntity(MountableRef.resourcePath, msg.player.transform.position + Vector3.up * 2f, msg.player.eyes.rotation) as ZiplineMountable;
		if (ziplineMountable != null)
		{
			CalculateZiplinePoints(ziplineTargets, ref linePoints);
			ziplineMountable.SetDestination(linePoints, position, startRot);
			ziplineMountable.Spawn();
			ziplineMountable.MountPlayer(msg.player);
			if (msg.player.GetMounted() != ziplineMountable)
			{
				ziplineMountable.Kill();
			}
			SetFlag(Flags.Busy, true);
			Invoke(ClearBusy, 2f);
		}
	}

	private void ClearBusy()
	{
		SetFlag(Flags.Busy, false);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.zipline == null)
		{
			info.msg.zipline = Facepunch.Pool.Get<Zipline>();
		}
		info.msg.zipline.destinationPoints = Facepunch.Pool.GetList<VectorData>();
		foreach (Vector3 ziplineTarget in ziplineTargets)
		{
			info.msg.zipline.destinationPoints.Add(new VectorData(ziplineTarget.x, ziplineTarget.y, ziplineTarget.z));
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.zipline == null)
		{
			return;
		}
		ziplineTargets.Clear();
		foreach (VectorData destinationPoint in info.msg.zipline.destinationPoints)
		{
			ziplineTargets.Add(destinationPoint);
		}
	}

	private void CalculateZiplinePoints(List<Vector3> targets, ref List<Vector3> points)
	{
		if (points == null && targets.Count != 0)
		{
			Vector3[] array = new Vector3[targets.Count + 1];
			array[0] = LineDeparturePoint.position;
			for (int i = 0; i < targets.Count; i++)
			{
				array[i + 1] = targets[i];
			}
			float[] array2 = new float[array.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = LineSlackAmount;
			}
			points = Facepunch.Pool.GetList<Vector3>();
			Bezier.ApplyLineSlack(array, array2, ref points, 25);
		}
	}

	private Vector3 CalculateLineMidPoint(Vector3 start, Vector3 endPoint)
	{
		Vector3 result = Vector3.Lerp(start, endPoint, 0.5f);
		result.y -= LineSlackAmount;
		return result;
	}

	private void UpdateBuildingBlocks()
	{
		BuildingBlock.gameObject.SetActive(false);
		if (ziplineTargets.Count > 0)
		{
			_003CUpdateBuildingBlocks_003Eg__SetUpBuildingBlock_007C20_0(BuildingBlock, linePoints);
		}
	}

	private bool IsPlayerFacingValidDirection(BasePlayer ply)
	{
		return Vector3.Dot(ply.eyes.HeadForward(), base.transform.forward) > 0.2f;
	}
}
