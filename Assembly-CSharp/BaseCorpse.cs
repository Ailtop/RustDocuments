using System;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class BaseCorpse : BaseCombatEntity
{
	public GameObjectRef prefabRagdoll;

	public BaseEntity parentEnt;

	[NonSerialized]
	public ResourceDispenser resourceDispenser;

	[NonSerialized]
	public SpawnGroup spawnGroup;

	public override TraitFlag Traits => base.Traits | TraitFlag.Food | TraitFlag.Meat;

	public override void ResetState()
	{
		spawnGroup = null;
		base.ResetState();
	}

	public override void ServerInit()
	{
		SetupRigidBody();
		ResetRemovalTime();
		resourceDispenser = GetComponent<ResourceDispenser>();
		base.ServerInit();
	}

	public virtual void InitCorpse(BaseEntity pr)
	{
		parentEnt = pr;
		base.transform.SetPositionAndRotation(parentEnt.CenterPoint(), parentEnt.transform.rotation);
		SpawnPointInstance component = GetComponent<SpawnPointInstance>();
		if (component != null)
		{
			spawnGroup = component.parentSpawnPointUser as SpawnGroup;
		}
	}

	public virtual bool CanRemove()
	{
		return true;
	}

	public void RemoveCorpse()
	{
		if (!CanRemove())
		{
			ResetRemovalTime();
		}
		else
		{
			Kill();
		}
	}

	public void ResetRemovalTime(float dur)
	{
		using (TimeWarning.New("ResetRemovalTime"))
		{
			if (IsInvoking(RemoveCorpse))
			{
				CancelInvoke(RemoveCorpse);
			}
			Invoke(RemoveCorpse, dur);
		}
	}

	public virtual float GetRemovalTime()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode != null)
		{
			return activeGameMode.CorpseRemovalTime(this);
		}
		return Server.corpsedespawn;
	}

	public void ResetRemovalTime()
	{
		ResetRemovalTime(GetRemovalTime());
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.corpse = Facepunch.Pool.Get<Corpse>();
		if (BaseNetworkableEx.IsValid(parentEnt))
		{
			info.msg.corpse.parentID = parentEnt.net.ID;
		}
	}

	public void TakeChildren(BaseEntity takeChildrenFrom)
	{
		if (takeChildrenFrom.children == null)
		{
			return;
		}
		using (TimeWarning.New("Corpse.TakeChildren"))
		{
			BaseEntity[] array = takeChildrenFrom.children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SwitchParent(this);
			}
		}
	}

	public override void ApplyInheritedVelocity(Vector3 velocity)
	{
	}

	private Rigidbody SetupRigidBody()
	{
		if (base.isServer)
		{
			GameObject gameObject = base.gameManager.FindPrefab(prefabRagdoll.resourcePath);
			if (gameObject == null)
			{
				return null;
			}
			Ragdoll component = gameObject.GetComponent<Ragdoll>();
			if (component == null)
			{
				return null;
			}
			if (component.primaryBody == null)
			{
				Debug.LogError("[BaseCorpse] ragdoll.primaryBody isn't set!" + component.gameObject.name);
				return null;
			}
			BoxCollider component2 = component.primaryBody.GetComponent<BoxCollider>();
			if (component2 == null)
			{
				Debug.LogError("Ragdoll has unsupported primary collider (make it supported) ", component);
				return null;
			}
			BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
			boxCollider.size = component2.size * 2f;
			boxCollider.center = component2.center;
			boxCollider.sharedMaterial = component2.sharedMaterial;
		}
		Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
		if (rigidbody == null)
		{
			Debug.LogError("[BaseCorpse] already has a RigidBody defined - and it shouldn't!" + base.gameObject.name);
			return null;
		}
		rigidbody.mass = 10f;
		rigidbody.useGravity = true;
		rigidbody.drag = 0.5f;
		rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		if (base.isServer)
		{
			Buoyancy component3 = GetComponent<Buoyancy>();
			if (component3 != null)
			{
				component3.rigidBody = rigidbody;
			}
			ConVar.Physics.ApplyDropped(rigidbody);
			Vector3 velocity = Vector3Ex.Range(-1f, 1f);
			velocity.y += 1f;
			rigidbody.velocity = velocity;
			rigidbody.angularVelocity = Vector3Ex.Range(-10f, 10f);
		}
		return rigidbody;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.corpse != null)
		{
			Load(info.msg.corpse);
		}
	}

	private void Load(Corpse corpse)
	{
		if (base.isServer)
		{
			parentEnt = BaseNetworkable.serverEntities.Find(corpse.parentID) as BaseEntity;
		}
		_ = base.isClient;
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer)
		{
			ResetRemovalTime();
			if ((bool)resourceDispenser)
			{
				resourceDispenser.OnAttacked(info);
			}
			if (!info.DidGather)
			{
				base.OnAttacked(info);
			}
		}
	}

	public override string Categorize()
	{
		return "corpse";
	}

	public override void Eat(BaseNpc baseNpc, float timeSpent)
	{
		ResetRemovalTime();
		Hurt(timeSpent * 5f);
		baseNpc.AddCalories(timeSpent * 2f);
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}
}
