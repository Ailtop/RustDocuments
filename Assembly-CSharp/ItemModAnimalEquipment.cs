public class ItemModAnimalEquipment : ItemMod
{
	public enum SlotType
	{
		Basic = 0,
		Armor = 1,
		Saddle = 2,
		Bit = 3,
		Feet = 4,
		SaddleDouble = 5
	}

	public BaseEntity.Flags WearableFlag;

	public bool hideHair;

	public ProtectionProperties animalProtection;

	public ProtectionProperties riderProtection;

	public int additionalInventorySlots;

	public float speedModifier;

	public float staminaUseModifier;

	public SlotType slot;
}
