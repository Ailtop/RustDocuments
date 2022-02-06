using System;
using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust.Modular;
using UnityEngine;

public abstract class BaseModularVehicle : GroundVehicle, PlayerInventory.ICanMoveFrom, IPrefabPreProcess
{
	public bool inEditableLocation;

	public bool prevEditable;

	public bool immuneToDecay;

	public Vector3 realLocalCOM;

	public Item AssociatedItemInstance;

	private bool disablePhysics;

	[Header("Modular Vehicle")]
	[HideInInspector]
	public float mass;

	[SerializeField]
	public List<ModularVehicleSocket> moduleSockets;

	[SerializeField]
	public Transform centreOfMassTransform;

	[SerializeField]
	public Transform waterSample;

	[SerializeField]
	public LODGroup lodGroup;

	public const Flags FLAG_KINEMATIC = Flags.Reserved6;

	public Dictionary<BaseVehicleModule, Action> moduleAddActions = new Dictionary<BaseVehicleModule, Action>();

	public ModularVehicleInventory Inventory { get; set; }

	public Vector3 CentreOfMass => centreOfMassTransform.localPosition;

	public int NumAttachedModules => AttachedModuleEntities.Count;

	public bool HasAnyModules => AttachedModuleEntities.Count > 0;

	public List<BaseVehicleModule> AttachedModuleEntities { get; } = new List<BaseVehicleModule>();


	public int TotalSockets => moduleSockets.Count;

	public int NumFreeSockets
	{
		get
		{
			int num = 0;
			for (int i = 0; i < NumAttachedModules; i++)
			{
				num += AttachedModuleEntities[i].GetNumSocketsTaken();
			}
			return TotalSockets - num;
		}
	}

	public float TotalMass { get; set; }

	public bool IsKinematic => HasFlag(Flags.Reserved6);

	public virtual bool IsLockable => false;

	public bool HasInited { get; private set; }

	public static ItemDefinition AssociatedItemDef => ((BaseCombatEntity)/*Error: ldarg 0 (out-of-bounds)*/).repair.itemTarget;

	public bool IsEditableNow
	{
		get
		{
			if (base.isServer)
			{
				if (inEditableLocation)
				{
					return CouldBeEdited();
				}
				return false;
			}
			return false;
		}
	}

	public override void ServerInit()
	{
		//IL_003c: Incompatible stack heights: 0 vs 1
		base.ServerInit();
		if (!disablePhysics)
		{
			rigidBody.isKinematic = false;
		}
		prevEditable = IsEditableNow;
		if (Inventory == null)
		{
			Inventory = new ModularVehicleInventory(this, AssociatedItemDef, true);
		}
	}

	public override void PreServerLoad()
	{
		//IL_001c: Incompatible stack heights: 0 vs 1
		base.PreServerLoad();
		if (Inventory == null)
		{
			Inventory = new ModularVehicleInventory(this, AssociatedItemDef, false);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (Inventory != null && Inventory.UID == 0)
		{
			Inventory.GiveUIDs();
		}
		SetFlag(Flags.Open, false);
	}

	public override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (Inventory != null)
		{
			Inventory.Dispose();
		}
	}

	public override float MaxVelocity()
	{
		return Mathf.Max(GetMaxForwardSpeed() * 1.3f, 30f);
	}

	public abstract bool IsComplete();

	public bool CouldBeEdited()
	{
		if (!AnyMounted())
		{
			return !IsDead();
		}
		return false;
	}

	public void DisablePhysics()
	{
		disablePhysics = true;
		rigidBody.isKinematic = true;
	}

	public void EnablePhysics()
	{
		disablePhysics = false;
		rigidBody.isKinematic = false;
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (IsEditableNow != prevEditable)
		{
			SendNetworkUpdate();
			prevEditable = IsEditableNow;
		}
		SetFlag(Flags.Reserved6, rigidBody.isKinematic);
	}

