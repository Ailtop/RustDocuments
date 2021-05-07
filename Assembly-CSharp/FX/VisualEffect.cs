using Characters;
using UnityEngine;

namespace FX
{
	public abstract class VisualEffect : MonoBehaviour
	{
		public static void PostProcess(PoolObject poolObject, Character target, float scale, float angle, Vector3 offset, EffectInfo.AttachInfo attachInfo, bool relativeScaleToTargetSize, bool overwrite = false)
		{
		}

		public static void PostProcess(PoolObject poolObject, Character target, float scale, float angle, Vector3 offset, bool attachToTarget, bool relativeScaleToTargetSize, bool overwrite = false)
		{
			float num = 1f;
			if (relativeScaleToTargetSize)
			{
				Vector3 size = target.collider.bounds.size;
				num = Mathf.Min(size.x, size.y);
			}
			if (attachToTarget)
			{
				poolObject.transform.parent = target.transform;
			}
			PostProcess(poolObject, scale * num, angle, offset, overwrite);
		}

		public static void PostProcess(PoolObject poolObject, float scale, float angle, Vector3 offset, bool overwrite = false)
		{
			if (overwrite)
			{
				poolObject.transform.localScale = Vector3.one * scale;
				poolObject.transform.eulerAngles = new Vector3(0f, 0f, angle);
			}
			else
			{
				poolObject.transform.localScale *= scale;
				poolObject.transform.eulerAngles += new Vector3(0f, 0f, angle);
			}
			poolObject.transform.localPosition += offset;
		}
	}
}
