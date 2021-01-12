using System;
using System.Collections.Generic;
using UnityEngine;

public class Prefab<T> : Prefab, IComparable<Prefab<T>> where T : Component
{
	public T Component;

	public Prefab(string name, GameObject prefab, T component, GameManager manager, PrefabAttribute.Library attribute)
		: base(name, prefab, manager, attribute)
	{
		Component = component;
	}

	public int CompareTo(Prefab<T> that)
	{
		return CompareTo((Prefab)that);
	}
}
public class Prefab : IComparable<Prefab>
{
	public uint ID;

	public string Name;

	public GameObject Object;

	public GameManager Manager;

	public PrefabAttribute.Library Attribute;

	public PrefabParameters Parameters;

	public static PrefabAttribute.Library DefaultAttribute => PrefabAttribute.server;

	public static GameManager DefaultManager => GameManager.server;

	public Prefab(string name, GameObject prefab, GameManager manager, PrefabAttribute.Library attribute)
	{
		ID = StringPool.Get(name);
		Name = name;
		Object = prefab;
		Manager = manager;
		Attribute = attribute;
		Parameters = (prefab ? prefab.GetComponent<PrefabParameters>() : null);
	}

	public static implicit operator GameObject(Prefab prefab)
	{
		return prefab.Object;
	}

	public int CompareTo(Prefab that)
	{
		if (that == null)
		{
			return 1;
		}
		PrefabPriority prefabPriority = (Parameters != null) ? Parameters.Priority : PrefabPriority.Default;
		return ((that.Parameters != null) ? that.Parameters.Priority : PrefabPriority.Default).CompareTo(prefabPriority);
	}

	public bool ApplyTerrainAnchors(ref Vector3 pos, Quaternion rot, Vector3 scale, TerrainAnchorMode mode, SpawnFilter filter = null)
	{
		TerrainAnchor[] anchors = Attribute.FindAll<TerrainAnchor>(ID);
		return TerrainAnchorEx.ApplyTerrainAnchors(Object.transform, anchors, ref pos, rot, scale, mode, filter);
	}

	public bool ApplyTerrainAnchors(ref Vector3 pos, Quaternion rot, Vector3 scale, SpawnFilter filter = null)
	{
		TerrainAnchor[] anchors = Attribute.FindAll<TerrainAnchor>(ID);
		return TerrainAnchorEx.ApplyTerrainAnchors(Object.transform, anchors, ref pos, rot, scale, filter);
	}

	public bool ApplyTerrainChecks(Vector3 pos, Quaternion rot, Vector3 scale, SpawnFilter filter = null)
	{
		TerrainCheck[] anchors = Attribute.FindAll<TerrainCheck>(ID);
		return TerrainCheckEx.ApplyTerrainChecks(Object.transform, anchors, pos, rot, scale, filter);
	}

	public bool ApplyTerrainFilters(Vector3 pos, Quaternion rot, Vector3 scale, SpawnFilter filter = null)
	{
		TerrainFilter[] filters = Attribute.FindAll<TerrainFilter>(ID);
		return TerrainFilterEx.ApplyTerrainFilters(Object.transform, filters, pos, rot, scale, filter);
	}

	public void ApplyTerrainModifiers(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		TerrainModifier[] modifiers = Attribute.FindAll<TerrainModifier>(ID);
		TerrainModifierEx.ApplyTerrainModifiers(Object.transform, modifiers, pos, rot, scale);
	}

	public void ApplyTerrainPlacements(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		TerrainPlacement[] placements = Attribute.FindAll<TerrainPlacement>(ID);
		TerrainPlacementEx.ApplyTerrainPlacements(Object.transform, placements, pos, rot, scale);
	}

	public bool ApplyWaterChecks(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		WaterCheck[] anchors = Attribute.FindAll<WaterCheck>(ID);
		return WaterCheckEx.ApplyWaterChecks(Object.transform, anchors, pos, rot, scale);
	}

