using PhysicsUtils;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class ShiftToPlayer : CharacterOperation
	{
		[SerializeField]
		private Transform _object;

		[SerializeField]
		private float _offsetY;

		[SerializeField]
		private float _offsetX;

		[SerializeField]
		private bool _fromPlatform;

		[SerializeField]
		private bool _lastStandingPlatform;

		private static NonAllocCaster caster;

		static ShiftToPlayer()
		{
			caster = new NonAllocCaster(1);
		}

		public override void Run(Character owner)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			Collider2D platform = GetPlatform();
			float x = player.transform.position.x + _offsetX;
			float y = platform.bounds.max.y + _offsetY;
			_object.position = new Vector2(x, y);
		}

		private Collider2D GetPlatform()
		{
			if (_lastStandingPlatform)
			{
				return null;
			}
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
