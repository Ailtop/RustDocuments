using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class TextureParameter : ParameterOverride<Texture>
	{
		public TextureParameterDefault defaultState = TextureParameterDefault.Black;

		public override void Interp(Texture from, Texture to, float t)
		{
			if (from == null && to == null)
			{
				value = null;
				return;
			}
			if (from != null && to != null)
			{
				value = TextureLerper.instance.Lerp(from, to, t);
				return;
			}
			if (defaultState == TextureParameterDefault.Lut2D)
			{
				Texture lutStrip = RuntimeUtilities.GetLutStrip((from != null) ? from.height : to.height);
				if (from == null)
				{
					from = lutStrip;
				}
				if (to == null)
				{
					to = lutStrip;
				}
			}
			Color to2;
			switch (defaultState)
			{
			case TextureParameterDefault.Black:
				to2 = Color.black;
				break;
			case TextureParameterDefault.White:
				to2 = Color.white;
				break;
			case TextureParameterDefault.Transparent:
				to2 = Color.clear;
				break;
			case TextureParameterDefault.Lut2D:
			{
				Texture lutStrip2 = RuntimeUtilities.GetLutStrip((from != null) ? from.height : to.height);
				if (from == null)
				{
					from = lutStrip2;
				}
				if (to == null)
				{
					to = lutStrip2;
				}
				if (from.width != to.width || from.height != to.height)
				{
					value = null;
				}
				else
				{
					value = TextureLerper.instance.Lerp(from, to, t);
				}
				return;
			}
			default:
				base.Interp(from, to, t);
				return;
			}
			if (from == null)
			{
				value = TextureLerper.instance.Lerp(to, to2, 1f - t);
			}
			else
			{
				value = TextureLerper.instance.Lerp(from, to2, t);
			}
		}
	}
}
