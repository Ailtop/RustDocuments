using System;
using UnityEngine;

public class ConditionalModel : PrefabAttribute
{
	public GameObjectRef prefab;

	public bool onClient = true;

	public bool onServer = true;

	[NonSerialized]
	public ModelConditionTest[] conditions;

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.AttributeSetup(rootObj, name, serverside, clientside, bundling);
		conditions = GetComponentsInChildren<ModelConditionTest>(includeInactive: true);
	}

	public bool RunTests(BaseEntity parent)
	{
		for (int i = 0; i < conditions.Length; i++)
		{
			if (!conditions[i].DoTest(parent))
			{
				return false;
			}
		}
		return true;
	}

	public GameObject InstantiateSkin(BaseEntity parent)
	{
		if (!onServer && isServer)
		{
			return null;
		}
		GameObject gameObject = gameManager.CreatePrefab(prefab.resourcePath, parent.transform, active: false);
		if ((bool)gameObject)
		{
			gameObject.transform.localPosition = worldPosition;
			gameObject.transform.localRotation = worldRotation;
			PoolableEx.AwakeFromInstantiate(gameObject);
		}
		return gameObject;
	}

	protected override Type GetIndexedType()
	{
		return typeof(ConditionalModel);
	}
}
