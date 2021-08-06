using System;
using UnityEngine;
using UnityEngine.UI;

public class VehicleEditingPanel : LootPanel
{
	[Serializable]
	private class CreateChassisEntry
	{
		public byte garageChassisIndex;

		public Button craftButton;

		public Text craftButtonText;

		public Text requirementsText;

		public ItemDefinition GetChassisItemDef(ModularCarGarage garage)
		{
			return garage.chassisBuildOptions[garageChassisIndex].itemDef;
		}
	}

	[SerializeField]
	[Range(0f, 1f)]
	private float disabledAlpha = 0.25f;

	[Header("Edit Vehicle")]
	[SerializeField]
	private CanvasGroup editGroup;

	[SerializeField]
	private GameObject moduleInternalItemsGroup;

	[SerializeField]
	private GameObject moduleInternalLiquidsGroup;

	[SerializeField]
	private GameObject destroyChassisGroup;

	[SerializeField]
	private Button itemTakeButton;

	[SerializeField]
	private Button liquidTakeButton;

	[SerializeField]
	private GameObject liquidHelp;

	[SerializeField]
	private GameObject liquidButton;

	[SerializeField]
	private Color gotColor;

	[SerializeField]
	private Color notGotColor;

	[SerializeField]
	private Text generalInfoText;

	[SerializeField]
	private Text generalWarningText;

	[SerializeField]
	private Image generalWarningImage;

	[SerializeField]
	private Text repairInfoText;

	[SerializeField]
	private Button repairButton;

	[SerializeField]
	private Text destroyChassisButtonText;

	[SerializeField]
	private Text destroyChassisCountdown;

	[SerializeField]
	private Translate.Phrase phraseEditingInfo;

	[SerializeField]
	private Translate.Phrase phraseNoOccupant;

	[SerializeField]
	private Translate.Phrase phraseBadOccupant;

	[SerializeField]
	private Translate.Phrase phraseNotDriveable;

	[SerializeField]
	private Translate.Phrase phraseNotRepairable;

	[SerializeField]
	private Translate.Phrase phraseRepairNotNeeded;

	[SerializeField]
	private Translate.Phrase phraseRepairSelectInfo;

	[SerializeField]
	private Translate.Phrase phraseRepairEnactInfo;

	[SerializeField]
	private Translate.Phrase phraseHasLock;

	[SerializeField]
	private Translate.Phrase phraseHasNoLock;

	[SerializeField]
	private Translate.Phrase phraseAddLock;

	[SerializeField]
	private Translate.Phrase phraseAddKey;

	[SerializeField]
	private Translate.Phrase phraseAddLockButton;

	[SerializeField]
	private Translate.Phrase phraseCraftKeyButton;

	[SerializeField]
	private Text carLockInfoText;

	[SerializeField]
	private Text carLockButtonText;

	[SerializeField]
	private Button actionLockButton;

	[SerializeField]
	private Button removeLockButton;

	[SerializeField]
	private Translate.Phrase phraseEmptyStorage;

	[Header("Create Chassis")]
	[SerializeField]
	private CreateChassisEntry[] chassisOptions;
}
