using UnityEngine;

public class InventoryUI : MonoBehaviour
{
	public GameObject ContactsButton;

	private void Update()
	{
		if (ContactsButton != null && ContactsButton.activeSelf && !RelationshipManager.contacts)
		{
			ContactsButton.SetActive(value: false);
		}
	}
}
