using UnityEngine;

namespace FX
{
	public static class Materials
	{
		public static Material color { get; private set; }

		public static Material colorOverlay { get; private set; }

		public static Material character { get; private set; }

		public static Material effect { get; private set; }

		public static Material effect_darken { get; private set; }

		public static Material effect_lighten { get; private set; }

		public static Material effect_linearBurn { get; private set; }

		public static Material effect_linearDodge { get; private set; }

		public static Material minimap { get; private set; }

		public static Material ui_grayScale { get; private set; }

		static Materials()
		{
			Resource instance = Resource.instance;
			color = instance.materialDictionary["color"];
			colorOverlay = instance.materialDictionary["colorOverlay"];
			character = instance.materialDictionary["character"];
			effect = instance.materialDictionary["effect"];
			effect_darken = instance.materialDictionary["effect_darken"];
			effect_lighten = instance.materialDictionary["effect_lighten"];
			effect_linearBurn = instance.materialDictionary["effect_linearBurn"];
			effect_linearDodge = instance.materialDictionary["effect_linearDodge"];
			minimap = instance.materialDictionary["minimap"];
			ui_grayScale = instance.materialDictionary["ui_grayScale"];
		}
	}
}
