using Singletons;
using UnityEngine;

namespace FX
{
	public class ScreenEffectSpawner : Singleton<ScreenEffectSpawner>
	{
		[SerializeField]
		private CameraController _cameraController;

		private float _cachedZoom;

		private void Update()
		{
			float zoom = _cameraController.zoom;
			if (zoom != _cachedZoom)
			{
				_cachedZoom = zoom;
				base.transform.localScale = Vector3.one * zoom;
			}
		}

		public void Spawn(EffectInfo effect, Vector2 offset)
		{
			Vector3 position = base.transform.position;
			position.x += offset.x;
			position.y += offset.y;
			position.z = 0f;
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = effect.Spawn(position);
			reusableChronoSpriteEffect.transform.parent = base.transform;
			reusableChronoSpriteEffect.transform.localScale = Vector3.one;
		}
	}
}
