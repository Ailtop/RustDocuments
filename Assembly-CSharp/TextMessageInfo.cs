using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TextMessageInfo : ScriptableObject, IEnumerable<TextMessageInfo.Message>, IEnumerable
{
	[Serializable]
	public class Message
	{
		public int startIndex;

		public int endIndex;
	}

	[SerializeField]
	private string _nameKey;

	[SerializeField]
	private string _messageKey;

	[SerializeField]
	private Message[] _messageKeys;

	public Message[] messages => _messageKeys;

	public string nameKey => _nameKey;

	public string messageKey => _messageKey;

	public IEnumerator<Message> GetEnumerator()
	{
		return (IEnumerator<Message>)messages.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return messages.GetEnumerator();
	}
}
