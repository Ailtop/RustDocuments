using UnityEngine;

namespace Characters
{
	public class AttackDamage : MonoBehaviour, IAttackDamage
	{
		[SerializeField]
		private int _minAttackDamage;

		[SerializeField]
		private int _maxAttackDamage;

		public int minAttackDamage
		{
			get
			{
				return _minAttackDamage;
			}
			set
			{
				_minAttackDamage = value;
			}
		}

		public int maxAttackDamage
		{
			get
			{
				return _maxAttackDamage;
			}
			set
			{
				_maxAttackDamage = value;
			}
		}

		public float amount => Random.Range(minAttackDamage, _maxAttackDamage + 1);
	}
}
