using System;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public class MiningQuarry : BaseResourceExtractor
{
	[Serializable]
	public enum QuarryType
	{
		None = 0,
		Basic = 1,
		Sulfur = 2,
		HQM = 3
	}

	[Serializable]
	public class ChildPrefab
	{
		public GameObjectRef prefabToSpawn;

		public GameObject origin;

		public BaseEntity instance;

		public void DoSpawn(MiningQuarry owner)
		{
			if (prefabToSpawn.isValid)
			{
				instance = GameManager.server.CreateEntity(prefabToSpawn.resourcePath, origin.transform.localPosition, origin.transform.localRotation);
				instance.SetParent(owner);
				instance.Spawn();
			}
		}
	}

	public Animator beltAnimator;

	public Renderer beltScrollRenderer;

	public int scrollMatIndex = 3;

	public SoundPlayer[] onSounds;

	public float processRate = 5f;

	public float workToAdd = 15f;

	public GameObjectRef bucketDropEffect;

	public GameObject bucketDropTransform;

	public ChildPrefab engineSwitchPrefab;

	public ChildPrefab hopperPrefab;

	public ChildPrefab fuelStoragePrefab;

	public QuarryType staticType;

	public bool isStatic;

	public ResourceDepositManager.ResourceDeposit _linkedDeposit;

	public bool IsEngineOn()
	{
		return HasFlag(Flags.On);
	}

	public void SetOn(bool isOn)
	{
		SetFlag(Flags.On, isOn);
		engineSwitchPrefab.instance.SetFlag(Flags.On, isOn);
		SendNetworkUpdate();
		engineSwitchPrefab.instance.SendNetworkUpdate();
		if (isOn)
		{
			InvokeRepeating(ProcessResources, processRate, processRate);
		}
		else
		{
			CancelInvoke(ProcessResources);
		}
	}

	public void EngineSwitch(bool isOn)
	{
		if (isOn && FuelCheck())
		{
			SetOn(isOn: true);
		}
		else
		{
			SetOn(isOn: false);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!isStatic)
		{
			ResourceDepositManager.ResourceDeposit resourceDeposit = (_linkedDeposit = ResourceDepositManager.GetOrCreate(base.transform.position));
		}
		else
		{
			UpdateStaticDeposit();
		}
		SpawnChildEntities();
		engineSwitchPrefab.instance.SetFlag(Flags.On, HasFlag(Flags.On));
	}

	public void UpdateStaticDeposit()
	{
		if (isStatic)
		{
			if (_linkedDeposit == null)
			{
				_linkedDeposit = new ResourceDepositManager.ResourceDeposit();
			}
			else
			{
				_linkedDeposit._resources.Clear();
			}
			if (staticType == QuarryType.None)
			{
				_linkedDeposit.Add(ItemManager.FindItemDefinition("stones"), 1f, 1000, 0.3f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
				_linkedDeposit.Add(ItemManager.FindItemDefinition("metal.ore"), 1f, 1000, 5f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
				_linkedDeposit.Add(ItemManager.FindItemDefinition("sulfur.ore"), 1f, 1000, 7.5f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
				_linkedDeposit.Add(ItemManager.FindItemDefinition("hq.metal.ore"), 1f, 1000, 75f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
			}
			else if (staticType == QuarryType.Basic)
			{
				_linkedDeposit.Add(ItemManager.FindItemDefinition("metal.ore"), 1f, 1000, 2f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
				_linkedDeposit.Add(ItemManager.FindItemDefinition("stones"), 1f, 1000, 0.3f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
			}
			else if (staticType == QuarryType.Sulfur)
			{
				_linkedDeposit.Add(ItemManager.FindItemDefinition("sulfur.ore"), 1f, 1000, 2f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
			}
			else if (staticType == QuarryType.HQM)
			{
				_linkedDeposit.Add(ItemManager.FindItemDefinition("hq.metal.ore"), 1f, 1000, 30f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
			}
			_linkedDeposit.Add(ItemManager.FindItemDefinition("crude.oil"), 1f, 1000, 10f, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM, liquid: true);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		EngineSwitch(HasFlag(Flags.On));
		UpdateStaticDeposit();
	}

	public void SpawnChildEntities()
	{
		engineSwitchPrefab.DoSpawn(this);
		hopperPrefab.DoSpawn(this);
		fuelStoragePrefab.DoSpawn(this);
	}

	public void ProcessResources()
	{
		if (_linkedDeposit == null || hopperPrefab.instance == null)
		{
			return;
		}
		foreach (ResourceDepositManager.ResourceDeposit.ResourceDepositEntry resource in _linkedDeposit._resources)
		{
			if ((!canExtractLiquid && resource.isLiquid) || (!canExtractSolid && !resource.isLiquid))
			{
				continue;
			}
			resource.workDone += workToAdd;
			if (!(resource.workDone < resource.workNeeded))
			{
				int num = Mathf.FloorToInt(resource.workDone / resource.workNeeded);
				resource.workDone -= (float)num * resource.workNeeded;
				Item item = ItemManager.Create(resource.type, num, 0uL);
				if (Interface.CallHook("OnQuarryGather", this, item) != null)
				{
					item.Remove();
				}
				else if (!item.MoveToContainer(hopperPrefab.instance.GetComponent<StorageContainer>().inventory))
				{
					item.Remove();
					SetOn(isOn: false);
				}
			}
		}
		if (!FuelCheck())
		{
			SetOn(isOn: false);
		}
	}

	public bool FuelCheck()
	{
		Item item = fuelStoragePrefab.instance.GetComponent<StorageContainer>().inventory.FindItemsByItemName("lowgradefuel");
		if (item != null && item.amount >= 1)
		{
			object obj = Interface.CallHook("OnQuarryConsumeFuel", this, item);
			if (obj is Item)
			{
				item = (Item)obj;
			}
			item.UseItem();
			return true;
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (fuelStoragePrefab.instance == null || hopperPrefab.instance == null)
			{
				Debug.Log("Cannot save mining quary because children were null");
				return;
			}
			info.msg.miningQuarry = Pool.Get<ProtoBuf.MiningQuarry>();
			info.msg.miningQuarry.extractor = Pool.Get<ResourceExtractor>();
			info.msg.miningQuarry.extractor.fuelContents = fuelStoragePrefab.instance.GetComponent<StorageContainer>().inventory.Save();
			info.msg.miningQuarry.extractor.outputContents = hopperPrefab.instance.GetComponent<StorageContainer>().inventory.Save();
			info.msg.miningQuarry.staticType = (int)staticType;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.miningQuarry != null)
		{
			if (fuelStoragePrefab.instance == null || hopperPrefab.instance == null)
			{
				Debug.Log("Cannot load mining quary because children were null");
				return;
			}
			fuelStoragePrefab.instance.GetComponent<StorageContainer>().inventory.Load(info.msg.miningQuarry.extractor.fuelContents);
			hopperPrefab.instance.GetComponent<StorageContainer>().inventory.Load(info.msg.miningQuarry.extractor.outputContents);
			staticType = (QuarryType)info.msg.miningQuarry.staticType;
		}
	}

	public void Update()
	{
	}
}
