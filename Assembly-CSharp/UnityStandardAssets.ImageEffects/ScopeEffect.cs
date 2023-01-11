using UnityEngine;

namespace UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Other/Scope Overlay")]
public class ScopeEffect : PostEffectsBase, IImageEffect
{
	public Material overlayMaterial;

	public override bool CheckResources()
	{
		return true;
	}

	public bool IsActive()
	{
		if (base.enabled)
		{
			return CheckResources();
		}
		return false;
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		overlayMaterial.SetVector("_Screen", new Vector2(Screen.width, Screen.height));
		Graphics.Blit(source, destination, overlayMaterial);
	}
}
