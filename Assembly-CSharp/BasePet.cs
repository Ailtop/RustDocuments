using System.Collections.Generic;
using Rust;
using UnityEngine;

public class BasePet : NPCPlayer, IThinker
{
	public static Dictionary<ulong, BasePet> ActivePetByOwnerID = new Dictionary<ulong, BasePet>();

	[ServerVar]
	public static bool queuedMovementsAllowed = true;

	[ServerVar]
	public static bool onlyQueueBaseNavMovements = true;

	[ServerVar]
	[Help("How many miliseconds to budget for processing pet movements per frame")]
	public static float movementupdatebudgetms = 1f;

	public float BaseAttackRate = 2f;

	public float BaseAttackDamge = 20f;

	public DamageType AttackDamageType = DamageType.Slash;

	public GameObjectRef mapMarkerPrefab;

	private BaseEntity _mapMarkerInstance;

	[HideInInspector]
	public bool inQueue;

	public static Queue<BasePet> _movementProcessQueue = new Queue<BasePet>();

	public PetBrain Brain { get; protected set; }

	public override float StartHealth()
	{
		return startHealth;
	}

	public override float StartMaxHealth()
	{
		return startHealth;
	}

	public override float MaxHealth()
	{
		return _maxHealth;
	}

	public static void ProcessMovementQueue()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = movementupdatebudgetms / 1000f;
		while (_movementProcessQueue.Count > 0 && Time.realtimeSinceStartup < realtimeSinceStartup + num)
		{
			BasePet basePet = _movementProcessQueue.Dequeue();
			if (basePet != null)
			{
				basePet.DoBudgetedMoveUpdate();
				basePet.inQueue = false;
			}
		}
	}

	public void DoBudgetedMoveUpdate()
	{
		if (Brain != null)
		{
			Brain.DoMovementTick();
		}
	}

	public override bool IsLoadBalanced()
	{
		return true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Brain = GetComponent<PetBrain>();
		if (!base.isClient)
		{
			AIThinkManager.AddPet(this);
		}
	}

	public void CreateMapMarker()
	{
		if (_mapMarkerInstance != null)
		{
			_mapMarkerInstance.Kill();
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(mapMarkerPrefab?.resourcePath, Vector3.zero, Quaternion.identity);
		baseEntity.OwnerID = base.OwnerID;
		baseEntity.Spawn();
		baseEntity.SetParent(this);
		_mapMarkerInstance = baseEntity;
	}

	internal override void DoServerDestroy()
	{
		if (Brain.OwningPlayer != null)
		{
			Brain.OwningPlayer.ClearClientPetLink();
		}
		AIThinkManager.RemovePet(this);
		base.DoServerDestroy();
	}

	public virtual void TryThink()
	{
		ServerThink_Internal();
	}

	public override void ServerThink(float delta)
	{
		base.ServerThink(delta);
		if (Brain.ShouldServerThink())
		{
			Brain.DoThink();
		}
	}

	public void ApplyPetStatModifiers()
	{
		if (inventory == null)
		{
			return;
		}
		for (int i = 0; i < inventory.containerWear.capacity; i++)
		{
			Item slot = inventory.containerWear.GetSlot(i);
			if (slot != null)
			{
				ItemModPetStats component = slot.info.GetComponent<ItemModPetStats>();
				if (component != null)
				{
					component.Apply(this);
				}
			}
		}
		Heal(MaxHealth());
	}

	private void OnPhysicsNeighbourChanged()
	{
		if (Brain != null && Brain.Navigator != null)
		{
			Brain.Navigator.ForceToGround();
		}
	}
}