	public override bool MountEligable(BasePlayer player)
	{
		if (!base.MountEligable(player))
		{
			return false;
		}
		if (IsDead())
		{
			return false;
		}
		if (HasDriver() && base.Velocity.magnitude >= 2f)
		{
			return false;
		}
		return true;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.modularVehicle = Pool.Get<ModularVehicle>();
		info.msg.modularVehicle.editable = IsEditableNow;
	}

	public bool CanMoveFrom(BasePlayer player, Item item)
	{
		BaseVehicleModule moduleForItem = GetModuleForItem(item);
		if (moduleForItem != null)
		{
			object obj = Interface.CallHook("OnVehicleModuleMove", moduleForItem, this, player);
			if (obj != null)
			{
				if (!(obj is bool))
				{
					return false;
				}
				return (bool)obj;
			}
			return moduleForItem.CanBeMovedNow();
		}
		return true;
	}

	protected abstract Vector3 GetCOMMultiplier();

	public abstract void ModuleHurt(BaseVehicleModule hurtModule, HitInfo info);

	public abstract void ModuleReachedZeroHealth();

	public bool TryAddModule(Item moduleItem, int socketIndex)
	{
		string failureReason;
		if (!ModuleCanBeAdded(moduleItem, socketIndex, out failureReason))
		{
			Debug.LogError(GetType().Name + ": Can't add module: " + failureReason);
			return false;
		}
		bool num = Inventory.TryAddModuleItem(moduleItem, socketIndex);
		if (!num)
		{
			Debug.LogError(GetType().Name + ": Couldn't add new item!");
		}
		return num;
	}

	public bool TryAddModule(Item moduleItem)
	{
		ItemModVehicleModule component = moduleItem.info.GetComponent<ItemModVehicleModule>();
		if (component == null)
		{
			return false;
		}
		int socketsTaken = component.socketsTaken;
		int num = Inventory.TryGetFreeSocket(socketsTaken);
		if (num < 0)
		{
			return false;
		}
		return TryAddModule(moduleItem, num);
	}

	public bool ModuleCanBeAdded(Item moduleItem, int socketIndex, out string failureReason)
	{
		if (!base.isServer)
		{
			failureReason = "Can only add modules on server";
			return false;
		}
		if (moduleItem == null)
		{
			failureReason = "Module item is null";
			return false;
		}
		if (moduleItem.info.category != ItemCategory.Component)
		{
			failureReason = "Not a component type item";
			return false;
		}
		ItemModVehicleModule component = moduleItem.info.GetComponent<ItemModVehicleModule>();
		if (component == null)
		{
			failureReason = "Not the right item module type";
			return false;
		}
		int socketsTaken = component.socketsTaken;
		if (socketIndex < 0)
		{
			socketIndex = Inventory.TryGetFreeSocket(socketsTaken);
		}
		if (!Inventory.SocketsAreFree(socketIndex, socketsTaken, moduleItem))
		{
			failureReason = "One or more desired sockets already in use";
			return false;
		}
		failureReason = string.Empty;
		return true;
	}

	public BaseVehicleModule CreatePhysicalModuleEntity(Item moduleItem, ItemModVehicleModule itemModModule, int socketIndex)
	{
		Vector3 worldPosition = moduleSockets[socketIndex].WorldPosition;
		Quaternion worldRotation = moduleSockets[socketIndex].WorldRotation;
		BaseVehicleModule baseVehicleModule = itemModModule.CreateModuleEntity(this, worldPosition, worldRotation);
		baseVehicleModule.AssociatedItemInstance = moduleItem;
		SetUpModule(baseVehicleModule, moduleItem);
		return baseVehicleModule;
	}

	public void SetUpModule(BaseVehicleModule moduleEntity, Item moduleItem)
	{
		moduleEntity.InitializeHealth(moduleItem.condition, moduleItem.maxCondition);
		if (moduleItem.condition < moduleItem.maxCondition)
		{
			moduleEntity.SendNetworkUpdate();
		}
	}

