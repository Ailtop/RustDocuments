using ProtoBuf;

namespace CompanionServer;

public interface IConnection
{
	long ConnectionId { get; }

	IRemoteControllable CurrentCamera { get; }

	bool IsControllingCamera { get; }

	ulong ControllingSteamId { get; }

	InputState InputState { get; set; }

	void Send(AppResponse response);

	void Subscribe(PlayerTarget target);

	void Subscribe(EntityTarget target);

	bool BeginViewing(IRemoteControllable camera);

	void EndViewing();
}