	public void ApplyDecorComponents(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
	{
		DecorComponent[] components = Attribute.FindAll<DecorComponent>(ID);
		DecorComponentEx.ApplyDecorComponents(Object.transform, components, ref pos, ref rot, ref scale);
	}

	public bool CheckEnvironmentVolumes(Vector3 pos, Quaternion rot, Vector3 scale, EnvironmentType type)
	{
		return EnvironmentVolumeEx.CheckEnvironmentVolumes(Object.transform, pos, rot, scale, type);
	}

	public GameObject Spawn(Transform transform, bool active = true)
	{
		return Manager.CreatePrefab(Name, transform, active);
	}

	public GameObject Spawn(Vector3 pos, Quaternion rot, bool active = true)
	{
		return Manager.CreatePrefab(Name, pos, rot, active);
	}

	public GameObject Spawn(Vector3 pos, Quaternion rot, Vector3 scale, bool active = true)
	{
		return Manager.CreatePrefab(Name, pos, rot, scale, active);
	}

	public BaseEntity SpawnEntity(Vector3 pos, Quaternion rot, bool active = true)
	{
		return Manager.CreateEntity(Name, pos, rot, active);
	}

	public static Prefab<T> Load<T>(uint id, GameManager manager = null, PrefabAttribute.Library attribute = null) where T : Component
	{
		if (manager == null)
		{
			manager = DefaultManager;
		}
		if (attribute == null)
		{
			attribute = DefaultAttribute;
		}
		string text = StringPool.Get(id);
		GameObject gameObject = manager.FindPrefab(text);
		T component = gameObject.GetComponent<T>();
		return new Prefab<T>(text, gameObject, component, manager, attribute);
	}

	public static Prefab Load(uint id, GameManager manager = null, PrefabAttribute.Library attribute = null)
	{
		if (manager == null)
		{
			manager = DefaultManager;
		}
		if (attribute == null)
		{
			attribute = DefaultAttribute;
		}
		string text = StringPool.Get(id);
		GameObject prefab = manager.FindPrefab(text);
		return new Prefab(text, prefab, manager, attribute);
	}

	public static Prefab[] Load(string folder, GameManager manager = null, PrefabAttribute.Library attribute = null, bool useProbabilities = true)
	{
		if (string.IsNullOrEmpty(folder))
		{
			return null;
		}
		if (manager == null)
		{
			manager = DefaultManager;
		}
		if (attribute == null)
		{
			attribute = DefaultAttribute;
		}
		string[] array = FindPrefabNames(folder, useProbabilities);
		Prefab[] array2 = new Prefab[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			string text = array[i];
			GameObject prefab = manager.FindPrefab(text);
			array2[i] = new Prefab(text, prefab, manager, attribute);
		}
		return array2;
	}

	public static Prefab<T>[] Load<T>(string folder, GameManager manager = null, PrefabAttribute.Library attribute = null, bool useProbabilities = true) where T : Component
	{
		if (string.IsNullOrEmpty(folder))
		{
			return null;
		}
		return Load<T>(FindPrefabNames(folder, useProbabilities), manager, attribute);
	}

	public static Prefab<T>[] Load<T>(string[] names, GameManager manager = null, PrefabAttribute.Library attribute = null) where T : Component
	{
		if (manager == null)
		{
			manager = DefaultManager;
		}
		if (attribute == null)
		{
			attribute = DefaultAttribute;
		}
		Prefab<T>[] array = new Prefab<T>[names.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string text = names[i];
			GameObject gameObject = manager.FindPrefab(text);
			T component = gameObject.GetComponent<T>();
			array[i] = new Prefab<T>(text, gameObject, component, manager, attribute);
		}
		return array;
	}

	public static Prefab LoadRandom(string folder, ref uint seed, GameManager manager = null, PrefabAttribute.Library attribute = null, bool useProbabilities = true)
	{
		if (string.IsNullOrEmpty(folder))
		{
			return null;
		}
		if (manager == null)
		{
			manager = DefaultManager;
		}
		if (attribute == null)
		{
			attribute = DefaultAttribute;
		}
		string[] array = FindPrefabNames(folder, useProbabilities);
		if (array.Length == 0)
		{
			return null;
		}
		string text = array[SeedRandom.Range(ref seed, 0, array.Length)];
		GameObject prefab = manager.FindPrefab(text);
		return new Prefab(text, prefab, manager, attribute);
	}

	public static Prefab<T> LoadRandom<T>(string folder, ref uint seed, GameManager manager = null, PrefabAttribute.Library attribute = null, bool useProbabilities = true) where T : Component
	{
		if (string.IsNullOrEmpty(folder))
		{
			return null;
		}
		if (manager == null)
		{
			manager = DefaultManager;
		}
		if (attribute == null)
		{
			attribute = DefaultAttribute;
		}
		string[] array = FindPrefabNames(folder, useProbabilities);
		if (array.Length == 0)
		{
			return null;
		}
		string text = array[SeedRandom.Range(ref seed, 0, array.Length)];
		GameObject gameObject = manager.FindPrefab(text);
		T component = gameObject.GetComponent<T>();
		return new Prefab<T>(text, gameObject, component, manager, attribute);
	}

	private static string[] FindPrefabNames(string strPrefab, bool useProbabilities = false)
	{
		strPrefab = strPrefab.TrimEnd('/').ToLower();
		GameObject[] array = FileSystem.LoadPrefabs(strPrefab + "/");
		List<string> list = new List<string>(array.Length);
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			string item = strPrefab + "/" + gameObject.name.ToLower() + ".prefab";
			if (!useProbabilities)
			{
				list.Add(item);
				continue;
			}
			PrefabParameters component = gameObject.GetComponent<PrefabParameters>();
			int num = (!component) ? 1 : component.Count;
			for (int j = 0; j < num; j++)
			{
				list.Add(item);
			}
		}
		list.Sort();
		return list.ToArray();
	}
}
