using UnityEngine;

namespace FX
{
	public class MaterialProperty : MonoBehaviour
	{
		private static readonly int _huePropertyID = Shader.PropertyToID("_Hue");

		[Range(-180f, 180f)]
		public int hue;

		[SerializeField]
		[GetComponent]
		private SpriteRenderer _renderer;

		private MaterialPropertyBlock _propertyBlock;

		private void Start()
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			_renderer.sharedMaterial = Materials.effect;
			_renderer.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetInt(_huePropertyID, hue);
			_renderer.SetPropertyBlock(materialPropertyBlock);
		}
	}
}
