using Facepunch;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Hair Set")]
public class HairSet : ScriptableObject
{
	[Serializable]
	public class MeshReplace
	{
		[HideInInspector]
		public string FindName;

		public Mesh Find;

		public Mesh[] ReplaceShapes;

		public bool Test(string materialName)
		{
			return FindName == materialName;
		}
	}

	public MeshReplace[] MeshReplacements;

	public void Process(PlayerModelHair playerModelHair, HairDyeCollection dyeCollection, HairDye dye, MaterialPropertyBlock block)
	{
		List<SkinnedMeshRenderer> obj = Pool.GetList<SkinnedMeshRenderer>();
		playerModelHair.gameObject.GetComponentsInChildren(true, obj);
		foreach (SkinnedMeshRenderer item in obj)
		{
			if (!(item.sharedMesh == null) && !(item.sharedMaterial == null))
			{
				string name = item.sharedMesh.name;
				string name2 = item.sharedMaterial.name;
				if (!item.gameObject.activeSelf)
				{
					item.gameObject.SetActive(true);
				}
				for (int i = 0; i < MeshReplacements.Length; i++)
				{
					MeshReplacements[i].Test(name);
				}
				if (dye != null && item.gameObject.activeSelf)
				{
					dye.Apply(dyeCollection, block);
				}
			}
		}
		Pool.FreeList(ref obj);
	}

	public void ProcessMorphs(GameObject obj, int blendShapeIndex = -1)
	{
	}
}
