#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class WireTool : HeldEntity
{
	public enum WireColour
	{
		Default = 0,
		Red = 1,
		Green = 2,
		Blue = 3,
		Yellow = 4,
		Pink = 5,
		Purple = 6,
		Orange = 7,
		White = 8,
		LightBlue = 9,
		Count = 10
	}

	public struct PendingPlug_t
	{
		public IOEntity ent;

		public bool input;

		public int index;

		public GameObject tempLine;
	}

	public Sprite InputSprite;

	public Sprite OutputSprite;

	public Sprite ClearSprite;

	public static float maxWireLength = 30f;

	private const int maxLineNodes = 16;

	public GameObjectRef plugEffect;

	public SoundDefinition clearStartSoundDef;

	public SoundDefinition clearSoundDef;

	public GameObjectRef ioLine;

	public IOEntity.IOType wireType;

	public float RadialMenuHoldTime = 0.25f;

	public float disconnectDelay = 0.15f;

	public float clearDelay = 0.65f;

	private const float IndustrialWallOffset = 0.04f;

	public static Translate.Phrase Default = new Translate.Phrase("wiretoolcolour.default", "Default");

	public static Translate.Phrase DefaultDesc = new Translate.Phrase("wiretoolcolour.default.desc", "Default connection color");

	public static Translate.Phrase Red = new Translate.Phrase("wiretoolcolour.red", "Red");

	public static Translate.Phrase RedDesc = new Translate.Phrase("wiretoolcolour.red.desc", "Red connection color");

	public static Translate.Phrase Green = new Translate.Phrase("wiretoolcolour.green", "Green");

	public static Translate.Phrase GreenDesc = new Translate.Phrase("wiretoolcolour.green.desc", "Green connection color");

	public static Translate.Phrase Blue = new Translate.Phrase("wiretoolcolour.blue", "Blue");

	public static Translate.Phrase BlueDesc = new Translate.Phrase("wiretoolcolour.blue.desc", "Blue connection color");

	public static Translate.Phrase Yellow = new Translate.Phrase("wiretoolcolour.yellow", "Yellow");

	public static Translate.Phrase YellowDesc = new Translate.Phrase("wiretoolcolour.yellow.desc", "Yellow connection color");

	public static Translate.Phrase LightBlue = new Translate.Phrase("wiretoolcolour.light_blue", "Light Blue");

	public static Translate.Phrase LightBlueDesc = new Translate.Phrase("wiretoolcolour.light_blue.desc", "Light Blue connection color");

	public static Translate.Phrase Orange = new Translate.Phrase("wiretoolcolour.orange", "Orange");

	public static Translate.Phrase OrangeDesc = new Translate.Phrase("wiretoolcolour.orange.desc", "Orange connection color");

	public static Translate.Phrase Purple = new Translate.Phrase("wiretoolcolour.purple", "Purple");

	public static Translate.Phrase PurpleDesc = new Translate.Phrase("wiretoolcolour.purple.desc", "Purple connection color");

	public static Translate.Phrase White = new Translate.Phrase("wiretoolcolour.white", "White");

	public static Translate.Phrase WhiteDesc = new Translate.Phrase("wiretoolcolour.white.desc", "White connection color");

	public static Translate.Phrase Pink = new Translate.Phrase("wiretoolcolour.pink", "Pink");

	public static Translate.Phrase PinkDesc = new Translate.Phrase("wiretoolcolour.pink.desc", "Pink connection color");

	public PendingPlug_t pending;

	private const float IndustrialThickness = 0.01f;

	public bool CanChangeColours
	{
		get
		{
			if (wireType != 0 && wireType != IOEntity.IOType.Fluidic)
			{
				return wireType == IOEntity.IOType.Industrial;
			}
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WireTool.OnRpcMessage"))
		{
			if (rpc == 40328523 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - MakeConnection ");
				}
				using (TimeWarning.New("MakeConnection"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(40328523u, "MakeConnection", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(40328523u, "MakeConnection", this, player))
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
							MakeConnection(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in MakeConnection");
					}
				}
				return true;
			}
			if (rpc == 121409151 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestChangeColor ");
				}
				using (TimeWarning.New("RequestChangeColor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(121409151u, "RequestChangeColor", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(121409151u, "RequestChangeColor", this, player))
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
							RequestChangeColor(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RequestChangeColor");
					}
				}
				return true;
			}
			if (rpc == 2469840259u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestClear ");
				}
				using (TimeWarning.New("RequestClear"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2469840259u, "RequestClear", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(2469840259u, "RequestClear", this, player))
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
							RPCMessage msg4 = rPCMessage;
							RequestClear(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RequestClear");
					}
				}
				return true;
			}
			if (rpc == 2596458392u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetPlugged ");
				}
				using (TimeWarning.New("SetPlugged"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage plugged = rPCMessage;
							SetPlugged(plugged);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in SetPlugged");
					}
				}
				return true;
			}
			if (rpc == 210386477 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - TryClear ");
				}
				using (TimeWarning.New("TryClear"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(210386477u, "TryClear", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(210386477u, "TryClear", this, player))
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
							RPCMessage msg5 = rPCMessage;
							TryClear(msg5);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in TryClear");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void ClearPendingPlug()
	{
		pending.ent = null;
		pending.index = -1;
	}

	public bool HasPendingPlug()
	{
		if (pending.ent != null)
		{
			return pending.index != -1;
		}
		return false;
	}

	public bool PendingPlugIsInput()
	{
		if (pending.ent != null && pending.index != -1)
		{
			return pending.input;
		}
		return false;
	}

	public bool PendingPlugIsType(IOEntity.IOType type)
	{
		if (pending.ent != null && pending.index != -1)
		{
			if (!pending.input || pending.ent.inputs[pending.index].type != type)
			{
				if (!pending.input)
				{
					return pending.ent.outputs[pending.index].type == type;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool PendingPlugIsOutput()
	{
		if (pending.ent != null && pending.index != -1)
		{
			return !pending.input;
		}
		return false;
	}

	public Vector3 PendingPlugWorldPos()
	{
		if (pending.ent == null || pending.index == -1)
		{
			return Vector3.zero;
		}
		if (pending.input)
		{
			return pending.ent.transform.TransformPoint(pending.ent.inputs[pending.index].handlePosition);
		}
		return pending.ent.transform.TransformPoint(pending.ent.outputs[pending.index].handlePosition);
	}

	public static bool CanPlayerUseWires(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseWires", player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		List<Collider> obj2 = Facepunch.Pool.GetList<Collider>();
		GamePhysics.OverlapSphere(player.eyes.position, 0.1f, obj2, 536870912, QueryTriggerInteraction.Collide);
		bool result = obj2.All((Collider collider) => collider.gameObject.CompareTag("IgnoreWireCheck"));
		Facepunch.Pool.FreeList(ref obj2);
		return result;
	}

	public static bool CanModifyEntity(BasePlayer player, IOEntity ent)
	{
		if (player.CanBuild(ent.transform.position, ent.transform.rotation, ent.bounds))
		{
			return ent.AllowWireConnections();
		}
		return false;
	}

	public bool PendingPlugRoot()
	{
		if (pending.ent != null)
		{
			return pending.ent.IsRootEntity();
		}
		return false;
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	[RPC_Server.FromOwner]
	public void TryClear(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		NetworkableId uid = msg.read.EntityID();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		IOEntity iOEntity = ((baseNetworkable == null) ? null : baseNetworkable.GetComponent<IOEntity>());
		if (!(iOEntity == null) && CanPlayerUseWires(player) && CanModifyEntity(player, iOEntity))
		{
			iOEntity.ClearConnections();
			iOEntity.SendNetworkUpdate();
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void MakeConnection(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanPlayerUseWires(player))
		{
			return;
		}
		int num = msg.read.Int32();
		if (num > 18)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < num; i++)
		{
			Vector3 item = msg.read.Vector3();
			list.Add(item);
		}
		NetworkableId uid = msg.read.EntityID();
		int num2 = msg.read.Int32();
		NetworkableId uid2 = msg.read.EntityID();
		int num3 = msg.read.Int32();
		WireColour wireColour = IntToColour(msg.read.Int32());
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		IOEntity iOEntity = ((baseNetworkable == null) ? null : baseNetworkable.GetComponent<IOEntity>());
		if (iOEntity == null)
		{
			return;
		}
		BaseNetworkable baseNetworkable2 = BaseNetworkable.serverEntities.Find(uid2);
		IOEntity iOEntity2 = ((baseNetworkable2 == null) ? null : baseNetworkable2.GetComponent<IOEntity>());
		if (!(iOEntity2 == null) && ValidateLine(list, iOEntity, iOEntity2, player, num3) && !(Vector3.Distance(baseNetworkable2.transform.position, baseNetworkable.transform.position) > maxWireLength) && num2 < iOEntity.inputs.Length && num3 < iOEntity2.outputs.Length && !(iOEntity.inputs[num2].connectedTo.Get() != null) && !(iOEntity2.outputs[num3].connectedTo.Get() != null) && (!iOEntity.inputs[num2].rootConnectionsOnly || iOEntity2.IsRootEntity()) && CanModifyEntity(player, iOEntity) && CanModifyEntity(player, iOEntity2) && Interface.CallHook("OnWireConnect", player, baseNetworkable, num2, baseNetworkable2, num3, list) == null)
		{
			iOEntity.inputs[num2].connectedTo.Set(iOEntity2);
			iOEntity.inputs[num2].connectedToSlot = num3;
			iOEntity.inputs[num2].wireColour = wireColour;
			iOEntity.inputs[num2].connectedTo.Init();
			iOEntity2.outputs[num3].connectedTo.Set(iOEntity);
			iOEntity2.outputs[num3].connectedToSlot = num2;
			iOEntity2.outputs[num3].linePoints = list.ToArray();
			iOEntity2.outputs[num3].wireColour = wireColour;
			iOEntity2.outputs[num3].connectedTo.Init();
			iOEntity2.outputs[num3].worldSpaceLineEndRotation = iOEntity.transform.TransformDirection(iOEntity.inputs[num2].handleDirection);
			iOEntity2.MarkDirtyForceUpdateOutputs();
			iOEntity2.SendNetworkUpdate();
			iOEntity.SendNetworkUpdate();
			iOEntity2.SendChangedToRoot(forceUpdate: true);
			iOEntity2.RefreshIndustrialPreventBuilding();
			if (wireType == IOEntity.IOType.Industrial)
			{
				iOEntity.NotifyIndustrialNetworkChanged();
				iOEntity2.NotifyIndustrialNetworkChanged();
			}
		}
	}

	[RPC_Server]
	public void SetPlugged(RPCMessage msg)
	{
	}

	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void RequestClear(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanPlayerUseWires(player))
		{
			return;
		}
		NetworkableId uid = msg.read.EntityID();
		int num = msg.read.Int32();
		bool flag = msg.read.Bit();
		bool flag2 = msg.read.Bit();
		IOEntity iOEntity = BaseNetworkable.serverEntities.Find(uid) as IOEntity;
		if (iOEntity == null)
		{
			return;
		}
		IOEntity.IOSlot iOSlot = (flag ? iOEntity.inputs : iOEntity.outputs)[num];
		IOEntity iOEntity2 = iOSlot.connectedTo.Get();
		if (iOEntity2 == null)
		{
			return;
		}
		IOEntity.IOSlot iOSlot2 = (flag ? iOEntity2.outputs : iOEntity2.inputs)[iOSlot.connectedToSlot];
		using WireReconnectMessage wireReconnectMessage = Facepunch.Pool.Get<WireReconnectMessage>();
		wireReconnectMessage.isInput = !flag;
		wireReconnectMessage.slotIndex = iOSlot.connectedToSlot;
		wireReconnectMessage.entityId = iOSlot.connectedTo.Get().net.ID;
		wireReconnectMessage.wireColor = 0;
		wireReconnectMessage.linePoints = Facepunch.Pool.GetList<Vector3>();
		IOEntity iOEntity3 = iOEntity;
		Vector3[] array = iOSlot.linePoints;
		if (array == null || array.Length == 0)
		{
			iOEntity3 = iOEntity2;
			array = iOSlot2.linePoints;
		}
		if (array == null)
		{
			array = new Vector3[0];
		}
		bool flag3 = iOEntity3 != iOEntity;
		if (iOEntity == iOEntity3 && flag)
		{
			flag3 = true;
		}
		wireReconnectMessage.linePoints.AddRange(array);
		if (flag3)
		{
			wireReconnectMessage.linePoints.Reverse();
		}
		if (wireReconnectMessage.linePoints.Count >= 2)
		{
			wireReconnectMessage.linePoints.RemoveAt(0);
			wireReconnectMessage.linePoints.RemoveAt(wireReconnectMessage.linePoints.Count - 1);
		}
		for (int i = 0; i < wireReconnectMessage.linePoints.Count; i++)
		{
			wireReconnectMessage.linePoints[i] = iOEntity3.transform.TransformPoint(wireReconnectMessage.linePoints[i]);
		}
		if (AttemptClearSlot(iOEntity, player, num, flag) && flag2)
		{
			ClientRPCPlayer(null, player, "OnWireCleared", wireReconnectMessage);
		}
	}

	public static bool AttemptClearSlot(BaseNetworkable clearEnt, BasePlayer ply, int clearIndex, bool isInput)
	{
		IOEntity iOEntity = ((clearEnt == null) ? null : clearEnt.GetComponent<IOEntity>());
		if (iOEntity == null)
		{
			return false;
		}
		if (ply != null && !CanModifyEntity(ply, iOEntity))
		{
			return false;
		}
		if (clearIndex >= (isInput ? iOEntity.inputs.Length : iOEntity.outputs.Length))
		{
			return false;
		}
		IOEntity.IOSlot iOSlot = (isInput ? iOEntity.inputs[clearIndex] : iOEntity.outputs[clearIndex]);
		if (iOSlot.connectedTo.Get() == null)
		{
			return false;
		}
		IOEntity iOEntity2 = iOSlot.connectedTo.Get();
		object obj = Interface.CallHook("OnWireClear", ply, iOEntity, clearIndex, iOEntity2, isInput);
		if (obj is bool)
		{
			return (bool)obj;
		}
		IOEntity.IOSlot obj2 = (isInput ? iOEntity2.outputs[iOSlot.connectedToSlot] : iOEntity2.inputs[iOSlot.connectedToSlot]);
		if (isInput)
		{
			iOEntity.UpdateFromInput(0, clearIndex);
		}
		else if ((bool)iOEntity2)
		{
			iOEntity2.UpdateFromInput(0, iOSlot.connectedToSlot);
		}
		iOSlot.Clear();
		obj2.Clear();
		iOEntity.MarkDirtyForceUpdateOutputs();
		iOEntity.SendNetworkUpdateImmediate();
		iOEntity.RefreshIndustrialPreventBuilding();
		if (iOEntity2 != null)
		{
			iOEntity2.RefreshIndustrialPreventBuilding();
		}
		if (isInput && iOEntity2 != null)
		{
			iOEntity2.SendChangedToRoot(forceUpdate: true);
		}
		else if (!isInput)
		{
			IOEntity.IOSlot[] inputs = iOEntity.inputs;
			foreach (IOEntity.IOSlot iOSlot2 in inputs)
			{
				if (iOSlot2.mainPowerSlot && (bool)iOSlot2.connectedTo.Get())
				{
					iOSlot2.connectedTo.Get().SendChangedToRoot(forceUpdate: true);
				}
			}
		}
		iOEntity2.SendNetworkUpdateImmediate();
		if (iOEntity != null && iOEntity.ioType == IOEntity.IOType.Industrial)
		{
			iOEntity.NotifyIndustrialNetworkChanged();
		}
		if (iOEntity2 != null && iOEntity2.ioType == IOEntity.IOType.Industrial)
		{
			iOEntity2.NotifyIndustrialNetworkChanged();
		}
		return true;
	}

	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void RequestChangeColor(RPCMessage msg)
	{
		if (!CanPlayerUseWires(msg.player))
		{
			return;
		}
		NetworkableId uid = msg.read.EntityID();
		int index = msg.read.Int32();
		bool flag = msg.read.Bit();
		WireColour wireColour = IntToColour(msg.read.Int32());
		IOEntity iOEntity = BaseNetworkable.serverEntities.Find(uid) as IOEntity;
		if (iOEntity == null)
		{
			return;
		}
		IOEntity.IOSlot iOSlot = (flag ? iOEntity.inputs.ElementAtOrDefault(index) : iOEntity.outputs.ElementAtOrDefault(index));
		if (iOSlot != null)
		{
			IOEntity iOEntity2 = iOSlot.connectedTo.Get();
			if (!(iOEntity2 == null))
			{
				IOEntity.IOSlot obj = (flag ? iOEntity2.outputs : iOEntity2.inputs)[iOSlot.connectedToSlot];
				iOSlot.wireColour = wireColour;
				iOEntity.SendNetworkUpdate();
				obj.wireColour = wireColour;
				iOEntity2.SendNetworkUpdate();
			}
		}
	}

	public WireColour IntToColour(int i)
	{
		if (i < 0)
		{
			i = 0;
		}
		if (i >= 10)
		{
			i = 9;
		}
		WireColour wireColour = (WireColour)i;
		if (wireType == IOEntity.IOType.Fluidic && wireColour == WireColour.Green)
		{
			wireColour = WireColour.Default;
		}
		return wireColour;
	}

	public bool ValidateLine(List<Vector3> lineList, IOEntity inputEntity, IOEntity outputEntity, BasePlayer byPlayer, int outputIndex)
	{
		if (lineList.Count < 2)
		{
			return false;
		}
		if (inputEntity == null || outputEntity == null)
		{
			return false;
		}
		Vector3 a = lineList[0];
		float num = 0f;
		int count = lineList.Count;
		for (int i = 1; i < count; i++)
		{
			Vector3 vector = lineList[i];
			num += Vector3.Distance(a, vector);
			if (num > maxWireLength)
			{
				return false;
			}
			a = vector;
		}
		Vector3 point = lineList[count - 1];
		Bounds bounds = outputEntity.bounds;
		bounds.Expand(0.5f);
		if (!bounds.Contains(point))
		{
			return false;
		}
		Vector3 position = outputEntity.transform.TransformPoint(lineList[0]);
		point = inputEntity.transform.InverseTransformPoint(position);
		Bounds bounds2 = inputEntity.bounds;
		bounds2.Expand(0.5f);
		if (!bounds2.Contains(point))
		{
			return false;
		}
		if (byPlayer == null)
		{
			return false;
		}
		Vector3 position2 = outputEntity.transform.TransformPoint(lineList[lineList.Count - 1]);
		if (byPlayer.Distance(position2) > 5f && byPlayer.Distance(position) > 5f)
		{
			return false;
		}
		if (outputIndex >= 0 && outputIndex < outputEntity.outputs.Length && outputEntity.outputs[outputIndex].type == IOEntity.IOType.Industrial && !VerifyLineOfSight(lineList, outputEntity.transform.localToWorldMatrix))
		{
			return false;
		}
		return true;
	}

	public bool VerifyLineOfSight(List<Vector3> positions, Matrix4x4 localToWorldSpace)
	{
		Vector3 worldSpaceA = localToWorldSpace.MultiplyPoint3x4(positions[0]);
		for (int i = 1; i < positions.Count; i++)
		{
			Vector3 vector = localToWorldSpace.MultiplyPoint3x4(positions[i]);
			if (!VerifyLineOfSight(worldSpaceA, vector))
			{
				return false;
			}
			worldSpaceA = vector;
		}
		return true;
	}

	public bool VerifyLineOfSight(Vector3 worldSpaceA, Vector3 worldSpaceB)
	{
		float maxDistance = Vector3.Distance(worldSpaceA, worldSpaceB);
		Vector3 normalized = (worldSpaceA - worldSpaceB).normalized;
		List<RaycastHit> obj = Facepunch.Pool.GetList<RaycastHit>();
		GamePhysics.TraceAll(new Ray(worldSpaceB, normalized), 0.01f, obj, maxDistance, 2162944);
		bool result = true;
		foreach (RaycastHit item in obj)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if (entity != null && RaycastHitEx.IsOnLayer(item, Rust.Layer.Deployed))
			{
				if (entity is VendingMachine)
				{
					result = false;
					break;
				}
			}
			else if (!(entity != null) || !(entity is Door))
			{
				result = false;
				break;
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}
}
