using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Rust.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using VLB;

public class PrefabPreProcess : IPrefabProcessor
{
	public static Type[] clientsideOnlyTypes = new Type[38]
	{
		typeof(IClientComponent),
		typeof(SkeletonSkinLod),
		typeof(ImageEffectLayer),
		typeof(NGSS_Directional),
		typeof(VolumetricDustParticles),
		typeof(VolumetricLightBeam),
		typeof(Cloth),
		typeof(MeshFilter),
		typeof(Renderer),
		typeof(AudioLowPassFilter),
		typeof(AudioSource),
		typeof(AudioListener),
		typeof(ParticleSystemRenderer),
		typeof(ParticleSystem),
		typeof(ParticleEmitFromParentObject),
		typeof(ImpostorShadows),
		typeof(Light),
		typeof(LODGroup),
		typeof(Animator),
		typeof(AnimationEvents),
		typeof(PlayerVoiceSpeaker),
		typeof(VoiceProcessor),
		typeof(PlayerVoiceRecorder),
		typeof(ParticleScaler),
		typeof(PostEffectsBase),
		typeof(TOD_ImageEffect),
		typeof(TOD_Scattering),
		typeof(TOD_Rays),
		typeof(Tree),
		typeof(Projector),
		typeof(HttpImage),
		typeof(EventTrigger),
		typeof(StandaloneInputModule),
		typeof(UIBehaviour),
		typeof(Canvas),
		typeof(CanvasRenderer),
		typeof(CanvasGroup),
		typeof(GraphicRaycaster)
	};

	public static Type[] serversideOnlyTypes = new Type[2]
	{
		typeof(IServerComponent),
		typeof(NavMeshObstacle)
	};

	public bool isClientside;

	public bool isServerside;

	public bool isBundling;

	public Dictionary<string, GameObject> prefabList = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

	public List<Component> destroyList = new List<Component>();

	public List<GameObject> cleanupList = new List<GameObject>();

	public PrefabPreProcess(bool clientside, bool serverside, bool bundling = false)
	{
		isClientside = clientside;
		isServerside = serverside;
		isBundling = bundling;
	}

	public GameObject Find(string strPrefab)
	{
		GameObject value;
		if (prefabList.TryGetValue(strPrefab, out value))
		{
			if (value == null)
			{
				prefabList.Remove(strPrefab);
				return null;
			}
			return value;
		}
		return null;
	}

	public bool NeedsProcessing(GameObject go)
	{
		if (go.CompareTag("NoPreProcessing"))
		{
			return false;
		}
		if (HasComponents<IPrefabPreProcess>(go.transform))
		{
			return true;
		}
		if (HasComponents<IPrefabPostProcess>(go.transform))
		{
			return true;
		}
		if (HasComponents<IEditorComponent>(go.transform))
		{
			return true;
		}
		if (!isClientside)
		{
			if (clientsideOnlyTypes.Any((Type type) => HasComponents(go.transform, type)))
			{
				return true;
			}
			if (HasComponents<IClientComponentEx>(go.transform))
			{
				return true;
			}
		}
		if (!isServerside)
		{
			if (serversideOnlyTypes.Any((Type type) => HasComponents(go.transform, type)))
			{
				return true;
			}
			if (HasComponents<IServerComponentEx>(go.transform))
			{
				return true;
			}
		}
		return false;
	}

	public void ProcessObject(string name, GameObject go, bool resetLocalTransform = true)
	{
		if (!isClientside)
		{
			Type[] array = clientsideOnlyTypes;
			foreach (Type t in array)
			{
				DestroyComponents(t, go, isClientside, isServerside);
			}
			foreach (IClientComponentEx item in FindComponents<IClientComponentEx>(go.transform))
			{
				item.PreClientComponentCull(this);
			}
		}
		if (!isServerside)
		{
			Type[] array = serversideOnlyTypes;
			foreach (Type t2 in array)
			{
				DestroyComponents(t2, go, isClientside, isServerside);
			}
			foreach (IServerComponentEx item2 in FindComponents<IServerComponentEx>(go.transform))
			{
				item2.PreServerComponentCull(this);
			}
		}
		DestroyComponents(typeof(IEditorComponent), go, isClientside, isServerside);
		if (resetLocalTransform)
		{
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
		}
		List<Transform> list = FindComponents<Transform>(go.transform);
		list.Reverse();
		MeshColliderCookingOptions meshColliderCookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
		MeshColliderCookingOptions cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;
		MeshColliderCookingOptions meshColliderCookingOptions2 = (MeshColliderCookingOptions)(-1);
		foreach (MeshCollider item3 in FindComponents<MeshCollider>(go.transform))
		{
			if (item3.cookingOptions == meshColliderCookingOptions || item3.cookingOptions == meshColliderCookingOptions2)
			{
				item3.cookingOptions = cookingOptions;
			}
		}
		foreach (IPrefabPreProcess item4 in FindComponents<IPrefabPreProcess>(go.transform))
		{
			item4.PreProcess(this, go, name, isServerside, isClientside, isBundling);
		}
		foreach (Transform item5 in list)
		{
			if (!item5 || !item5.gameObject)
			{
				continue;
			}
			if (isServerside && item5.gameObject.CompareTag("Server Cull"))
			{
				RemoveComponents(item5.gameObject);
				NominateForDeletion(item5.gameObject);
			}
			if (isClientside)
			{
				bool num = item5.gameObject.CompareTag("Client Cull");
				bool flag = item5 != go.transform && item5.gameObject.GetComponent<BaseEntity>() != null;
				if (num || flag)
				{
					RemoveComponents(item5.gameObject);
					NominateForDeletion(item5.gameObject);
				}
			}
		}
		RunCleanupQueue();
		foreach (IPrefabPostProcess item6 in FindComponents<IPrefabPostProcess>(go.transform))
		{
			item6.PostProcess(this, go, name, isServerside, isClientside, isBundling);
		}
	}

