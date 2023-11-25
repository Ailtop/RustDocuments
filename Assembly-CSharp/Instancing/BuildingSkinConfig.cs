using System.Collections.Generic;

namespace Instancing;

public class BuildingSkinConfig
{
	public uint PrefabId;

	public List<ConditionalModelConfig> Conditionals = new List<ConditionalModelConfig>();
}
