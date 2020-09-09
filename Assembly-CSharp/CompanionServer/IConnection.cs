using CCTVRender;
using ProtoBuf;

namespace CompanionServer
{
	public interface IConnection : IReceiver
	{
		void Send(AppResponse response);

		void Subscribe(PlayerTarget target);

		void Unsubscribe(PlayerTarget target);

		void Subscribe(EntityTarget target);

		void Unsubscribe(EntityTarget target);
	}
}
