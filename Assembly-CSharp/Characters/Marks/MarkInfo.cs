using FX;
using UnityEngine;

namespace Characters.Marks
{
	[CreateAssetMenu]
	public class MarkInfo : ScriptableObject
	{
		public delegate void OnStackDelegate(Mark mark, float stack);

		public OnStackDelegate onStack;

		[SerializeField]
		private int _maxStack;

		[SerializeField]
		protected Sprite[] _stackImages;

		[SerializeField]
		private EffectInfo.AttachInfo _attachInfo = new EffectInfo.AttachInfo(true, false, 9, EffectInfo.AttachInfo.Pivot.Top);

		public int maxStack => _maxStack;

		public Sprite[] stackImages => _stackImages;

		public EffectInfo.AttachInfo attachInfo => _attachInfo;
	}
}
