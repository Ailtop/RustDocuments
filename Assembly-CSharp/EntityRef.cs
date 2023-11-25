using UnityEngine;

public struct EntityRef
{
	internal BaseEntity ent_cached;

	internal NetworkableId id_cached;

	public NetworkableId uid
	{
		get
		{
			if (BaseNetworkableEx.IsValid(ent_cached))
			{
				id_cached = ent_cached.net.ID;
			}
			return id_cached;
		}
		set
		{
			id_cached = value;
			if (!id_cached.IsValid)
			{
				ent_cached = null;
			}
			else if (!BaseNetworkableEx.IsValid(ent_cached) || !(ent_cached.net.ID == id_cached))
			{
				ent_cached = null;
			}
		}
	}

	public bool IsSet()
	{
		return id_cached.IsValid;
	}

	public bool IsValid(bool serverside)
	{
		return BaseNetworkableEx.IsValid(Get(serverside));
	}

	public void Set(BaseEntity ent)
	{
		ent_cached = ent;
		id_cached = default(NetworkableId);
		if (BaseNetworkableEx.IsValid(ent_cached))
		{
			id_cached = ent_cached.net.ID;
		}
	}

	public BaseEntity Get(bool serverside)
	{
		if (ent_cached == null && id_cached.IsValid)
		{
			if (serverside)
			{
				ent_cached = BaseNetworkable.serverEntities.Find(id_cached) as BaseEntity;
			}
			else
			{
				Debug.LogWarning("EntityRef: Looking for clientside entities on pure server!");
			}
		}
		if (!BaseNetworkableEx.IsValid(ent_cached))
		{
			ent_cached = null;
		}
		return ent_cached;
	}
}
public struct EntityRef<T> where T : BaseEntity
{
	private EntityRef entityRef;

	public bool IsSet => entityRef.IsSet();

	public NetworkableId uid
	{
		get
		{
			return entityRef.uid;
		}
		set
		{
			entityRef.uid = value;
		}
	}

	public EntityRef(NetworkableId uid)
	{
		entityRef = new EntityRef
		{
			uid = uid
		};
	}

	public bool IsValid(bool serverside)
	{
		return BaseNetworkableEx.IsValid(Get(serverside));
	}

	public void Set(T entity)
	{
		entityRef.Set(entity);
	}

	public T Get(bool serverside)
	{
		BaseEntity baseEntity = entityRef.Get(serverside);
		if ((object)baseEntity == null)
		{
			return null;
		}
		if (!(baseEntity is T result))
		{
			Set(null);
			return null;
		}
		return result;
	}

	public bool TryGet(bool serverside, out T entity)
	{
		entity = Get(serverside);
		return (object)entity != null;
	}
}
