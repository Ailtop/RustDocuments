using System;
using UnityEngine;

namespace Level.Chapter4
{
	[Serializable]
	public class Node : INode
	{
		[SerializeField]
		private string _displayText = "display text";

		public string DisplayText
		{
			get
			{
				return _displayText;
			}
			set
			{
				_displayText = value;
			}
		}
	}
}
