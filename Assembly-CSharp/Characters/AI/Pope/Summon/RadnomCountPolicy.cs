using UnityEngine;

namespace Characters.AI.Pope.Summon
{
	public sealed class RadnomCountPolicy : CountPolicy
	{
		[SerializeField]
		[MinMaxSlider(0f, 100f)]
		private Vector2Int _range;

		public override int GetCount()
		{
			return Random.Range(_range.x, _range.y + 1);
		}
	}
}
