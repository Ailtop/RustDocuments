using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ServerGib : BaseCombatEntity
{
	public GameObject _gibSource;

	public string _gibName;

	public PhysicMaterial physicsMaterial;

	private MeshCollider meshCollider;

	private Rigidbody rigidBody;

	public override float BoundsPadding()
	{
		return 3f;
	}

	public static List<ServerGib> CreateGibs(string entityToCreatePath, GameObject creator, GameObject gibSource, Vector3 inheritVelocity, float spreadVelocity)
	{
		List<ServerGib> list = new List<ServerGib>();
		MeshRenderer[] componentsInChildren = gibSource.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
			Vector3 normalized = meshRenderer.transform.localPosition.normalized;
			Vector3 vector = creator.transform.localToWorldMatrix.MultiplyPoint(meshRenderer.transform.localPosition) + normalized * 0.5f;
			Quaternion quaternion = creator.transform.rotation * meshRenderer.transform.localRotation;
			BaseEntity baseEntity = GameManager.server.CreateEntity(entityToCreatePath, vector, quaternion);
			if ((bool)baseEntity)
			{
				ServerGib component2 = baseEntity.GetComponent<ServerGib>();
				component2.transform.SetPositionAndRotation(vector, quaternion);
				component2._gibName = meshRenderer.name;
				MeshCollider component3 = meshRenderer.GetComponent<MeshCollider>();
				Mesh physicsMesh = ((component3 != null) ? component3.sharedMesh : component.sharedMesh);
				component2.PhysicsInit(physicsMesh);
				Vector3 vector2 = meshRenderer.transform.localPosition.normalized * spreadVelocity;
				component2.rigidBody.velocity = inheritVelocity + vector2;
				component2.rigidBody.angularVelocity = Vector3Ex.Range(-1f, 1f).normalized * 1f;
				component2.rigidBody.WakeUp();
				component2.Spawn();
				list.Add(component2);
			}
		}
		foreach (ServerGib item in list)
		{
			foreach (ServerGib item2 in list)
			{
				if (!(item == item2))
				{
					Physics.IgnoreCollision(item2.GetCollider(), item.GetCollider(), ignore: true);
				}
			}
		}
		return list;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk && _gibName != "")
		{
			info.msg.servergib = Pool.Get<ProtoBuf.ServerGib>();
			info.msg.servergib.gibName = _gibName;
		}
	}

	public MeshCollider GetCollider()
	{
		return meshCollider;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Invoke(RemoveMe, 1800f);
	}

	public void RemoveMe()
	{
		Kill();
	}

	public virtual void PhysicsInit(Mesh physicsMesh)
	{
		Mesh sharedMesh = null;
		MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
		if (component != null)
		{
			sharedMesh = component.sharedMesh;
			component.sharedMesh = physicsMesh;
		}
		meshCollider = base.gameObject.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = physicsMesh;
		meshCollider.convex = true;
		meshCollider.material = physicsMaterial;
		if (component != null)
		{
			component.sharedMesh = sharedMesh;
		}
		Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
		rigidbody.useGravity = true;
		rigidbody.mass = Mathf.Clamp(meshCollider.bounds.size.magnitude * meshCollider.bounds.size.magnitude * 20f, 10f, 2000f);
		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		if (base.isServer)
		{
			rigidbody.drag = 0.1f;
			rigidbody.angularDrag = 0.1f;
		}
		rigidBody = rigidbody;
		base.gameObject.layer = LayerMask.NameToLayer("Default");
		if (base.isClient)
		{
			rigidbody.isKinematic = true;
		}
	}
}
