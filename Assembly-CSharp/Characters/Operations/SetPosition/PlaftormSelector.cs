using PhysicsUtils;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public static class PlaftormSelector
	{
		private static NonAllocCaster caster;

		static PlaftormSelector()
		{
			caster = new NonAllocCaster(1);
		}

		private static Collider2D GetPlatform()
		{
			caster.contactFilter.SetLayerMask(Layers.groundMask);
			NonAllocCaster nonAllocCaster = caster.BoxCast(Singleton<Service>.Instance.levelManager.player.transform.position, Singleton<Service>.Instance.levelManager.player.collider.bounds.size, 0f, Vector2.down, 100f);
			if (nonAllocCaster.results.Count == 0)
			{
				return null;
			}
			return nonAllocCaster.results[0].collider;
		}
	}
}
