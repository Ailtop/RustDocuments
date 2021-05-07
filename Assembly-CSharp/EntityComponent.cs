using System;
using UnityEngine;

public class EntityComponent<T> : EntityComponentBase where T : BaseEntity
{
	[NonSerialized]
	public T _baseEntity;

	public T baseEntity
	{
		get
		{
			if ((UnityEngine.Object)_baseEntity == (UnityEngine.Object)null)
			{
				UpdateBaseEntity();
			}
			return _baseEntity;
		}
	}

	protected void UpdateBaseEntity()
	{
		if ((bool)this && (bool)base.gameObject)
		{
			_baseEntity = base.gameObject.ToBaseEntity() as T;
		}
	}

	protected override BaseEntity GetBaseEntity()
	{
		return baseEntity;
	}
}
