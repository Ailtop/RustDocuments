#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class IOEntity : DecayEntity
{
	public enum IOType
	{
		Electric = 0,
		Fluidic = 1,
		Kinetic = 2,
		Generic = 3,
		Industrial = 4
	}

	[Serializable]
	public class IORef
	{
		public EntityRef entityRef;

		public IOEntity ioEnt;

		public void Init()
		{
			if (ioEnt != null && !entityRef.IsValid(serverside: true))
			{
				entityRef.Set(ioEnt);
			}
			if (entityRef.IsValid(serverside: true))
			{
				ioEnt = entityRef.Get(serverside: true).GetComponent<IOEntity>();
			}
		}

		public void InitClient()
		{
			if (entityRef.IsValid(serverside: false) && ioEnt == null)
			{
				ioEnt = entityRef.Get(serverside: false).GetComponent<IOEntity>();
			}
		}

		public IOEntity Get(bool isServer = true)
		{
			if (ioEnt == null && entityRef.IsValid(isServer))
			{
				ioEnt = entityRef.Get(isServer) as IOEntity;
			}
			return ioEnt;
		}

		public void Clear()
		{
			IOEntity obj = ioEnt;
			ioEnt = null;
			entityRef.Set(null);
			Interface.CallHook("OnIORefCleared", this, obj);
		}

		public void Set(IOEntity newIOEnt)
		{
			entityRef.Set(newIOEnt);
		}
	}

	[Serializable]
	public class IOSlot
	{
		public string niceName;

		public IOType type;

		public IORef connectedTo;

		public int connectedToSlot;

		public Vector3[] linePoints;

		public float[] slackLevels;

		public Vector3 worldSpaceLineEndRotation;

		public ClientIOLine line;

		public Vector3 handlePosition;

		public Vector3 handleDirection;

		public bool rootConnectionsOnly;

		public bool mainPowerSlot;

		public WireTool.WireColour wireColour;

		public float lineThickness;

		public void Clear()
		{
			if (connectedTo == null)
			{
				connectedTo = new IORef();
			}
			else
			{
				connectedTo.Clear();
			}
			connectedToSlot = 0;
			linePoints = null;
		}
	}

	private struct FrameTiming
	{
		public string PrefabName;

		public float Time;
	}

	public struct ContainerInputOutput
	{
		public IIndustrialStorage Storage;

		public int SlotIndex;

		public int MaxStackSize;

		public int ParentStorage;

		public int IndustrialSiblingCount;
	}

	[Header("IOEntity")]
	public Transform debugOrigin;

	public ItemDefinition sourceItem;

	[NonSerialized]
	public int lastResetIndex;

	[Help("How many miliseconds to budget for processing io entities per server frame")]
	[ServerVar]
	public static float framebudgetms = 1f;

	[ServerVar]
	public static float responsetime = 0.1f;

	[ServerVar]
	public static int backtracking = 8;

	[ServerVar(Help = "Print out what is taking so long in the IO frame budget")]
	public static bool debugBudget = false;

	[ServerVar(Help = "Ignore frames with a lower ms than this while debugBudget is active")]
	public static float debugBudgetThreshold = 2f;

	public const Flags Flag_ShortCircuit = Flags.Reserved7;

	public const Flags Flag_HasPower = Flags.Reserved8;

	public IOSlot[] inputs;

	public IOSlot[] outputs;

	public IOType ioType;

	public static Queue<IOEntity> _processQueue = new Queue<IOEntity>();

	private static List<FrameTiming> timings = new List<FrameTiming>();

	public int cachedOutputsUsed;

	public int lastPassthroughEnergy;

	public int lastEnergy;

	public int currentEnergy;

	public float lastUpdateTime;

	public int lastUpdateBlockedFrame;

	public bool ensureOutputsUpdated;

	public const int MaxContainerSourceCount = 32;

	private List<Collider> spawnedColliders = new List<Collider>();

	public virtual bool IsGravitySource => false;

	private bool HasBlockedUpdatedOutputsThisFrame => UnityEngine.Time.frameCount == lastUpdateBlockedFrame;

	public virtual bool BlockFluidDraining => false;

	protected virtual float LiquidPassthroughGravityThreshold => 1f;

	protected virtual bool DisregardGravityRestrictionsOnLiquid => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("IOEntity.OnRpcMessage"))
		{
			if (rpc == 4161541566u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RequestData ");
				}
				using (TimeWarning.New("Server_RequestData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4161541566u, "Server_RequestData", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4161541566u, "Server_RequestData", this, player, 6f))
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
							Server_RequestData(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_RequestData");
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
		if (base.isServer)
		{
			lastResetIndex = 0;
			cachedOutputsUsed = 0;
			lastPassthroughEnergy = 0;
			lastEnergy = 0;
			currentEnergy = 0;
			lastUpdateTime = 0f;
			ensureOutputsUpdated = false;
		}
		ClearIndustrialPreventBuilding();
	}

	public string GetDisplayName()
	{
		if (sourceItem != null)
		{
			return sourceItem.displayName.translated;
		}
		return base.ShortPrefabName;
	}

	public virtual bool IsRootEntity()
	{
		return false;
	}

	public IOEntity FindGravitySource(ref Vector3 worldHandlePosition, int depth, bool ignoreSelf)
	{
		if (depth <= 0)
		{
			return null;
		}
		if (!ignoreSelf && IsGravitySource)
		{
			worldHandlePosition = base.transform.TransformPoint(outputs[0].handlePosition);
			return this;
		}
		IOSlot[] array = inputs;
		for (int i = 0; i < array.Length; i++)
		{
			IOEntity iOEntity = array[i].connectedTo.Get(base.isServer);
			if (iOEntity != null)
			{
				if (iOEntity.IsGravitySource)
				{
					worldHandlePosition = iOEntity.transform.TransformPoint(iOEntity.outputs[0].handlePosition);
					return iOEntity;
				}
				iOEntity = iOEntity.FindGravitySource(ref worldHandlePosition, depth - 1, ignoreSelf: false);
				if (iOEntity != null)
				{
					worldHandlePosition = iOEntity.transform.TransformPoint(iOEntity.outputs[0].handlePosition);
					return iOEntity;
				}
			}
		}
		return null;
	}

	public virtual void SetFuelType(ItemDefinition def, IOEntity source)
	{
	}

	public virtual bool WantsPower()
	{
		return true;
	}

	public virtual bool AllowWireConnections()
	{
		if (GetComponentInParent<BaseVehicle>() != null)
		{
			return false;
		}
		return true;
	}

	public virtual bool WantsPassthroughPower()
	{
		return WantsPower();
	}

	public virtual int ConsumptionAmount()
	{
		return 1;
	}

	public virtual bool ShouldDrainBattery(IOEntity battery)
	{
		return ioType == battery.ioType;
	}

	public virtual int MaximalPowerOutput()
	{
		return 0;
	}

	public virtual bool AllowDrainFrom(int outputSlot)
	{
		return true;
	}

	public virtual bool IsPowered()
	{
		return HasFlag(Flags.Reserved8);
	}

	public bool IsConnectedToAnySlot(IOEntity entity, int slot, int depth, bool defaultReturn = false)
	{
		if (depth > 0 && slot < inputs.Length)
		{
			IOEntity iOEntity = inputs[slot].connectedTo.Get();
			if (iOEntity != null)
			{
				if (iOEntity == entity)
				{
					return true;
				}
				if (ConsiderConnectedTo(entity))
				{
					return true;
				}
				if (iOEntity.IsConnectedTo(entity, depth - 1, defaultReturn))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsConnectedTo(IOEntity entity, int slot, int depth, bool defaultReturn = false)
	{
		if (depth > 0 && slot < inputs.Length)
		{
			IOSlot iOSlot = inputs[slot];
			if (iOSlot.mainPowerSlot)
			{
				IOEntity iOEntity = iOSlot.connectedTo.Get();
				if (iOEntity != null)
				{
					if (iOEntity == entity)
					{
						return true;
					}
					if (ConsiderConnectedTo(entity))
					{
						return true;
					}
					if (iOEntity.IsConnectedTo(entity, depth - 1, defaultReturn))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsConnectedTo(IOEntity entity, int depth, bool defaultReturn = false)
	{
		if (depth > 0)
		{
			for (int i = 0; i < inputs.Length; i++)
			{
				IOSlot iOSlot = inputs[i];
				if (!iOSlot.mainPowerSlot)
				{
					continue;
				}
				IOEntity iOEntity = iOSlot.connectedTo.Get();
				if (iOEntity != null)
				{
					if (iOEntity == entity)
					{
						return true;
					}
					if (ConsiderConnectedTo(entity))
					{
						return true;
					}
					if (iOEntity.IsConnectedTo(entity, depth - 1, defaultReturn))
					{
						return true;
					}
				}
			}
			return false;
		}
		return defaultReturn;
	}

	protected virtual bool ConsiderConnectedTo(IOEntity entity)
	{
		return false;
	}

	[RPC_Server.IsVisible(6f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(10uL)]
	private void Server_RequestData(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		int slot = msg.read.Int32();
		bool input = msg.read.Int32() == 1;
		SendAdditionalData(player, slot, input);
	}

	public virtual void SendAdditionalData(BasePlayer player, int slot, bool input)
	{
		int passthroughAmountForAnySlot = GetPassthroughAmountForAnySlot(slot, input);
		ClientRPCPlayer(null, player, "Client_ReceiveAdditionalData", currentEnergy, passthroughAmountForAnySlot, 0f, 0f);
	}

	protected int GetPassthroughAmountForAnySlot(int slot, bool isInputSlot)
	{
		int result = 0;
		if (isInputSlot)
		{
			if (slot >= 0 && slot < inputs.Length)
			{
				IOSlot iOSlot = inputs[slot];
				IOEntity iOEntity = iOSlot.connectedTo.Get();
				if (iOEntity != null && iOSlot.connectedToSlot >= 0 && iOSlot.connectedToSlot < iOEntity.outputs.Length)
				{
					result = iOEntity.GetPassthroughAmount(inputs[slot].connectedToSlot);
				}
			}
		}
		else if (slot >= 0 && slot < outputs.Length)
		{
			result = GetPassthroughAmount(slot);
		}
		return result;
	}

	public static void ProcessQueue()
	{
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float num = framebudgetms / 1000f;
		if (debugBudget)
		{
			timings.Clear();
		}
		while (_processQueue.Count > 0 && UnityEngine.Time.realtimeSinceStartup < realtimeSinceStartup + num && !_processQueue.Peek().HasBlockedUpdatedOutputsThisFrame)
		{
			float realtimeSinceStartup2 = UnityEngine.Time.realtimeSinceStartup;
			IOEntity iOEntity = _processQueue.Dequeue();
			if (BaseNetworkableEx.IsValid(iOEntity))
			{
				iOEntity.UpdateOutputs();
			}
			if (debugBudget)
			{
				timings.Add(new FrameTiming
				{
					PrefabName = iOEntity.ShortPrefabName,
					Time = (UnityEngine.Time.realtimeSinceStartup - realtimeSinceStartup2) * 1000f
				});
			}
		}
		if (!debugBudget)
		{
			return;
		}
		float num2 = UnityEngine.Time.realtimeSinceStartup - realtimeSinceStartup;
		float num3 = debugBudgetThreshold / 1000f;
		if (!(num2 > num3))
		{
			return;
		}
		TextTable textTable = new TextTable();
		textTable.AddColumns("Prefab Name", "Time (in ms)");
		foreach (FrameTiming timing in timings)
		{
			string[] obj = new string[2] { timing.PrefabName, null };
			float time = timing.Time;
			obj[1] = time.ToString();
			textTable.AddRow(obj);
		}
		textTable.AddRow("Total time", (num2 * 1000f).ToString());
		Debug.Log(textTable.ToString());
	}

	public virtual void ResetIOState()
	{
	}

	public virtual void Init()
	{
		for (int i = 0; i < outputs.Length; i++)
		{
			IOSlot iOSlot = outputs[i];
			iOSlot.connectedTo.Init();
			if (iOSlot.connectedTo.Get() != null)
			{
				int connectedToSlot = iOSlot.connectedToSlot;
				if (connectedToSlot < 0 || connectedToSlot >= iOSlot.connectedTo.Get().inputs.Length)
				{
					Debug.LogError("Slot IOR Error: " + base.name + " setting up inputs for " + iOSlot.connectedTo.Get().name + " slot : " + iOSlot.connectedToSlot);
				}
				else
				{
					iOSlot.connectedTo.Get().inputs[iOSlot.connectedToSlot].connectedTo.Set(this);
					iOSlot.connectedTo.Get().inputs[iOSlot.connectedToSlot].connectedToSlot = i;
					iOSlot.connectedTo.Get().inputs[iOSlot.connectedToSlot].connectedTo.Init();
				}
			}
		}
		UpdateUsedOutputs();
		if (IsRootEntity())
		{
			Invoke(MarkDirtyForceUpdateOutputs, UnityEngine.Random.Range(1f, 1f));
		}
	}

	internal override void DoServerDestroy()
	{
		if (base.isServer)
		{
			Shutdown();
		}
		base.DoServerDestroy();
	}

	public void ClearConnections()
	{
		List<IOEntity> obj = Facepunch.Pool.GetList<IOEntity>();
		List<IOEntity> obj2 = Facepunch.Pool.GetList<IOEntity>();
		IOSlot[] array = inputs;
		foreach (IOSlot iOSlot in array)
		{
			IOEntity iOEntity = null;
			if (iOSlot.connectedTo.Get() != null)
			{
				iOEntity = iOSlot.connectedTo.Get();
				if (iOSlot.type == IOType.Industrial)
				{
					obj2.Add(iOEntity);
				}
				IOSlot[] array2 = iOSlot.connectedTo.Get().outputs;
				foreach (IOSlot iOSlot2 in array2)
				{
					if (iOSlot2.connectedTo.Get() != null && iOSlot2.connectedTo.Get().EqualNetID(this))
					{
						iOSlot2.Clear();
					}
				}
			}
			iOSlot.Clear();
			if ((bool)iOEntity)
			{
				iOEntity.SendNetworkUpdate();
			}
		}
		array = outputs;
		foreach (IOSlot iOSlot3 in array)
		{
			if (iOSlot3.connectedTo.Get() != null)
			{
				obj.Add(iOSlot3.connectedTo.Get());
				if (iOSlot3.type == IOType.Industrial)
				{
					obj2.Add(obj[obj.Count - 1]);
				}
				IOSlot[] array2 = iOSlot3.connectedTo.Get().inputs;
				foreach (IOSlot iOSlot4 in array2)
				{
					if (iOSlot4.connectedTo.Get() != null && iOSlot4.connectedTo.Get().EqualNetID(this))
					{
						iOSlot4.Clear();
					}
				}
			}
			if ((bool)iOSlot3.connectedTo.Get())
			{
				iOSlot3.connectedTo.Get().UpdateFromInput(0, iOSlot3.connectedToSlot);
			}
			iOSlot3.Clear();
		}
		SendNetworkUpdate();
		foreach (IOEntity item in obj)
		{
			if (item != null)
			{
				item.MarkDirty();
				item.SendNetworkUpdate();
			}
		}
		for (int k = 0; k < inputs.Length; k++)
		{
			UpdateFromInput(0, k);
		}
		foreach (IOEntity item2 in obj2)
		{
			if (item2 != null)
			{
				item2.NotifyIndustrialNetworkChanged();
			}
			item2.RefreshIndustrialPreventBuilding();
		}
		Facepunch.Pool.FreeList(ref obj);
		Facepunch.Pool.FreeList(ref obj2);
		RefreshIndustrialPreventBuilding();
	}

	public void Shutdown()
	{
		SendChangedToRoot(forceUpdate: true);
		ClearConnections();
	}

	public void MarkDirtyForceUpdateOutputs()
	{
		ensureOutputsUpdated = true;
		MarkDirty();
	}

	public void UpdateUsedOutputs()
	{
		cachedOutputsUsed = 0;
		IOSlot[] array = outputs;
		for (int i = 0; i < array.Length; i++)
		{
			IOEntity iOEntity = array[i].connectedTo.Get();
			if (iOEntity != null && !iOEntity.IsDestroyed)
			{
				cachedOutputsUsed++;
			}
		}
	}

	public virtual void MarkDirty()
	{
		if (!base.isClient)
		{
			UpdateUsedOutputs();
			TouchIOState();
		}
	}

	public virtual int DesiredPower()
	{
		return ConsumptionAmount();
	}

	public virtual int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return inputAmount;
	}

	public virtual int GetCurrentEnergy()
	{
		return Mathf.Clamp(currentEnergy - ConsumptionAmount(), 0, currentEnergy);
	}

	public virtual int GetPassthroughAmount(int outputSlot = 0)
	{
		if (outputSlot < 0 || outputSlot >= outputs.Length)
		{
			return 0;
		}
		IOEntity iOEntity = outputs[outputSlot].connectedTo.Get();
		if (iOEntity == null || iOEntity.IsDestroyed)
		{
			return 0;
		}
		int num = ((cachedOutputsUsed == 0) ? 1 : cachedOutputsUsed);
		return GetCurrentEnergy() / num;
	}

	public virtual void UpdateHasPower(int inputAmount, int inputSlot)
	{
		SetFlag(Flags.Reserved8, inputAmount >= ConsumptionAmount() && inputAmount > 0, recursive: false, networkupdate: false);
	}

	public void TouchInternal()
	{
		int passthroughAmount = GetPassthroughAmount();
		bool num = lastPassthroughEnergy != passthroughAmount;
		lastPassthroughEnergy = passthroughAmount;
		if (num)
		{
			IOStateChanged(currentEnergy, 0);
			ensureOutputsUpdated = true;
		}
		_processQueue.Enqueue(this);
	}

	public virtual void UpdateFromInput(int inputAmount, int inputSlot)
	{
		if (Interface.CallHook("OnInputUpdate", this, inputAmount, inputSlot) != null)
		{
			return;
		}
		if (inputs[inputSlot].type != ioType || inputs[inputSlot].type == IOType.Industrial)
		{
			IOStateChanged(inputAmount, inputSlot);
			return;
		}
		UpdateHasPower(inputAmount, inputSlot);
		lastEnergy = currentEnergy;
		currentEnergy = CalculateCurrentEnergy(inputAmount, inputSlot);
		int passthroughAmount = GetPassthroughAmount();
		bool flag = lastPassthroughEnergy != passthroughAmount;
		lastPassthroughEnergy = passthroughAmount;
		if (currentEnergy != lastEnergy || flag)
		{
			IOStateChanged(inputAmount, inputSlot);
			ensureOutputsUpdated = true;
		}
		_processQueue.Enqueue(this);
	}

	public virtual void TouchIOState()
	{
		if (!base.isClient)
		{
			TouchInternal();
		}
	}

	public virtual void SendIONetworkUpdate()
	{
		SendNetworkUpdate_Flags();
	}

	public virtual void IOStateChanged(int inputAmount, int inputSlot)
	{
	}

	public virtual void OnCircuitChanged(bool forceUpdate)
	{
		if (forceUpdate)
		{
			MarkDirtyForceUpdateOutputs();
		}
	}

	public virtual void SendChangedToRoot(bool forceUpdate)
	{
		List<IOEntity> existing = Facepunch.Pool.GetList<IOEntity>();
		SendChangedToRootRecursive(forceUpdate, ref existing);
		Facepunch.Pool.FreeList(ref existing);
	}

	public virtual void SendChangedToRootRecursive(bool forceUpdate, ref List<IOEntity> existing)
	{
		bool flag = IsRootEntity();
		if (existing.Contains(this))
		{
			return;
		}
		existing.Add(this);
		bool flag2 = false;
		for (int i = 0; i < inputs.Length; i++)
		{
			IOSlot iOSlot = inputs[i];
			if (!iOSlot.mainPowerSlot)
			{
				continue;
			}
			IOEntity iOEntity = iOSlot.connectedTo.Get();
			if (!(iOEntity == null) && !existing.Contains(iOEntity))
			{
				flag2 = true;
				if (forceUpdate)
				{
					iOEntity.ensureOutputsUpdated = true;
				}
				iOEntity.SendChangedToRootRecursive(forceUpdate, ref existing);
			}
		}
		if (flag)
		{
			forceUpdate = forceUpdate && !flag2;
			OnCircuitChanged(forceUpdate);
		}
	}

	public void NotifyIndustrialNetworkChanged()
	{
		List<IOEntity> obj = Facepunch.Pool.GetList<IOEntity>();
		OnIndustrialNetworkChanged();
		NotifyIndustrialNetworkChanged(obj, input: true, 128);
		obj.Clear();
		NotifyIndustrialNetworkChanged(obj, input: false, 128);
		Facepunch.Pool.FreeList(ref obj);
	}

	private void NotifyIndustrialNetworkChanged(List<IOEntity> existing, bool input, int maxDepth)
	{
		if (maxDepth <= 0 || existing.Contains(this))
		{
			return;
		}
		if (existing.Count != 0)
		{
			OnIndustrialNetworkChanged();
		}
		existing.Add(this);
		IOSlot[] array = (input ? inputs : outputs);
		foreach (IOSlot iOSlot in array)
		{
			if (iOSlot.type == IOType.Industrial && iOSlot.connectedTo.Get() != null)
			{
				iOSlot.connectedTo.Get().NotifyIndustrialNetworkChanged(existing, input, maxDepth - 1);
			}
		}
	}

	protected virtual void OnIndustrialNetworkChanged()
	{
	}

	public bool ShouldUpdateOutputs()
	{
		if (UnityEngine.Time.realtimeSinceStartup - lastUpdateTime < responsetime)
		{
			lastUpdateBlockedFrame = UnityEngine.Time.frameCount;
			_processQueue.Enqueue(this);
			return false;
		}
		lastUpdateTime = UnityEngine.Time.realtimeSinceStartup;
		SendIONetworkUpdate();
		if (outputs.Length == 0)
		{
			ensureOutputsUpdated = false;
			return false;
		}
		return true;
	}

	public virtual void UpdateOutputs()
	{
		if (Interface.CallHook("OnOutputUpdate", this) != null || !ShouldUpdateOutputs() || !ensureOutputsUpdated)
		{
			return;
		}
		ensureOutputsUpdated = false;
		using (TimeWarning.New("ProcessIOOutputs"))
		{
			for (int i = 0; i < outputs.Length; i++)
			{
				IOSlot iOSlot = outputs[i];
				bool flag = true;
				IOEntity iOEntity = iOSlot.connectedTo.Get();
				if (!(iOEntity != null))
				{
					continue;
				}
				if (ioType == IOType.Fluidic && !DisregardGravityRestrictionsOnLiquid && !iOEntity.DisregardGravityRestrictionsOnLiquid)
				{
					using (TimeWarning.New("FluidOutputProcessing"))
					{
						if (!iOEntity.AllowLiquidPassthrough(this, base.transform.TransformPoint(iOSlot.handlePosition)))
						{
							flag = false;
						}
					}
				}
				int passthroughAmount = GetPassthroughAmount(i);
				iOEntity.UpdateFromInput(flag ? passthroughAmount : 0, iOSlot.connectedToSlot);
			}
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Rust.Application.isLoadingSave)
		{
			Init();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Init();
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		Init();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity = Facepunch.Pool.Get<ProtoBuf.IOEntity>();
		info.msg.ioEntity.inputs = Facepunch.Pool.GetList<ProtoBuf.IOEntity.IOConnection>();
		info.msg.ioEntity.outputs = Facepunch.Pool.GetList<ProtoBuf.IOEntity.IOConnection>();
		IOSlot[] array = inputs;
		foreach (IOSlot iOSlot in array)
		{
			ProtoBuf.IOEntity.IOConnection iOConnection = Facepunch.Pool.Get<ProtoBuf.IOEntity.IOConnection>();
			iOConnection.connectedID = iOSlot.connectedTo.entityRef.uid;
			iOConnection.connectedToSlot = iOSlot.connectedToSlot;
			iOConnection.niceName = iOSlot.niceName;
			iOConnection.type = (int)iOSlot.type;
			iOConnection.inUse = iOConnection.connectedID.IsValid;
			iOConnection.colour = (int)iOSlot.wireColour;
			iOConnection.lineThickness = iOSlot.lineThickness;
			info.msg.ioEntity.inputs.Add(iOConnection);
		}
		array = outputs;
		foreach (IOSlot iOSlot2 in array)
		{
			ProtoBuf.IOEntity.IOConnection iOConnection2 = Facepunch.Pool.Get<ProtoBuf.IOEntity.IOConnection>();
			iOConnection2.connectedID = iOSlot2.connectedTo.entityRef.uid;
			iOConnection2.connectedToSlot = iOSlot2.connectedToSlot;
			iOConnection2.niceName = iOSlot2.niceName;
			iOConnection2.type = (int)iOSlot2.type;
			iOConnection2.inUse = iOConnection2.connectedID.IsValid;
			iOConnection2.colour = (int)iOSlot2.wireColour;
			iOConnection2.worldSpaceRotation = iOSlot2.worldSpaceLineEndRotation;
			iOConnection2.lineThickness = iOSlot2.lineThickness;
			if (iOSlot2.linePoints != null)
			{
				iOConnection2.linePointList = Facepunch.Pool.GetList<ProtoBuf.IOEntity.IOConnection.LineVec>();
				iOConnection2.linePointList.Clear();
				for (int j = 0; j < iOSlot2.linePoints.Length; j++)
				{
					Vector3 vector = iOSlot2.linePoints[j];
					ProtoBuf.IOEntity.IOConnection.LineVec lineVec = Facepunch.Pool.Get<ProtoBuf.IOEntity.IOConnection.LineVec>();
					lineVec.vec = vector;
					if (iOSlot2.slackLevels.Length > j)
					{
						lineVec.vec.w = iOSlot2.slackLevels[j];
					}
					iOConnection2.linePointList.Add(lineVec);
				}
			}
			info.msg.ioEntity.outputs.Add(iOConnection2);
		}
	}

	public virtual float IOInput(IOEntity from, IOType inputType, float inputAmount, int slot = 0)
	{
		IOSlot[] array = outputs;
		foreach (IOSlot iOSlot in array)
		{
			if (iOSlot.connectedTo.Get() != null)
			{
				inputAmount = iOSlot.connectedTo.Get().IOInput(this, iOSlot.type, inputAmount, iOSlot.connectedToSlot);
			}
		}
		return inputAmount;
	}

	public void FindContainerSource(List<ContainerInputOutput> found, int depth, bool input, List<IOEntity> ignoreList, int parentId = -1, int stackSize = 0)
	{
		if (depth <= 0 || found.Count >= 32)
		{
			return;
		}
		int num = 0;
		int num2 = 1;
		IOSlot[] array;
		if (!input)
		{
			num2 = 0;
			array = outputs;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].type == IOType.Industrial)
				{
					num2++;
				}
			}
		}
		List<int> obj = Facepunch.Pool.GetList<int>();
		array = (input ? inputs : outputs);
		foreach (IOSlot iOSlot in array)
		{
			num++;
			if (iOSlot.type != IOType.Industrial)
			{
				continue;
			}
			IOEntity iOEntity = iOSlot.connectedTo.Get(base.isServer);
			if (!(iOEntity != null) || ignoreList.Contains(iOEntity))
			{
				continue;
			}
			int num3 = -1;
			if (iOEntity is IIndustrialStorage storage2)
			{
				num = iOSlot.connectedToSlot;
				if (GetExistingCount(storage2) < 2)
				{
					found.Add(new ContainerInputOutput
					{
						SlotIndex = num,
						Storage = storage2,
						ParentStorage = parentId,
						MaxStackSize = stackSize / num2
					});
					num3 = found.Count - 1;
					obj.Add(num3);
				}
			}
			else
			{
				ignoreList.Add(iOEntity);
			}
			if ((!(iOEntity is IIndustrialStorage) || iOEntity is IndustrialStorageAdaptor) && !(iOEntity is IndustrialConveyor) && iOEntity != null)
			{
				iOEntity.FindContainerSource(found, depth - 1, input, ignoreList, (num3 == -1) ? parentId : num3, stackSize / num2);
			}
		}
		int count = obj.Count;
		foreach (int item in obj)
		{
			ContainerInputOutput value = found[item];
			value.IndustrialSiblingCount = count;
			found[item] = value;
		}
		Facepunch.Pool.FreeList(ref obj);
		int GetExistingCount(IIndustrialStorage storage)
		{
			int num4 = 0;
			foreach (ContainerInputOutput item2 in found)
			{
				if (item2.Storage == storage)
				{
					num4++;
				}
			}
			return num4;
		}
	}

	public virtual bool AllowLiquidPassthrough(IOEntity fromSource, Vector3 sourceWorldPosition, bool forPlacement = false)
	{
		if (fromSource.DisregardGravityRestrictionsOnLiquid || DisregardGravityRestrictionsOnLiquid)
		{
			return true;
		}
		if (inputs.Length == 0)
		{
			return false;
		}
		Vector3 vector = base.transform.TransformPoint(inputs[0].handlePosition);
		float num = sourceWorldPosition.y - vector.y;
		if (num > 0f)
		{
			return true;
		}
		if (Mathf.Abs(num) < LiquidPassthroughGravityThreshold)
		{
			return true;
		}
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity == null)
		{
			return;
		}
		if (!info.fromDisk && info.msg.ioEntity.inputs != null)
		{
			int count = info.msg.ioEntity.inputs.Count;
			if (inputs.Length != count)
			{
				inputs = new IOSlot[count];
			}
			for (int i = 0; i < count; i++)
			{
				if (inputs[i] == null)
				{
					inputs[i] = new IOSlot();
				}
				ProtoBuf.IOEntity.IOConnection iOConnection = info.msg.ioEntity.inputs[i];
				inputs[i].connectedTo = new IORef();
				inputs[i].connectedTo.entityRef.uid = iOConnection.connectedID;
				if (base.isClient)
				{
					inputs[i].connectedTo.InitClient();
				}
				inputs[i].connectedToSlot = iOConnection.connectedToSlot;
				inputs[i].niceName = iOConnection.niceName;
				inputs[i].type = (IOType)iOConnection.type;
				inputs[i].wireColour = (WireTool.WireColour)iOConnection.colour;
				inputs[i].lineThickness = iOConnection.lineThickness;
			}
		}
		if (info.msg.ioEntity.outputs != null)
		{
			int count2 = info.msg.ioEntity.outputs.Count;
			IOSlot[] array = null;
			if (outputs.Length != count2 && count2 > 0)
			{
				array = outputs;
				outputs = new IOSlot[count2];
				for (int j = 0; j < array.Length; j++)
				{
					if (j < count2)
					{
						outputs[j] = array[j];
					}
				}
			}
			for (int k = 0; k < count2; k++)
			{
				if (outputs[k] == null)
				{
					outputs[k] = new IOSlot();
				}
				ProtoBuf.IOEntity.IOConnection iOConnection2 = info.msg.ioEntity.outputs[k];
				if (iOConnection2.linePointList == null || iOConnection2.linePointList.Count == 0 || !iOConnection2.connectedID.IsValid)
				{
					outputs[k].Clear();
				}
				outputs[k].connectedTo = new IORef();
				outputs[k].connectedTo.entityRef.uid = iOConnection2.connectedID;
				if (base.isClient)
				{
					outputs[k].connectedTo.InitClient();
				}
				outputs[k].connectedToSlot = iOConnection2.connectedToSlot;
				outputs[k].niceName = iOConnection2.niceName;
				outputs[k].type = (IOType)iOConnection2.type;
				outputs[k].wireColour = (WireTool.WireColour)iOConnection2.colour;
				outputs[k].worldSpaceLineEndRotation = iOConnection2.worldSpaceRotation;
				outputs[k].lineThickness = iOConnection2.lineThickness;
				if (info.fromDisk || base.isClient)
				{
					List<ProtoBuf.IOEntity.IOConnection.LineVec> linePointList = iOConnection2.linePointList;
					if (outputs[k].linePoints == null || outputs[k].linePoints.Length != linePointList.Count)
					{
						outputs[k].linePoints = new Vector3[linePointList.Count];
					}
					if (outputs[k].slackLevels == null || outputs[k].slackLevels.Length != linePointList.Count)
					{
						outputs[k].slackLevels = new float[linePointList.Count];
					}
					for (int l = 0; l < linePointList.Count; l++)
					{
						outputs[k].linePoints[l] = linePointList[l].vec;
						outputs[k].slackLevels[l] = linePointList[l].vec.w;
					}
				}
			}
		}
		RefreshIndustrialPreventBuilding();
	}

	public int GetConnectedInputCount()
	{
		int num = 0;
		IOSlot[] array = inputs;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].connectedTo.Get(base.isServer) != null)
			{
				num++;
			}
		}
		return num;
	}

	public int GetConnectedOutputCount()
	{
		int num = 0;
		IOSlot[] array = outputs;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].connectedTo.Get(base.isServer) != null)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasConnections()
	{
		if (GetConnectedInputCount() <= 0)
		{
			return GetConnectedOutputCount() > 0;
		}
		return true;
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		ClearIndustrialPreventBuilding();
	}

	public void RefreshIndustrialPreventBuilding()
	{
		ClearIndustrialPreventBuilding();
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		for (int i = 0; i < outputs.Length; i++)
		{
			IOSlot iOSlot = outputs[i];
			if (iOSlot.type != IOType.Industrial || iOSlot.linePoints == null || iOSlot.linePoints.Length <= 1)
			{
				continue;
			}
			Vector3 vector = localToWorldMatrix.MultiplyPoint3x4(iOSlot.linePoints[0]);
			for (int j = 1; j < iOSlot.linePoints.Length; j++)
			{
				Vector3 vector2 = localToWorldMatrix.MultiplyPoint3x4(iOSlot.linePoints[j]);
				Vector3 pos = Vector3.Lerp(vector2, vector, 0.5f);
				float num = Vector3.Distance(vector2, vector);
				Quaternion rot = Quaternion.LookRotation((vector2 - vector).normalized);
				GameObject obj = base.gameManager.CreatePrefab("assets/prefabs/misc/ioentitypreventbuilding.prefab", pos, rot);
				obj.transform.SetParent(base.transform);
				if (obj.TryGetComponent<CapsuleCollider>(out var component))
				{
					component.height = num + component.radius;
					spawnedColliders.Add(component);
				}
				if (obj.TryGetComponent<ColliderInfo_Pipe>(out var component2))
				{
					component2.OutputSlotIndex = i;
					component2.ParentEntity = this;
				}
				vector = vector2;
			}
		}
	}

	private void ClearIndustrialPreventBuilding()
	{
		foreach (Collider spawnedCollider in spawnedColliders)
		{
			base.gameManager.Retire(spawnedCollider.gameObject);
		}
		spawnedColliders.Clear();
	}
}
