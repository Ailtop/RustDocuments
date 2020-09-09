using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Skeleton Properties")]
public class SkeletonProperties : ScriptableObject
{
	[Serializable]
	public class BoneProperty
	{
		public GameObject bone;

		public Translate.Phrase name;

		public HitArea area;
	}

	public GameObject boneReference;

	[BoneProperty]
	public BoneProperty[] bones;

	[NonSerialized]
	private Dictionary<uint, BoneProperty> quickLookup;

	public void OnValidate()
	{
		if (boneReference == null)
		{
			Debug.LogWarning("boneReference is null", this);
			return;
		}
		List<BoneProperty> list = bones.ToList();
		foreach (Transform child in TransformEx.GetAllChildren(boneReference.transform))
		{
			if (list.All((BoneProperty x) => x.bone != child.gameObject))
			{
				list.Add(new BoneProperty
				{
					bone = child.gameObject,
					name = new Translate.Phrase
					{
						token = child.name.ToLower(),
						english = child.name.ToLower()
					}
				});
			}
		}
		bones = list.ToArray();
	}

	private void BuildDictionary()
	{
		quickLookup = new Dictionary<uint, BoneProperty>();
		BoneProperty[] array = bones;
		foreach (BoneProperty boneProperty in array)
		{
			uint num = StringPool.Get(boneProperty.bone.name);
			if (!quickLookup.ContainsKey(num))
			{
				quickLookup.Add(num, boneProperty);
				continue;
			}
			string name = boneProperty.bone.name;
			string name2 = quickLookup[num].bone.name;
			Debug.LogWarning("Duplicate bone id " + num + " for " + name + " and " + name2);
		}
	}

	public BoneProperty FindBone(uint id)
	{
		if (quickLookup == null)
		{
			BuildDictionary();
		}
		BoneProperty value = null;
		if (!quickLookup.TryGetValue(id, out value))
		{
			return null;
		}
		return value;
	}
}
