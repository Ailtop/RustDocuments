#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
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

	public GameObjectRef ioLine;

	public IOEntity.IOType wireType;

	public float RadialMenuHoldTime = 0.25f;

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

	public bool CanChangeColours
	{
		get
		{
			if (wireType != 0)
			{
				return wireType == IOEntity.IOType.Fluidic;
			}
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WireTool.OnRpcMessage"))
		{
			if (rpc == 678101026 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - AddLine "));
				}
				using (TimeWarning.New("AddLine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(678101026u, "AddLine", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(678101026u, "AddLine", this, player))
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
							AddLine(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AddLine");
					}
				}
				return true;
			}
			if (rpc == 40328523 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - MakeConnection "));
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
							RPCMessage msg3 = rPCMessage;
							MakeConnection(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in MakeConnection");
					}
				}
				return true;
			}
			if (rpc == 121409151 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RequestChangeColor "));
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
							RPCMessage msg4 = rPCMessage;
							RequestChangeColor(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RequestChangeColor");
					}
				}
				return true;
			}
			if (rpc == 2469840259u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RequestClear "));
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
							RPCMessage msg5 = rPCMessage;
							RequestClear(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RequestClear");
					}
				}
				return true;
			}
			if (rpc == 2596458392u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SetPlugged "));
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
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in SetPlugged");
					}
				}
				return true;
			}
			if (rpc == 210386477 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - TryClear "));
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
							RPCMessage msg6 = rPCMessage;
							TryClear(msg6);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
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

	public static bool CanModifyEntity(BasePlayer player, BaseEntity ent)
	{
		return player.CanBuild(ent.transform.position, ent.transform.rotation, ent.bounds);
	}

	public bool PendingPlugRoot()
	{
		if (pending.ent != null)
		{
			return pending.ent.IsRootEntity();
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	public void TryClear(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		uint uid = msg.read.UInt32();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		IOEntity iOEntity = ((baseNetworkable == null) ? null : baseNetworkable.GetComponent<IOEntity>());
		if (!(iOEntity == null) && CanPlayerUseWires(player) && CanModifyEntity(player, iOEntity))
		{
			iOEntity.ClearConnections();
			iOEntity.SendNetworkUpdate();
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void MakeConnection(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanPlayerUseWires(player))
		{
			return;
		}
		uint uid = msg.read.UInt32();
		int num = msg.read.Int32();
		uint uid2 = msg.read.UInt32();
		int num2 = msg.read.Int32();
		WireColour wireColour = IntToColour(msg.read.Int32());
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		IOEntity iOEntity = ((baseNetworkable == null) ? null : baseNetworkable.GetComponent<IOEntity>());
		if (iOEntity == null)
		{
			return;
		}
		BaseNetworkable baseNetworkable2 = BaseNetworkable.serverEntities.Find(uid2);
		IOEntity iOEntity2 = ((baseNetworkable2 == null) ? null : baseNetworkable2.GetComponent<IOEntity>());
		if (!(iOEntity2 == null) && !(Vector3.Distance(baseNetworkable2.transform.position, baseNetworkable.transform.position) > maxWireLength) && num < iOEntity.inputs.Length && num2 < iOEntity2.outputs.Length && !(iOEntity.inputs[num].connectedTo.Get() != null) && !(iOEntity2.outputs[num2].connectedTo.Get() != null) && (!iOEntity.inputs[num].rootConnectionsOnly || iOEntity2.IsRootEntity()) && CanModifyEntity(player, iOEntity) && CanModifyEntity(player, iOEntity2))
		{
			if (Interface.CallHook("OnWireConnect", msg.player, baseNetworkable, num, baseNetworkable2, num2) != null)
			{
				((IOEntity)baseNetworkable2).outputs[num2].linePoints = null;
				return;
			}
			iOEntity.inputs[num].connectedTo.Set(iOEntity2);
			iOEntity.inputs[num].connectedToSlot = num2;
			iOEntity.inputs[num].wireColour = wireColour;
			iOEntity.inputs[num].connectedTo.Init();
			iOEntity2.outputs[num2].connectedTo.Set(iOEntity);
			iOEntity2.outputs[num2].connectedToSlot = num;
			iOEntity2.outputs[num2].wireColour = wireColour;
			iOEntity2.outputs[num2].connectedTo.Init();
			iOEntity2.MarkDirtyForceUpdateOutputs();
			iOEntity2.SendNetworkUpdate();
			iOEntity.SendNetworkUpdate();
			iOEntity2.SendChangedToRoot(forceUpdate: true);
		}
	}

	[RPC_Server]
	public void SetPlugged(RPCMessage msg)
	{
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	public void RequestClear(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanPlayerUseWires(player))
		{
			return;
		}
		uint uid = msg.read.UInt32();
		int num = msg.read.Int32();
		bool flag = msg.read.Bit();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		IOEntity iOEntity = ((baseNetworkable == null) ? null : baseNetworkable.GetComponent<IOEntity>());
		if (iOEntity == null || !CanModifyEntity(player, iOEntity) || num >= (flag ? iOEntity.inputs.Length : iOEntity.outputs.Length))
		{
			return;
		}
		IOEntity.IOSlot iOSlot = (flag ? iOEntity.inputs[num] : iOEntity.outputs[num]);
		if (iOSlot.connectedTo.Get() == null)
		{
			return;
		}
		IOEntity iOEntity2 = iOSlot.connectedTo.Get();
		if (Interface.CallHook("OnWireClear", msg.player, iOEntity, num, iOEntity2, flag) != null)
		{
			return;
		}
		IOEntity.IOSlot obj = (flag ? iOEntity2.outputs[iOSlot.connectedToSlot] : iOEntity2.inputs[iOSlot.connectedToSlot]);
		if (flag)
		{
			iOEntity.UpdateFromInput(0, num);
		}
		else if ((bool)iOEntity2)
		{
			iOEntity2.UpdateFromInput(0, iOSlot.connectedToSlot);
		}
		iOSlot.Clear();
		obj.Clear();
		iOEntity.MarkDirtyForceUpdateOutputs();
		iOEntity.SendNetworkUpdate();
		if (flag && iOEntity2 != null)
		{
			iOEntity2.SendChangedToRoot(forceUpdate: true);
		}
		else if (!flag)
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
		iOEntity2.SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	public void RequestChangeColor(RPCMessage msg)
	{
		if (!CanPlayerUseWires(msg.player))
		{
			return;
		}
		uint uid = msg.read.UInt32();
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

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	public void AddLine(RPCMessage msg)
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
		uint uid = msg.read.UInt32();
		int num2 = msg.read.Int32();
		uint uid2 = msg.read.UInt32();
		int num3 = msg.read.Int32();
		WireColour wireColour = IntToColour(msg.read.Int32());
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		IOEntity iOEntity = ((baseNetworkable == null) ? null : baseNetworkable.GetComponent<IOEntity>());
		if (!(iOEntity == null))
		{
			BaseNetworkable baseNetworkable2 = BaseNetworkable.serverEntities.Find(uid2);
			IOEntity iOEntity2 = ((baseNetworkable2 == null) ? null : baseNetworkable2.GetComponent<IOEntity>());
			if (!(iOEntity2 == null) && ValidateLine(list, iOEntity, iOEntity2, player) && num2 < iOEntity.inputs.Length && num3 < iOEntity2.outputs.Length && !(iOEntity.inputs[num2].connectedTo.Get() != null) && !(iOEntity2.outputs[num3].connectedTo.Get() != null) && (!iOEntity.inputs[num2].rootConnectionsOnly || iOEntity2.IsRootEntity()) && CanModifyEntity(player, iOEntity2) && CanModifyEntity(player, iOEntity))
			{
				iOEntity2.outputs[num3].linePoints = list.ToArray();
				iOEntity2.outputs[num3].wireColour = wireColour;
				iOEntity2.SendNetworkUpdate();
			}
		}
	}

	private WireColour IntToColour(int i)
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

	private bool ValidateLine(List<Vector3> lineList, IOEntity inputEntity, IOEntity outputEntity, BasePlayer byPlayer)
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
		return true;
	}
}
