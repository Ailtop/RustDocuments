using UnityEngine;

namespace UI
{
	public class DefaultMaterialShaderSetter : MonoBehaviour
	{
		[SerializeField]
		private Shader _shader;

		private void Awake()
		{
			Canvas.GetDefaultCanvasMaterial().shader = _shader;
		}
	}
}
