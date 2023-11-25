#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class AdvancedChristmasLights : IOEntity
{
	public struct pointEntry
	{
		public Vector3 point;

		public Vector3 normal;
	}

	public enum AnimationType
	{
		ON = 1,
		FLASHING = 2,
		CHASING = 3,
		FADE = 4,
		SLOWGLOW = 6
	}

	public GameObjectRef bulbPrefab;

	public LineRenderer lineRenderer;

	public List<pointEntry> points = new List<pointEntry>();

	public List<BaseBulb> bulbs = new List<BaseBulb>();

	public float bulbSpacing = 0.25f;

	public float wireThickness = 0.02f;

	public Transform wireEmission;

	public AnimationType animationStyle = AnimationType.ON;

	public RendererLOD _lod;

	[Tooltip("This many units used will result in +1 power usage")]
	public float lengthToPowerRatio = 5f;

	private bool finalized;

	private int lengthUsed;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AdvancedChristmasLights.OnRpcMessage"))
		{
			if (rpc == 1435781224 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetAnimationStyle ");
				}
				using (TimeWarning.New("SetAnimationStyle"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1435781224u, "SetAnimationStyle", this, player, 3f))
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
							RPCMessage rPCMessage2 = rPCMessage;
							SetAnimationStyle(rPCMessage2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetAnimationStyle");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void ClearPoints()
	{
		points.Clear();
	}

	public void FinishEditing()
	{
		finalized = true;
	}

	public bool IsFinalized()
	{
		return finalized;
	}

	public void AddPoint(Vector3 newPoint, Vector3 newNormal)
	{
		if (base.isServer && points.Count == 0)
		{
			newPoint = wireEmission.position;
		}
		pointEntry item = default(pointEntry);
		item.point = newPoint;
		item.normal = newNormal;
		points.Add(item);
		if (base.isServer)
		{
			SendNetworkUpdate();
		}
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	protected override int GetPickupCount()
	{
		return Mathf.Max(lengthUsed, 1);
	}

	public void AddLengthUsed(int addLength)
	{
		lengthUsed += addLength;
	}

	public override void ServerInit()
	{
		base.ServerInit();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lightString = Facepunch.Pool.Get<LightString>();
		info.msg.lightString.points = Facepunch.Pool.GetList<LightString.StringPoint>();
		info.msg.lightString.lengthUsed = lengthUsed;
		info.msg.lightString.animationStyle = (int)animationStyle;
		foreach (pointEntry point in points)
		{
			LightString.StringPoint stringPoint = Facepunch.Pool.Get<LightString.StringPoint>();
			stringPoint.point = point.point;
			stringPoint.normal = point.normal;
			info.msg.lightString.points.Add(stringPoint);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.lightString == null)
		{
			return;
		}
		ClearPoints();
		foreach (LightString.StringPoint point in info.msg.lightString.points)
		{
			AddPoint(point.point, point.normal);
		}
		lengthUsed = info.msg.lightString.lengthUsed;
		animationStyle = (AnimationType)info.msg.lightString.animationStyle;
		if (info.fromDisk)
		{
			FinishEditing();
		}
	}

	public bool IsStyle(AnimationType testType)
	{
		return testType == animationStyle;
	}

	public bool CanPlayerManipulate(BasePlayer player)
	{
		return true;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SetAnimationStyle(RPCMessage msg)
	{
		int value = msg.read.Int32();
		value = Mathf.Clamp(value, 1, 7);
		if (Global.developer > 0)
		{
			string text = value.ToString();
			int num = (int)animationStyle;
			Debug.Log("Set animation style to :" + text + " old was : " + num);
		}
		AnimationType animationType = (AnimationType)value;
		if (animationType != animationStyle)
		{
			animationStyle = animationType;
			SendNetworkUpdate();
		}
	}
}
