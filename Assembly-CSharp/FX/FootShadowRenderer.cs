using System.Collections.ObjectModel;
using PhysicsUtils;
using UnityEngine;

namespace FX
{
	public class FootShadowRenderer
	{
		private class Assets
		{
			internal static readonly SpriteRenderer footShadow;

			static Assets()
			{
				footShadow = Resource.instance.footShadow;
			}
		}

		private const float _maxDistance = 5f;

		private LineSequenceNonAllocCaster _lineSequenceCaster;

		private Vector2 _size;

		public SpriteRenderer spriteRenderer { get; private set; }

		public FootShadowRenderer(int accuracy, Transform transform)
		{
			_lineSequenceCaster = new LineSequenceNonAllocCaster(1, accuracy * 2 + 1)
			{
				caster = new RayCaster
				{
					direction = Vector2.down,
					distance = 5f
				}
			};
			_lineSequenceCaster.caster.contactFilter.SetLayerMask(Layers.groundMask);
			spriteRenderer = Object.Instantiate(Assets.footShadow, transform);
			_size = spriteRenderer.size;
		}

		public void SetBounds(Bounds bounds)
		{
			_lineSequenceCaster.start = bounds.min;
			_lineSequenceCaster.end.x = bounds.max.x;
			_lineSequenceCaster.end.y = bounds.min.y;
			_size.x = bounds.size.x;
			spriteRenderer.size = _size;
		}

		public void Update()
		{
			_lineSequenceCaster.Cast();
			ReadOnlyCollection<NonAllocCaster> nonAllocCasters = _lineSequenceCaster.nonAllocCasters;
			int num = -1;
			for (int i = 0; i < nonAllocCasters.Count; i++)
			{
				if (nonAllocCasters[i].results.Count != 0 && (num == -1 || nonAllocCasters[num].results[0].distance > nonAllocCasters[i].results[0].distance))
				{
					num = i;
				}
			}
			if (num == -1)
			{
				spriteRenderer.gameObject.SetActive(false);
				return;
			}
			RaycastHit2D raycastHit2D = nonAllocCasters[num].results[0];
			float distance = raycastHit2D.distance;
			Vector3 position = spriteRenderer.transform.position;
			position.y = raycastHit2D.point.y;
			float num2 = (5f - distance) / 5f;
			spriteRenderer.gameObject.SetActive(true);
			spriteRenderer.transform.position = position;
			spriteRenderer.transform.localScale = Vector3.one * num2;
		}

		public void DrawDebugLine()
		{
			_lineSequenceCaster.DrawDebugLine();
		}
	}
}
