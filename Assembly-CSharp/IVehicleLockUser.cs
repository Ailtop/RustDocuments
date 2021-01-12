public interface IVehicleLockUser
{
	bool PlayerHasUnlockPermission(BasePlayer player);

	bool PlayerCanUseThis(BasePlayer player, ModularCarLock.LockType lockType);

	bool PlayerCanDestroyLock(BasePlayer player, BaseVehicleModule viaModule);

	void RemoveLock();
}
