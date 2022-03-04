using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class ContactsPanel : SingletonComponent<ContactsPanel>
{
	public enum SortMode
	{
		None = 0,
		RecentlySeen = 1
	}

	public RectTransform alliesBucket;

	public RectTransform seenBucket;

	public RectTransform enemiesBucket;

	public RectTransform contentsBucket;

	public ContactsEntry contactEntryPrefab;

	public RawImage mugshotTest;

	public RawImage fullBodyTest;

	public RustButton[] filterButtons;

	public RelationshipManager.RelationshipType selectedRelationshipType = RelationshipManager.RelationshipType.Friend;

	public RustButton lastSeenToggle;

	public Translate.Phrase sortingByLastSeenPhrase;

	public Translate.Phrase sortingByFirstSeen;

	public RustText sortText;
}
