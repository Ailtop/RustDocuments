using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class VineColorSetter : MonoBehaviour
	{
		[SerializeField]
		private Gradient _phase2Color;

		[SerializeField]
		private Gradient _recoveredColor;

		[SerializeField]
		private LineRenderer[] _vines;

		public void SetColorPhase2()
		{
			SetColor(_phase2Color);
		}

		public void SetColorRecovered()
		{
			SetColor(_recoveredColor);
		}

		private void SetColor(Gradient gradient)
		{
			LineRenderer[] vines = _vines;
			for (int i = 0; i < vines.Length; i++)
			{
				vines[i].colorGradient = gradient;
			}
		}
	}
}