	public void Process(string name, GameObject go)
	{
		if (UnityEngine.Application.isPlaying && !go.CompareTag("NoPreProcessing"))
		{
			GameObject hierarchyGroup = GetHierarchyGroup();
			GameObject gameObject = go;
			go = Instantiate.GameObject(gameObject, hierarchyGroup.transform);
			go.name = gameObject.name;
			if (NeedsProcessing(go))
			{
				ProcessObject(name, go);
			}
			AddPrefab(name, go);
		}
	}

	public void Invalidate(string name)
	{
		GameObject value;
		if (prefabList.TryGetValue(name, out value))
		{
			prefabList.Remove(name);
			if (value != null)
			{
				UnityEngine.Object.DestroyImmediate(value, true);
			}
		}
	}

	public GameObject GetHierarchyGroup()
	{
		if (isClientside && isServerside)
		{
			return HierarchyUtil.GetRoot("PrefabPreProcess - Generic", false, true);
		}
		if (isServerside)
		{
			return HierarchyUtil.GetRoot("PrefabPreProcess - Server", false, true);
		}
		return HierarchyUtil.GetRoot("PrefabPreProcess - Client", false, true);
	}

	public void AddPrefab(string name, GameObject go)
	{
		go.SetActive(false);
		prefabList.Add(name, go);
	}

	private void DestroyComponents(Type t, GameObject go, bool client, bool server)
	{
		List<Component> list = new List<Component>();
		FindComponents(go.transform, list, t);
		list.Reverse();
		foreach (Component item in list)
		{
			RealmedRemove component = item.GetComponent<RealmedRemove>();
			if (!(component != null) || component.ShouldDelete(item, client, server))
			{
				if (!item.gameObject.CompareTag("persist"))
				{
					NominateForDeletion(item.gameObject);
				}
				UnityEngine.Object.DestroyImmediate(item, true);
			}
		}
	}

	private bool ShouldExclude(Transform transform)
	{
		if (transform.GetComponent<BaseEntity>() != null)
		{
			return true;
		}
		return false;
	}

	private bool HasComponents<T>(Transform transform)
	{
		if (transform.GetComponent<T>() != null)
		{
			return true;
		}
		foreach (Transform item in transform)
		{
			if (!ShouldExclude(item) && HasComponents<T>(item))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasComponents(Transform transform, Type t)
	{
		if (transform.GetComponent(t) != null)
		{
			return true;
		}
		foreach (Transform item in transform)
		{
			if (!ShouldExclude(item) && HasComponents(item, t))
			{
				return true;
			}
		}
		return false;
	}

	public List<T> FindComponents<T>(Transform transform)
	{
		List<T> list = new List<T>();
		FindComponents(transform, list);
		return list;
	}

	public void FindComponents<T>(Transform transform, List<T> list)
	{
		list.AddRange(transform.GetComponents<T>());
		foreach (Transform item in transform)
		{
			if (!ShouldExclude(item))
			{
				FindComponents(item, list);
			}
		}
	}

	public List<Component> FindComponents(Transform transform, Type t)
	{
		List<Component> list = new List<Component>();
		FindComponents(transform, list, t);
		return list;
	}

	public void FindComponents(Transform transform, List<Component> list, Type t)
	{
		list.AddRange(transform.GetComponents(t));
		foreach (Transform item in transform)
		{
			if (!ShouldExclude(item))
			{
				FindComponents(item, list, t);
			}
		}
	}

	public void RemoveComponent(Component c)
	{
		if (!(c == null))
		{
			destroyList.Add(c);
		}
	}

	public void RemoveComponents(GameObject gameObj)
	{
		Component[] components = gameObj.GetComponents<Component>();
		foreach (Component component in components)
		{
			if (!(component is Transform))
			{
				destroyList.Add(component);
			}
		}
	}

	public void NominateForDeletion(GameObject gameObj)
	{
		cleanupList.Add(gameObj);
	}

	public void RunCleanupQueue()
	{
		foreach (Component destroy in destroyList)
		{
			UnityEngine.Object.DestroyImmediate(destroy, true);
		}
		destroyList.Clear();
		foreach (GameObject cleanup in cleanupList)
		{
			DoCleanup(cleanup);
		}
		cleanupList.Clear();
	}

	public void DoCleanup(GameObject go)
	{
		if (!(go == null) && go.GetComponentsInChildren<Component>(true).Length <= 1)
		{
			Transform parent = go.transform.parent;
			if (!(parent == null) && !parent.name.StartsWith("PrefabPreProcess - "))
			{
				UnityEngine.Object.DestroyImmediate(go, true);
			}
		}
	}
}
