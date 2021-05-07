using UnityEngine;
using UnityEngine.UI;

namespace UI.Pause
{
	public class GraphicColorDiffuser : Graphic
	{
		[SerializeField]
		private Graphic[] _graphics;

		public override Color color
		{
			get
			{
				return base.color;
			}
			set
			{
				base.color = value;
				Graphic[] graphics = _graphics;
				for (int i = 0; i < graphics.Length; i++)
				{
					graphics[i].color = value;
				}
			}
		}

		public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
		{
			base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
			Graphic[] graphics = _graphics;
			for (int i = 0; i < graphics.Length; i++)
			{
				graphics[i].CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
			}
		}
	}
}
