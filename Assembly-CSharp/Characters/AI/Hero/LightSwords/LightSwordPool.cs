using System.Collections.Generic;
using System.Linq;
using Level;
using UnityEngine;

namespace Characters.AI.Hero.LightSwords
{
	public class LightSwordPool : MonoBehaviour
	{
		[SerializeField]
		private Character _owner;

		[SerializeField]
		private LightSword _lightSwordPrefab;

		[SerializeField]
		private int _count;

		private List<LightSword> _swords;

		private void Awake()
		{
			Create(_count);
		}

		private void Create(int count)
		{
			_swords = new List<LightSword>(_count);
			for (int i = 0; i < count; i++)
			{
				LightSword lightSword = Object.Instantiate(_lightSwordPrefab, Vector2.zero, Quaternion.identity, Map.Instance.transform);
				lightSword.Initialzie(_owner);
				_swords.Add(lightSword);
			}
		}

		public List<LightSword> Get()
		{
			return _swords;
		}

		public List<LightSword> Take(int count)
		{
			if (_swords.Count < count)
			{
				return null;
			}
			return _swords.Take(count).ToList();
		}
	}
}
