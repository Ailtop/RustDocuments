using Facepunch;
using Rust.Workshop;
using System.Linq;
using UnityEngine;

public class ViewmodelClothing : MonoBehaviour
{
	public SkeletonSkin[] SkeletonSkins;

	internal void CopyToSkeleton(Skeleton skeleton, GameObject parent, Item item)
	{
		SkeletonSkin[] skeletonSkins = SkeletonSkins;
		int num = 0;
		GameObject gameObject;
		while (true)
		{
			if (num >= skeletonSkins.Length)
			{
				return;
			}
			SkeletonSkin obj = skeletonSkins[num];
			gameObject = new GameObject();
			gameObject.transform.parent = parent.transform;
			obj.DuplicateAndRetarget(gameObject, skeleton).updateWhenOffscreen = true;
			if (item != null && item.skin != 0)
			{
				ItemSkinDirectory.Skin skin = item.info.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == item.skin);
				if (skin.id == 0 && item.skin != 0)
				{
					break;
				}
				if ((ulong)skin.id != item.skin)
				{
					return;
				}
				ItemSkin itemSkin = skin.invItem as ItemSkin;
				if (itemSkin == null)
				{
					return;
				}
				itemSkin.ApplySkin(gameObject);
			}
			num++;
		}
		Rust.Workshop.WorkshopSkin.Apply(gameObject, item.skin);
	}
}
