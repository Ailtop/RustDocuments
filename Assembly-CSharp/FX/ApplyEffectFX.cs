using UnityEngine;

namespace FX
{
	public class ApplyEffectFX : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer _renderer;

		[SerializeField]
		private int _hue;

		private MaterialPropertyBlock _propertyBlock;

		private void Awake()
		{
			_propertyBlock = new MaterialPropertyBlock();
			_renderer.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetInt(EffectInfo.huePropertyID, _hue);
			_renderer.SetPropertyBlock(_propertyBlock);
		}
	}
}
