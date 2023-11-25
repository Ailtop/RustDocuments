using System;
using System.Collections.Generic;

namespace Instancing;

[Serializable]
public class InstancedMeshConfig
{
	public List<InstancedLODState> states;
}
