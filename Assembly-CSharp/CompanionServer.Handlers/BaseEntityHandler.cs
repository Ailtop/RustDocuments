namespace CompanionServer.Handlers;

public abstract class BaseEntityHandler<T> : BaseHandler<T> where T : class
{
	protected AppIOEntity Entity { get; private set; }

	public override void EnterPool()
	{
		base.EnterPool();
		Entity = null;
	}

	public override ValidationResult Validate()
	{
		ValidationResult validationResult = base.Validate();
		if (validationResult != 0)
		{
			return validationResult;
		}
		AppIOEntity appIOEntity = BaseNetworkable.serverEntities.Find(base.Request.entityId) as AppIOEntity;
		if (appIOEntity == null)
		{
			return ValidationResult.NotFound;
		}
		BuildingPrivlidge buildingPrivilege = appIOEntity.GetBuildingPrivilege();
		if (buildingPrivilege != null && !buildingPrivilege.IsAuthed(base.UserId))
		{
			return ValidationResult.NotFound;
		}
		Entity = appIOEntity;
		base.Client.Subscribe(new EntityTarget(base.Request.entityId));
		return ValidationResult.Success;
	}
}
