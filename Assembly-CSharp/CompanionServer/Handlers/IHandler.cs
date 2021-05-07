using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers
{
	public interface IHandler : Pool.IPooled
	{
		AppRequest Request { get; }

		ValidationResult Validate();

		void Execute();

		void SendError(string code);
	}
}
