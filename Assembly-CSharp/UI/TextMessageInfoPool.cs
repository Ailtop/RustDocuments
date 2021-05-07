using UnityEngine;

namespace UI
{
	[CreateAssetMenu]
	public class TextMessageInfoPool : TextMessageInfo
	{
		public Message GetRandomText()
		{
			return base.messages[Random.Range(0, base.messages.Length - 1)];
		}
	}
}
