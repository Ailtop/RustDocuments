using System;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Weight : MonoBehaviour
	{
		[Serializable]
		public class Subcomponents : SubcomponentArray<Weight>
		{
		}

		[SerializeField]
		[Behaviour.Subcomponent(true)]
		private Behaviour _key;

		[SerializeField]
		private int _value = 1;

		[SerializeField]
		private string _tag;

		public Behaviour key => _key;

		public int value => _value;

		public override string ToString()
		{
			string arg = ((_tag == null || _tag.Length == 0) ? base.ToString() : _tag);
			return $"{arg}, weight : {_value}";
		}
	}
}
