using ConVar;
using Oxide.Core;
using UnityEngine;

public class DroppedItem : WorldItem
{
	[Header("DroppedItem")]
	public GameObject itemModel;

	private Collider childCollider;

	public override float GetNetworkTime()
	{
		return UnityEngine.Time.fixedTime;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (GetDespawnDuration() < float.PositiveInfinity)
		{
			Invoke(IdleDestroy, GetDespawnDuration());
		}
		ReceiveCollisionMessages(b: true);
	}

	public virtual float GetDespawnDuration()
	{
		return item?.GetDespawnDuration() ?? Server.itemdespawn;
	}

	public void IdleDestroy()
	{
		DestroyItem();
		Kill();
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (item != null)
		{
			DroppedItem droppedItem = hitEntity as DroppedItem;
			if (!(droppedItem == null) && droppedItem.item != null && !(droppedItem.item.info != item.info))
			{
				droppedItem.OnDroppedOn(this);
			}
		}
	}

	public void OnDroppedOn(DroppedItem di)
	{
		if (item == null || di.item == null || Interface.CallHook("CanCombineDroppedItem", this, di) != null || item.info.stackable <= 1 || di.item.info != item.info || (di.item.IsBlueprint() && di.item.blueprintTarget != item.blueprintTarget) || (di.item.hasCondition && di.item.condition != di.item.maxCondition) || (item.hasCondition && item.condition != item.maxCondition))
		{
			return;
		}
		if (di.item.info != null)
		{
			if (di.item.info.amountType == ItemDefinition.AmountType.Genetics)
			{
				int num = ((di.item.instanceData != null) ? di.item.instanceData.dataInt : (-1));
				int num2 = ((item.instanceData != null) ? item.instanceData.dataInt : (-1));
				if (num != num2)
				{
					return;
				}
			}
			if ((di.item.info.GetComponent<ItemModSign>() != null && ItemModAssociatedEntity<SignContent>.GetAssociatedEntity(di.item) != null) || (item.info != null && item.info.GetComponent<ItemModSign>() != null && ItemModAssociatedEntity<SignContent>.GetAssociatedEntity(item) != null))
			{
				return;
			}
		}
		int num3 = di.item.amount + item.amount;
		if (num3 <= item.info.stackable && num3 != 0)
		{
			di.DestroyItem();
			di.Kill();
			item.amount = num3;
			item.MarkDirty();
			if (GetDespawnDuration() < float.PositiveInfinity)
			{
				Invoke(IdleDestroy, GetDespawnDuration());
			}
			Effect.server.Run("assets/bundled/prefabs/fx/notice/stack.world.fx.prefab", this, 0u, Vector3.zero, Vector3.zero);
			Interface.CallHook("OnDroppedItemCombined", this);
		}
	}

	internal override void OnParentRemoved()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component == null)
		{
			base.OnParentRemoved();
			return;
		}
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		SetParent(null);
		if (UnityEngine.Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out var hitInfo, 2f, 27328512) && position.y < hitInfo.point.y)
		{
			position += Vector3.up * 1.5f;
		}
		base.transform.position = position;
		base.transform.rotation = rotation;
		ConVar.Physics.ApplyDropped(component);
		component.isKinematic = false;
		component.useGravity = true;
		component.WakeUp();
		if (GetDespawnDuration() < float.PositiveInfinity)
		{
			Invoke(IdleDestroy, GetDespawnDuration());
		}
	}

	public override void PostInitShared()
	{
		base.PostInitShared();
		GameObject gameObject = null;
		gameObject = ((item == null || !item.info.worldModelPrefab.isValid) ? Object.Instantiate(itemModel) : item.info.worldModelPrefab.Instantiate());
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		TransformEx.SetLayerRecursive(gameObject, base.gameObject.layer);
		childCollider = gameObject.GetComponent<Collider>();
		if ((bool)childCollider)
		{
			childCollider.enabled = false;
			childCollider.enabled = true;
		}
		if (base.isServer)
		{
			WorldModel component = gameObject.GetComponent<WorldModel>();
			float mass = (component ? component.mass : 1f);
			float drag = 0.1f;
			float angularDrag = 0.1f;
			Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
			rigidbody.mass = mass;
			rigidbody.drag = drag;
			rigidbody.angularDrag = angularDrag;
			rigidbody.interpolation = RigidbodyInterpolation.None;
			ConVar.Physics.ApplyDropped(rigidbody);
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		if (item != null)
		{
			PhysicsEffects component2 = base.gameObject.GetComponent<PhysicsEffects>();
			if (component2 != null)
			{
				component2.entity = this;
				if (item.info.physImpactSoundDef != null)
				{
					component2.physImpactSoundDef = item.info.physImpactSoundDef;
				}
			}
		}
		gameObject.SetActive(value: true);
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}
}
