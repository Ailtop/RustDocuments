using ConVar;
using Facepunch;
using Rust;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager
{
	public static GameManager server = new GameManager(clientside: false, serverside: true);

	public PrefabPreProcess preProcessed;

	public PrefabPoolCollection pool;

	public bool Clientside;

	public bool Serverside;

	public void Reset()
	{
		pool.Clear();
	}

	public GameManager(bool clientside, bool serverside)
	{
		Clientside = clientside;
		Serverside = serverside;
		preProcessed = new PrefabPreProcess(clientside, serverside);
		pool = new PrefabPoolCollection();
	}

	public GameObject FindPrefab(uint prefabID)
	{
		string text = StringPool.Get(prefabID);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return FindPrefab(text);
	}

	public GameObject FindPrefab(BaseEntity ent)
	{
		if (ent == null)
		{
			return null;
		}
		return FindPrefab(ent.PrefabName);
	}

	public GameObject FindPrefab(string strPrefab)
	{
		GameObject gameObject = preProcessed.Find(strPrefab);
		if (gameObject != null)
		{
			return gameObject;
		}
		gameObject = FileSystem.LoadPrefab(strPrefab);
		if (gameObject == null)
		{
			return null;
		}
		preProcessed.Process(strPrefab, gameObject);
		GameObject gameObject2 = preProcessed.Find(strPrefab);
		if (!(gameObject2 != null))
		{
			return gameObject;
		}
		return gameObject2;
	}

	public GameObject CreatePrefab(string strPrefab, Vector3 pos, Quaternion rot, Vector3 scale, bool active = true)
	{
		GameObject gameObject = Instantiate(strPrefab, pos, rot);
		if ((bool)gameObject)
		{
			gameObject.transform.localScale = scale;
			if (active)
			{
				PoolableEx.AwakeFromInstantiate(gameObject);
			}
		}
		return gameObject;
	}

	public GameObject CreatePrefab(string strPrefab, Vector3 pos, Quaternion rot, bool active = true)
	{
		GameObject gameObject = Instantiate(strPrefab, pos, rot);
		if ((bool)gameObject && active)
		{
			PoolableEx.AwakeFromInstantiate(gameObject);
		}
		return gameObject;
	}

	public GameObject CreatePrefab(string strPrefab, bool active = true)
	{
		GameObject gameObject = Instantiate(strPrefab, Vector3.zero, Quaternion.identity);
		if ((bool)gameObject && active)
		{
			PoolableEx.AwakeFromInstantiate(gameObject);
		}
		return gameObject;
	}

	public GameObject CreatePrefab(string strPrefab, Transform parent, bool active = true)
	{
		GameObject gameObject = Instantiate(strPrefab, parent.position, parent.rotation);
		if ((bool)gameObject)
		{
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			TransformEx.Identity(gameObject);
			if (active)
			{
				PoolableEx.AwakeFromInstantiate(gameObject);
			}
		}
		return gameObject;
	}

	public BaseEntity CreateEntity(string strPrefab, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), bool startActive = true)
	{
		if (string.IsNullOrEmpty(strPrefab))
		{
			return null;
		}
		GameObject gameObject = CreatePrefab(strPrefab, pos, rot, startActive);
		if (gameObject == null)
		{
			return null;
		}
		BaseEntity component = gameObject.GetComponent<BaseEntity>();
		if (component == null)
		{
			Debug.LogError("CreateEntity called on a prefab that isn't an entity! " + strPrefab);
			Object.Destroy(gameObject);
			return null;
		}
		if (component.CompareTag("CannotBeCreated"))
		{
			Debug.LogWarning("CreateEntity called on a prefab that has the CannotBeCreated tag set. " + strPrefab);
			Object.Destroy(gameObject);
			return null;
		}
		return component;
	}

	private GameObject Instantiate(string strPrefab, Vector3 pos, Quaternion rot)
	{
		if (!strPrefab.IsLower())
		{
			Debug.LogWarning("Converting prefab name to lowercase: " + strPrefab);
			strPrefab = strPrefab.ToLower();
		}
		GameObject gameObject = FindPrefab(strPrefab);
		if (!gameObject)
		{
			Debug.LogError("Couldn't find prefab \"" + strPrefab + "\"");
			return null;
		}
		GameObject gameObject2 = pool.Pop(StringPool.Get(strPrefab), pos, rot);
		if (gameObject2 == null)
		{
			gameObject2 = Facepunch.Instantiate.GameObject(gameObject, pos, rot);
			gameObject2.name = strPrefab;
		}
		else
		{
			gameObject2.transform.localScale = gameObject.transform.localScale;
		}
		if (!Clientside && Serverside && gameObject2.transform.parent == null)
		{
			SceneManager.MoveGameObjectToScene(gameObject2, Rust.Server.EntityScene);
		}
		return gameObject2;
	}

	public static void Destroy(Component component, float delay = 0f)
	{
		if (BaseNetworkableEx.IsValid(component as BaseEntity))
		{
			Debug.LogError("Trying to destroy an entity without killing it first: " + component.name);
		}
		Object.Destroy(component, delay);
	}

	public static void Destroy(GameObject instance, float delay = 0f)
	{
		if ((bool)instance)
		{
			if (BaseNetworkableEx.IsValid(instance.GetComponent<BaseEntity>()))
			{
				Debug.LogError("Trying to destroy an entity without killing it first: " + instance.name);
			}
			Object.Destroy(instance, delay);
		}
	}

	public static void DestroyImmediate(Component component, bool allowDestroyingAssets = false)
	{
		if (BaseNetworkableEx.IsValid(component as BaseEntity))
		{
			Debug.LogError("Trying to destroy an entity without killing it first: " + component.name);
		}
		Object.DestroyImmediate(component, allowDestroyingAssets);
	}

	public static void DestroyImmediate(GameObject instance, bool allowDestroyingAssets = false)
	{
		if (BaseNetworkableEx.IsValid(instance.GetComponent<BaseEntity>()))
		{
			Debug.LogError("Trying to destroy an entity without killing it first: " + instance.name);
		}
		Object.DestroyImmediate(instance, allowDestroyingAssets);
	}

	public void Retire(GameObject instance)
	{
		if (!instance)
		{
			return;
		}
		using (TimeWarning.New("GameManager.Retire"))
		{
			if (BaseNetworkableEx.IsValid(instance.GetComponent<BaseEntity>()))
			{
				Debug.LogError("Trying to retire an entity without killing it first: " + instance.name);
			}
			if (!Rust.Application.isQuitting && ConVar.Pool.enabled && PoolableEx.SupportsPooling(instance))
			{
				pool.Push(instance);
			}
			else
			{
				Object.Destroy(instance);
			}
		}
	}
}
