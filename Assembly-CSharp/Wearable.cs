using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using UnityEngine;

public class Wearable : MonoBehaviour, IItemSetup, IPrefabPreProcess
{
	[Flags]
	public enum RemoveSkin
	{
		Torso = 1,
		Feet = 2,
		Hands = 4,
		Legs = 8,
		Head = 0x10
	}

	[Flags]
	public enum RemoveHair
	{
		Head = 1,
		Eyebrow = 2,
		Facial = 4,
		Armpit = 8,
		Pubic = 0x10
	}

	[Flags]
	public enum DeformHair
	{
		None = 0,
		BaseballCap = 1,
		BoonieHat = 2,
		CandleHat = 3,
		MinersHat = 4,
		WoodHelmet = 5
	}

	[Flags]
	public enum OccupationSlots
	{
		HeadTop = 1,
		Face = 2,
		HeadBack = 4,
		TorsoFront = 8,
		TorsoBack = 0x10,
		LeftShoulder = 0x20,
		RightShoulder = 0x40,
		LeftArm = 0x80,
		RightArm = 0x100,
		LeftHand = 0x200,
		RightHand = 0x400,
		Groin = 0x800,
		Bum = 0x1000,
		LeftKnee = 0x2000,
		RightKnee = 0x4000,
		LeftLeg = 0x8000,
		RightLeg = 0x10000,
		LeftFoot = 0x20000,
		RightFoot = 0x40000,
		Mouth = 0x80000,
		Eyes = 0x100000,
		Back = 0x200000
	}

	[Serializable]
	public struct PartRandomizer
	{
		public PartCollection[] groups;
	}

	[Serializable]
	public struct PartCollection
	{
		public GameObject[] parts;
	}

	[InspectorFlags]
	public RemoveSkin removeSkin;

	[InspectorFlags]
	public RemoveSkin removeSkinFirstPerson;

	[InspectorFlags]
	public RemoveHair removeHair;

	[InspectorFlags]
	public DeformHair deformHair;

	[InspectorFlags]
	public OccupationSlots occupationUnder;

	[InspectorFlags]
	public OccupationSlots occupationOver;

	public bool IsBackpack;

	public bool showCensorshipCube;

	public bool showCensorshipCubeBreasts;

	public bool forceHideCensorshipBreasts;

	public string followBone;

	public bool disableRigStripping;

	public bool overrideDownLimit;

	public float downLimit = 70f;

	[HideInInspector]
	public PlayerModelHair playerModelHair;

	[HideInInspector]
	public PlayerModelHairCap playerModelHairCap;

	[HideInInspector]
	public WearableReplacementByRace wearableReplacementByRace;

	[HideInInspector]
	public WearableShadowLod wearableShadowLod;

	[HideInInspector]
	public List<Renderer> renderers = new List<Renderer>();

	[HideInInspector]
	public List<PlayerModelSkin> playerModelSkins = new List<PlayerModelSkin>();

	[HideInInspector]
	public List<BoneRetarget> boneRetargets = new List<BoneRetarget>();

	[HideInInspector]
	public List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();

	[HideInInspector]
	public List<SkeletonSkin> skeletonSkins = new List<SkeletonSkin>();

	[HideInInspector]
	public List<ComponentInfo> componentInfos = new List<ComponentInfo>();

	[HideInInspector]
	public List<WearableNotify> notifies = new List<WearableNotify>();

	public bool HideInEyesView;

	[Tooltip("If this is true, we'll hide this item in the first person view. Usually done for items that you definitely won't see in first person view, like facemasks and hats.")]
	[Header("First Person Legs")]
	public bool HideInFirstPerson;

	[Tooltip("Use this if the clothing item clips into the player view. It'll push the chest legs model backwards.")]
	[Range(0f, 5f)]
	public float ExtraLeanBack;

	[Tooltip("Enable this to check for BoneRetargets which need to be preserved in first person view")]
	public bool PreserveBones;

	public Renderer[] RenderersLod0;

	public Renderer[] RenderersLod1;

	public Renderer[] RenderersLod2;

	public Renderer[] RenderersLod3;

	public Renderer[] RenderersLod4;

	public Renderer[] SkipInFirstPersonLegs;

	private static LOD[] emptyLOD = new LOD[1];

	public PartRandomizer[] randomParts;

	public void OnItemSetup(Item item)
	{
	}

	public virtual void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		LODGroup[] componentsInChildren = GetComponentsInChildren<LODGroup>(includeInactive: true);
		foreach (LODGroup lODGroup in componentsInChildren)
		{
			lODGroup.SetLODs(emptyLOD);
			preProcess.RemoveComponent(lODGroup);
		}
	}

	public void CacheComponents()
	{
		playerModelHairCap = GetComponent<PlayerModelHairCap>();
		playerModelHair = GetComponent<PlayerModelHair>();
		wearableReplacementByRace = GetComponent<WearableReplacementByRace>();
		wearableShadowLod = GetComponent<WearableShadowLod>();
		GetComponentsInChildren(includeInactive: true, renderers);
		GetComponentsInChildren(includeInactive: true, playerModelSkins);
		GetComponentsInChildren(includeInactive: true, boneRetargets);
		GetComponentsInChildren(includeInactive: true, skinnedRenderers);
		GetComponentsInChildren(includeInactive: true, skeletonSkins);
		GetComponentsInChildren(includeInactive: true, componentInfos);
		GetComponentsInChildren(includeInactive: true, notifies);
		RenderersLod0 = renderers.Where((Renderer x) => x.gameObject.name.EndsWith("0")).ToArray();
		RenderersLod1 = renderers.Where((Renderer x) => x.gameObject.name.EndsWith("1")).ToArray();
		RenderersLod2 = renderers.Where((Renderer x) => x.gameObject.name.EndsWith("2")).ToArray();
		RenderersLod3 = renderers.Where((Renderer x) => x.gameObject.name.EndsWith("3")).ToArray();
		RenderersLod4 = renderers.Where((Renderer x) => x.gameObject.name.EndsWith("4")).ToArray();
		foreach (Renderer renderer in renderers)
		{
			renderer.gameObject.AddComponent<ObjectMotionVectorFix>();
			renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
		}
	}

	public void StripRig(IPrefabProcessor preProcess, SkinnedMeshRenderer skinnedMeshRenderer)
	{
		if (disableRigStripping)
		{
			return;
		}
		Transform transform = SkinnedMeshRendererEx.FindRig(skinnedMeshRenderer);
		if (!(transform != null))
		{
			return;
		}
		List<Transform> obj = Pool.GetList<Transform>();
		transform.GetComponentsInChildren(obj);
		for (int num = obj.Count - 1; num >= 0; num--)
		{
			if (preProcess != null)
			{
				preProcess.NominateForDeletion(obj[num].gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(obj[num].gameObject);
			}
		}
		Pool.FreeList(ref obj);
	}

	public void SetupRendererCache(IPrefabProcessor preProcess)
	{
	}
}
