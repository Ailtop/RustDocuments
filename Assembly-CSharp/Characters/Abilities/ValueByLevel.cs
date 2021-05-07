using System.Collections;
using UnityEngine;

namespace Characters.Abilities
{
	public class ValueByLevel : MonoBehaviour
	{
		[SerializeField]
		private float[] _values;

		public int level { private get; set; }

		public IList values => _values;

		public float GetValue()
		{
			return _values[level];
		}
	}
}
