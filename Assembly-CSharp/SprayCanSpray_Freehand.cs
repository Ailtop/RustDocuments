#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SprayCanSpray_Freehand : SprayCanSpray
{
	public AlignedLineDrawer LineDrawer;

	public List<AlignedLineDrawer.LinePoint> LinePoints = new List<AlignedLineDrawer.LinePoint>();

	public Color colour = Color.white;

	public float width;

	public EntityRef<BasePlayer> editingPlayer;

	public GroundWatch groundWatch;

	public MeshCollider meshCollider;

	public const int MaxLinePointLength = 60;

	public const float SimplifyTolerance = 0.008f;

	private bool AcceptingChanges => editingPlayer.IsValid(serverside: true);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SprayCanSpray_Freehand.OnRpcMessage"))
		{
			if (rpc == 2020094435 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_AddPointMidSpray ");
				}
				using (TimeWarning.New("Server_AddPointMidSpray"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							Server_AddPointMidSpray(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_AddPointMidSpray");
					}
				}
				return true;
			}
			if (rpc == 117883393 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_FinishEditing ");
				}
				using (TimeWarning.New("Server_FinishEditing"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							Server_FinishEditing(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Server_FinishEditing");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (LinePoints == null || LinePoints.Count == 0)
		{
			Kill();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.sprayLine == null)
		{
			info.msg.sprayLine = Facepunch.Pool.Get<SprayLine>();
		}
		if (info.msg.sprayLine.linePoints == null)
		{
			info.msg.sprayLine.linePoints = Facepunch.Pool.GetList<LinePoint>();
		}
		bool flag = AcceptingChanges && info.forDisk;
		if (LinePoints != null && !flag)
		{
			CopyPoints(LinePoints, info.msg.sprayLine.linePoints);
		}
		info.msg.sprayLine.width = width;
		info.msg.sprayLine.colour = new Vector3(colour.r, colour.g, colour.b);
		if (!info.forDisk)
		{
			info.msg.sprayLine.editingPlayer = editingPlayer.uid;
		}
	}

	public void SetColour(Color newColour)
	{
		colour = newColour;
	}

	public void SetWidth(float lineWidth)
	{
		width = lineWidth;
	}

	[RPC_Server]
	private void Server_AddPointMidSpray(RPCMessage msg)
	{
		if (AcceptingChanges && !(editingPlayer.Get(serverside: true) != msg.player) && LinePoints.Count + 1 <= 60)
		{
			Vector3 vector = msg.read.Vector3();
			Vector3 worldNormal = msg.read.Vector3();
			if (!(Vector3.Distance(vector, LinePoints[0].LocalPosition) >= 10f))
			{
				LinePoints.Add(new AlignedLineDrawer.LinePoint
				{
					LocalPosition = vector,
					WorldNormal = worldNormal
				});
				UpdateGroundWatch();
				SendNetworkUpdate();
			}
		}
	}

	public void EnableChanges(BasePlayer byPlayer)
	{
		base.OwnerID = byPlayer.userID;
		editingPlayer.Set(byPlayer);
		Invoke(TimeoutEditing, 30f);
	}

	public void TimeoutEditing()
	{
		if (editingPlayer.IsSet)
		{
			editingPlayer.Set(null);
			SendNetworkUpdate();
			Kill();
		}
	}

	[RPC_Server]
	private void Server_FinishEditing(RPCMessage msg)
	{
		BasePlayer basePlayer = editingPlayer.Get(serverside: true);
		if (msg.player != basePlayer)
		{
			return;
		}
		bool allowNewSprayImmediately = msg.read.Int32() == 1;
		if (basePlayer != null && basePlayer.GetHeldEntity() != null && basePlayer.GetHeldEntity() is SprayCan sprayCan)
		{
			sprayCan.ClearPaintingLine(allowNewSprayImmediately);
		}
		editingPlayer.Set(null);
		SprayList obj = SprayList.Deserialize(msg.read);
		int count = obj.linePoints.Count;
		if (count > 70)
		{
			Kill();
			Facepunch.Pool.FreeList(ref obj.linePoints);
			Facepunch.Pool.Free(ref obj);
			return;
		}
		if (LinePoints.Count <= 1)
		{
			Kill();
			Facepunch.Pool.FreeList(ref obj.linePoints);
			Facepunch.Pool.Free(ref obj);
			return;
		}
		CancelInvoke(TimeoutEditing);
		LinePoints.Clear();
		for (int i = 0; i < count; i++)
		{
			if (obj.linePoints[i].localPosition.sqrMagnitude < 100f)
			{
				LinePoints.Add(new AlignedLineDrawer.LinePoint
				{
					LocalPosition = obj.linePoints[i].localPosition,
					WorldNormal = obj.linePoints[i].worldNormal
				});
			}
		}
		OnDeployed(null, basePlayer, null);
		UpdateGroundWatch();
		Facepunch.Pool.FreeList(ref obj.linePoints);
		Facepunch.Pool.Free(ref obj);
		SendNetworkUpdate();
	}

	public void AddInitialPoint(Vector3 atNormal)
	{
		LinePoints = new List<AlignedLineDrawer.LinePoint>
		{
			new AlignedLineDrawer.LinePoint
			{
				LocalPosition = Vector3.zero,
				WorldNormal = atNormal
			}
		};
	}

	private void UpdateGroundWatch()
	{
		if (base.isServer && LinePoints.Count > 1)
		{
			Vector3 groundPosition = Vector3.Lerp(LinePoints[0].LocalPosition, LinePoints[LinePoints.Count - 1].LocalPosition, 0.5f);
			if (groundWatch != null)
			{
				groundWatch.groundPosition = groundPosition;
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.sprayLine != null)
		{
			if (info.msg.sprayLine.linePoints != null)
			{
				LinePoints.Clear();
				CopyPoints(info.msg.sprayLine.linePoints, LinePoints);
			}
			colour = new Color(info.msg.sprayLine.colour.x, info.msg.sprayLine.colour.y, info.msg.sprayLine.colour.z);
			width = info.msg.sprayLine.width;
			editingPlayer.uid = info.msg.sprayLine.editingPlayer;
			UpdateGroundWatch();
		}
	}

	public void CopyPoints(List<AlignedLineDrawer.LinePoint> from, List<LinePoint> to)
	{
		to.Clear();
		foreach (AlignedLineDrawer.LinePoint item in from)
		{
			LinePoint linePoint = Facepunch.Pool.Get<LinePoint>();
			linePoint.localPosition = item.LocalPosition;
			linePoint.worldNormal = item.WorldNormal;
			to.Add(linePoint);
		}
	}

	public void CopyPoints(List<AlignedLineDrawer.LinePoint> from, List<Vector3> to)
	{
		to.Clear();
		foreach (AlignedLineDrawer.LinePoint item in from)
		{
			to.Add(item.LocalPosition);
			to.Add(item.WorldNormal);
		}
	}

	public void CopyPoints(List<LinePoint> from, List<AlignedLineDrawer.LinePoint> to)
	{
		to.Clear();
		foreach (LinePoint item in from)
		{
			to.Add(new AlignedLineDrawer.LinePoint
			{
				LocalPosition = item.localPosition,
				WorldNormal = item.worldNormal
			});
		}
	}

	public static void CopyPoints(List<AlignedLineDrawer.LinePoint> from, List<AlignedLineDrawer.LinePoint> to)
	{
		to.Clear();
		foreach (AlignedLineDrawer.LinePoint item in from)
		{
			to.Add(item);
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		editingPlayer.Set(null);
	}
}
