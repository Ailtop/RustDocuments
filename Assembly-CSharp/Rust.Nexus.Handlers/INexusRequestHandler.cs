using Facepunch;
using ProtoBuf.Nexus;

namespace Rust.Nexus.Handlers;

public interface INexusRequestHandler : Pool.IPooled
{
	Response Response { get; }

	void Execute();
}
