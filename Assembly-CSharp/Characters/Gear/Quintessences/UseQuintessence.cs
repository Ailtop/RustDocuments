using UnityEngine;

namespace Characters.Gear.Quintessences
{
	public abstract class UseQuintessence : MonoBehaviour
	{
		[SerializeField]
		protected Quintessence _quintessence;

		protected virtual void Awake()
		{
			_quintessence.onUse += OnUse;
		}

		protected abstract void OnUse();
	}
}
