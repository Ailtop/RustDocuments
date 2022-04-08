public class NoticeArea : SingletonComponent<NoticeArea>
{
	public GameObjectRef itemPickupPrefab;

	public GameObjectRef itemPickupCondensedText;

	public GameObjectRef itemDroppedPrefab;

	private IVitalNotice[] notices;

	protected override void Awake()
	{
		base.Awake();
		notices = GetComponentsInChildren<IVitalNotice>(includeInactive: true);
	}
}
