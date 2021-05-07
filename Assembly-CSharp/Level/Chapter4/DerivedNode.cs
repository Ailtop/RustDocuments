using System;
using UnityEngine;

namespace Level.Chapter4
{
	[Serializable]
	public class DerivedNode : Node
	{
		[SerializeField]
		private string _displayTextDerived = "derived";
	}
}
