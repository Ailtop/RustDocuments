using System;
using UnityEngine;

public class NeonMeshPaintableSource : MeshPaintableSource
{
	public NeonSign neonSign;

	public float editorEmissionScale = 2f;

	public AnimationCurve lightingCurve;

	[NonSerialized]
	public Color topLeft;

	[NonSerialized]
	public Color topRight;

	[NonSerialized]
	public Color bottomLeft;

	[NonSerialized]
	public Color bottomRight;

	public override void UpdateMaterials(MaterialPropertyBlock block, Texture2D textureOverride = null, bool forEditing = false, bool isSelected = false)
	{
		base.UpdateMaterials(block, textureOverride, forEditing);
		if (forEditing)
		{
			block.SetFloat("_EmissionScale", editorEmissionScale);
			block.SetFloat("_Power", isSelected ? 1 : 0);
			if (!isSelected)
			{
				block.SetColor("_TubeInner", Color.clear);
				block.SetColor("_TubeOuter", Color.clear);
			}
		}
		else if (neonSign != null)
		{
			block.SetFloat("_Power", (isSelected && neonSign.HasFlag(BaseEntity.Flags.Reserved8)) ? 1 : 0);
		}
	}

	public override Color32[] UpdateFrom(Texture2D input)
	{
		_003C_003Ec__DisplayClass8_0 _003C_003Ec__DisplayClass8_ = default(_003C_003Ec__DisplayClass8_0);
		_003C_003Ec__DisplayClass8_._003C_003E4__this = this;
		Init();
		_003C_003Ec__DisplayClass8_.pixels = input.GetPixels32();
		texture.SetPixels32(_003C_003Ec__DisplayClass8_.pixels);
		texture.Apply(true, false);
		_003C_003Ec__DisplayClass8_.width = input.width;
		int height = input.height;
		int num = _003C_003Ec__DisplayClass8_.width / 2;
		int num2 = height / 2;
		topLeft = _003CUpdateFrom_003Eg__GetColorForRegion_007C8_0(0, num2, num, num2, ref _003C_003Ec__DisplayClass8_);
		topRight = _003CUpdateFrom_003Eg__GetColorForRegion_007C8_0(num, num2, num, num2, ref _003C_003Ec__DisplayClass8_);
		bottomLeft = _003CUpdateFrom_003Eg__GetColorForRegion_007C8_0(0, 0, num, num2, ref _003C_003Ec__DisplayClass8_);
		bottomRight = _003CUpdateFrom_003Eg__GetColorForRegion_007C8_0(num, 0, num, num2, ref _003C_003Ec__DisplayClass8_);
		return _003C_003Ec__DisplayClass8_.pixels;
	}
}