	public Item GetVehicleItem(uint itemUID)
	{
		Item item = Inventory.ChassisContainer.FindItemByUID(itemUID);
		if (item == null)
		{
			item = Inventory.ModuleContainer.FindItemByUID(itemUID);
		}
		return item;
	}

	public BaseVehicleModule GetModuleForItem(Item item)
	{
		if (item == null)
		{
			return null;
		}
		foreach (BaseVehicleModule attachedModuleEntity in AttachedModuleEntities)
		{
			if (attachedModuleEntity.AssociatedItemInstance == item)
			{
				return attachedModuleEntity;
			}
		}
		return null;
	}

	public void SetMass(float mass)
	{
		TotalMass = mass;
		rigidBody.mass = TotalMass;
	}

	public void SetCOM(Vector3 com)
	{
		realLocalCOM = com;
		rigidBody.centerOfMass = Vector3.Scale(realLocalCOM, GetCOMMultiplier());
	}

	public override void InitShared()
	{
		base.InitShared();
		AddMass(mass, CentreOfMass, base.transform.position);
		HasInited = true;
		foreach (BaseVehicleModule attachedModuleEntity in AttachedModuleEntities)
		{
			attachedModuleEntity.RefreshConditionals(false);
		}
	}

	public override void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(process, rootObj, name, serverside, clientside, bundling);
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			mass = component.mass;
		}
	}

	public virtual bool PlayerCanUseThis(BasePlayer player, ModularCarLock.LockType lockType)
	{
		return true;
	}

	public bool TryDeduceSocketIndex(BaseVehicleModule addedModule, out int index)
	{
		if (addedModule.FirstSocketIndex >= 0)
		{
			index = addedModule.FirstSocketIndex;
			return index >= 0;
		}
		index = -1;
		for (int i = 0; i < moduleSockets.Count; i++)
		{
			if (Vector3.SqrMagnitude(moduleSockets[i].WorldPosition - addedModule.transform.position) < 0.1f)
			{
				index = i;
				return true;
			}
		}
		return false;
	}

	public void AddMass(float moduleMass, Vector3 moduleCOM, Vector3 moduleWorldPos)
	{
		if (base.isServer)
		{
			Vector3 vector = base.transform.InverseTransformPoint(moduleWorldPos) + moduleCOM;
			if (TotalMass == 0f)
			{
				SetMass(moduleMass);
				SetCOM(vector);
				return;
			}
			float num = TotalMass + moduleMass;
			Vector3 cOM = realLocalCOM * (TotalMass / num) + vector * (moduleMass / num);
			SetMass(num);
			SetCOM(cOM);
		}
	}

	public void RemoveMass(float moduleMass, Vector3 moduleCOM, Vector3 moduleWorldPos)
	{
		if (base.isServer)
		{
			float num = TotalMass - moduleMass;
			Vector3 vector = base.transform.InverseTransformPoint(moduleWorldPos) + moduleCOM;
			Vector3 cOM = (realLocalCOM - vector * (moduleMass / TotalMass)) / (num / TotalMass);
			SetMass(num);
			SetCOM(cOM);
		}
	}

	public bool TryGetModuleAt(int socketIndex, out BaseVehicleModule result)
	{
		if (socketIndex < 0 || socketIndex >= moduleSockets.Count)
		{
			result = null;
			return false;
		}
		foreach (BaseVehicleModule attachedModuleEntity in AttachedModuleEntities)
		{
			int firstSocketIndex = attachedModuleEntity.FirstSocketIndex;
			int num = firstSocketIndex + attachedModuleEntity.GetNumSocketsTaken() - 1;
			if (firstSocketIndex <= socketIndex && num >= socketIndex)
			{
				result = attachedModuleEntity;
				return true;
			}
		}
		result = null;
		return false;
	}

	public ModularVehicleSocket GetSocket(int index)
	{
		if (index < 0 || index >= moduleSockets.Count)
		{
			return null;
		}
		return moduleSockets[index];
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		ModularVehicle modularVehicle = info.msg.modularVehicle;
	}

	public override bool CanPushNow(BasePlayer pusher)
	{
		if (!base.CanPushNow(pusher))
		{
			return false;
		}
		if (!IsKinematic)
		{
			return !IsEditableNow;
		}
		return false;
	}

	public override void OnChildAdded(BaseEntity childEntity)
	{
		base.OnChildAdded(childEntity);
		BaseVehicleModule module;
		if ((object)(module = childEntity as BaseVehicleModule) != null)
		{
			Action action = delegate
			{
				ModuleEntityAdded(module);
			};
			moduleAddActions[module] = action;
			module.Invoke(action, 0f);
		}
	}

	public override void OnChildRemoved(BaseEntity childEntity)
	{
		base.OnChildRemoved(childEntity);
		BaseVehicleModule removedModule;
		if ((object)(removedModule = childEntity as BaseVehicleModule) != null)
		{
			ModuleEntityRemoved(removedModule);
		}
	}

	public virtual void ModuleEntityAdded(BaseVehicleModule addedModule)
	{
		if (AttachedModuleEntities.Contains(addedModule))
		{
			return;
		}
		if (base.isServer && (this == null || IsDead() || base.IsDestroyed))
		{
			if (addedModule != null && !addedModule.IsDestroyed)
			{
				addedModule.Kill();
			}
			return;
		}
		int index = -1;
		if (base.isServer && addedModule.AssociatedItemInstance != null)
		{
			index = addedModule.AssociatedItemInstance.position;
		}
		if (index == -1 && !TryDeduceSocketIndex(addedModule, out index))
		{
			string text = $"{GetType().Name}: Couldn't get socket index from position ({addedModule.transform.position}).";
			for (int i = 0; i < moduleSockets.Count; i++)
			{
				text += $" Sqr dist to socket {i} at {moduleSockets[i].WorldPosition} is {Vector3.SqrMagnitude(moduleSockets[i].WorldPosition - addedModule.transform.position)}.";
			}
			Debug.LogError(text, addedModule.gameObject);
			return;
		}
		if (moduleAddActions.ContainsKey(addedModule))
		{
			moduleAddActions.Remove(addedModule);
		}
		AttachedModuleEntities.Add(addedModule);
		addedModule.ModuleAdded(this, index);
		AddMass(addedModule.Mass, addedModule.CentreOfMass, addedModule.transform.position);
		if (base.isServer && !Inventory.TrySyncModuleInventory(addedModule, index))
		{
			Debug.LogError($"{GetType().Name}: Unable to add module {addedModule.name} to socket ({index}). Destroying it.", base.gameObject);
			addedModule.Kill();
			AttachedModuleEntities.Remove(addedModule);
		}
		else
		{
			RefreshModulesExcept(addedModule);
		}
	}

	public virtual void ModuleEntityRemoved(BaseVehicleModule removedModule)
	{
		if (!base.IsDestroyed)
		{
			if (moduleAddActions.ContainsKey(removedModule))
			{
				removedModule.CancelInvoke(moduleAddActions[removedModule]);
				moduleAddActions.Remove(removedModule);
			}
			if (AttachedModuleEntities.Contains(removedModule))
			{
				RemoveMass(removedModule.Mass, removedModule.CentreOfMass, removedModule.transform.position);
				AttachedModuleEntities.Remove(removedModule);
				removedModule.ModuleRemoved();
				RefreshModulesExcept(removedModule);
			}
		}
	}

	public void RefreshModulesExcept(BaseVehicleModule ignoredModule)
	{
		foreach (BaseVehicleModule attachedModuleEntity in AttachedModuleEntities)
		{
			if (attachedModuleEntity != ignoredModule)
			{
				attachedModuleEntity.OtherVehicleModulesChanged();
			}
		}
	}
}
