using System;
using UnityEngine;

namespace BT
{
	public class BehaviourTree : MonoBehaviour
	{
		[Serializable]
		public class Subcomponents : SubcomponentArray<BehaviourTree>
		{
		}

		[SerializeField]
		private string _tag;

		[SerializeField]
		[Node.Subcomponent(true)]
		private Node _node;

		public Node node => _node;

		public NodeState Tick(Context context)
		{
			return _node.Tick(context);
		}

		public void ResetState()
		{
			_node.ResetState();
		}

		public override string ToString()
		{
			if (_tag != null && _tag.Length != 0)
			{
				return $"({node.GetType()}) {_tag}";
			}
			return base.ToString();
		}
	}
}
