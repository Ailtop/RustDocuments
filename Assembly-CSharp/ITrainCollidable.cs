public interface ITrainCollidable
{
	bool CustomCollision(BaseTrain train, TriggerTrainCollisions trainTrigger);

	bool EqualNetID(BaseNetworkable other);
}
