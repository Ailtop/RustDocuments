#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PoweredLightsDeployer : HeldEntity
{
	public GameObjectRef poweredLightsPrefab;

	public EntityRef activeLights;

	public MaterialReplacement guide;

	public GameObject guideObject;

	public float maxPlaceDistance = 5f;

	public float lengthPerAmount = 0.5f;

	public AdvancedChristmasLights active
	{
		get
		{
			BaseEntity baseEntity = activeLights.Get(base.isServer);
			if ((bool)baseEntity)
			{
				return baseEntity.GetComponent<AdvancedChristmasLights>();
			}
			return null;
		}
		set
		{
			activeLights.Set(value);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PoweredLightsDeployer.OnRpcMessage"))
		{
			if (rpc == 447739874 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - AddPoint "));
				}
				using (TimeWarning.New("AddPoint"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(447739874u, "AddPoint", this, player))
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
							AddPoint(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AddPoint");
					}
				}
				return true;
			}
			if (rpc == 1975273522 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Finish "));
				}
				using (TimeWarning.New("Finish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1975273522u, "Finish", this, player))
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
							RPCMessage msg3 = rPCMessage;
							Finish(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Finish");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static bool CanPlayerUse(BasePlayer player)
	{
		if (player.CanBuild())
		{
			return !GamePhysics.CheckSphere(player.eyes.position, 0.1f, 536870912, QueryTriggerInteraction.Collide);
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void AddPoint(RPCMessage msg)
	{
		Vector3 vector = msg.read.Vector3();
		Vector3 vector2 = msg.read.Vector3();
		BasePlayer player = msg.player;
		if (GetItem() == null || GetItem().amount < 1 || !IsVisible(vector) || !CanPlayerUse(player) || Vector3.Distance(vector, player.eyes.position) > maxPlaceDistance)
		{
			return;
		}
		int num = 1;
		if (active == null)
		{
			AdvancedChristmasLights component = GameManager.server.CreateEntity(poweredLightsPrefab.resourcePath, vector, Quaternion.LookRotation(vector2, player.eyes.HeadUp())).GetComponent<AdvancedChristmasLights>();
			component.Spawn();
			active = component;
			num = 1;
		}
		else
		{
			if (active.IsFinalized())
			{
				return;
			}
			float a = 0f;
			Vector3 vector3 = active.transform.position;
			if (active.points.Count > 0)
			{
				vector3 = active.points[active.points.Count - 1].point;
				a = Vector3.Distance(vector, vector3);
			}
			a = Mathf.Max(a, lengthPerAmount);
			float num2 = (float)GetItem().amount * lengthPerAmount;
			if (a > num2)
			{
				a = num2;
				vector = vector3 + Vector3Ex.Direction(vector, vector3) * a;
			}
			a = Mathf.Min(num2, a);
			num = Mathf.CeilToInt(a / lengthPerAmount);
		}
		active.AddPoint(vector, vector2);
		SetFlag(Flags.Reserved8, active != null);
		int iAmount = num;
		UseItemAmount(iAmount);
		active.AddLengthUsed(num);
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void Finish(RPCMessage msg)
	{
		DoFinish();
	}

	public void DoFinish()
	{
		if ((bool)active)
		{
			active.FinishEditing();
		}
		active = null;
		SendNetworkUpdate();
	}

	public override void OnHeldChanged()
	{
		DoFinish();
		active = null;
		base.OnHeldChanged();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.lightDeployer = Facepunch.Pool.Get<LightDeployer>();
			info.msg.lightDeployer.active = activeLights.uid;
		}
	}
}
