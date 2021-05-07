using System;
using UnityEngine;

namespace SkulStories
{
	[CreateAssetMenu]
	public class TextLinkInfos : ScriptableObject
	{
		[Serializable]
		public class TextLink
		{
			public enum Position
			{
				Normal,
				Below
			}

			public Position position;

			public string text;
		}

		[SerializeField]
		private TextLink[] _texts;

		public TextLink[] texts => _texts;
	}
}
