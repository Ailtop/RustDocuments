public class EntityFlag_ToggleNotify : EntityFlag_Toggle
{
	public bool UseEntityParent;

	protected override void OnStateToggled(bool state)
	{
		base.OnStateToggled(state);
		IFlagNotify flagNotify;
		if (!UseEntityParent && base.baseEntity != null && (flagNotify = base.baseEntity as IFlagNotify) != null)
		{
			flagNotify.OnFlagToggled(state);
		}
		IFlagNotify flagNotify2;
		if (UseEntityParent && base.baseEntity != null && base.baseEntity.GetParentEntity() != null && (flagNotify2 = base.baseEntity.GetParentEntity() as IFlagNotify) != null)
		{
			flagNotify2.OnFlagToggled(state);
		}
	}
}
