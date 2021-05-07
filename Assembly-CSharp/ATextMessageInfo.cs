using UnityEngine;

[CreateAssetMenu]
public class ATextMessageInfo : ScriptableObject
{
	[SerializeField]
	private string _nameKey;

	[SerializeField]
	private string _messagesKey;

	public string nameKey => _nameKey;

	public string messagesKey => _messagesKey;
}
