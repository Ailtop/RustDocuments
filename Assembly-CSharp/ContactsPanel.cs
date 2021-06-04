using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class ContactsPanel : SingletonComponent<ContactsPanel>
{
	public RectTransform alliesBucket;

	public RectTransform seenBucket;

	public RectTransform enemiesBucket;

	public RectTransform contentsBucket;

	public ContactsEntry contactEntryPrefab;

	public RawImage mugshotTest;

	public RawImage fullBodyTest;

	public RustButton[] filterButtons;

	public RelationshipManager.RelationshipType selectedRelationshipType = RelationshipManager.RelationshipType.Friend;
}
