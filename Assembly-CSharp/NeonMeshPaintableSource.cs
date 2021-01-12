using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class NeonMeshPaintableSource : MeshPaintableSource
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass8_0
	{
		public int width;

		public Color32[] pixels;

		public NeonMeshPaintableSource _003C_003E4__this;
	}

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

	[CompilerGenerated]
	private Color _003CUpdateFrom_003Eg__GetColorForRegion_007C8_0(int x, int y, int regionWidth, int regionHeight, ref _003C_003Ec__DisplayClass8_0 P_4)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = y + regionHeight;
		for (int i = y; i < num4; i++)
		{
			int num5 = i * P_4.width + x;
			int num6 = num5 + regionWidth;
			for (int j = num5; j < num6; j++)
			{
				Color32 color = P_4.pixels[j];
				float num7 = (float)(int)color.a / 255f;
				num += (float)(int)color.r * num7;
				num2 += (float)(int)color.g * num7;
				num3 += (float)(int)color.b * num7;
			}
		}
		int num8 = regionWidth * regionHeight * 255;
		return new Color(lightingCurve.Evaluate(num / (float)num8), lightingCurve.Evaluate(num2 / (float)num8), lightingCurve.Evaluate(num3 / (float)num8), 1f);
	}
}
