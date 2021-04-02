using Facepunch;
using ProtoBuf;

public class CardTablePlayerStorage : StorageContainer
{
	private EntityRef cardTableRef;

	public CardTable GetCardTable()
	{
		BaseEntity baseEntity = cardTableRef.Get(base.isServer);
		if (baseEntity != null && BaseEntityEx.IsValid(baseEntity))
		{
			return baseEntity as CardTable;
		}
		return null;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.simpleUID != null)
		{
			cardTableRef.uid = info.msg.simpleUID.uid;
		}
	}

	public void SetCardTable(CardTable cardTable)
	{
		cardTableRef.Set(cardTable);
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		CardTable cardTable = GetCardTable();
		if (cardTable != null)
		{
			cardTable.PlayerStorageChanged();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.simpleUID = Pool.Get<SimpleUID>();
		info.msg.simpleUID.uid = cardTableRef.uid;
	}
}
