using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ConVar;

[Factory("render")]
public class Render : ConsoleSystem
{
	public static bool use_normal_rendering = false;

	[ClientVar(Saved = true, Help = "0 = off, 1 = on (must restart client for changes to take effect)")]
	public static int instanced_rendering = 0;

	[ClientVar(ClientAdmin = true, Help = "Developer command to toggle instanced rendering at runtime to measure performance impact")]
	public static bool instanced_toggle_all = true;

	[ClientVar(ClientAdmin = true, Help = "Toggle rendering of cliffs on / off")]
	public static bool instanced_toggle_cliffs = true;

	[ClientVar(ClientAdmin = true, Help = "Toggle rendering of buildings on / off")]
	public static bool instanced_toggle_buildings = true;

	[ClientVar(ClientAdmin = true, Help = "Toggle rendering of uncategorized meshes on / off")]
	public static bool instanced_toggle_other = true;

	[ClientVar(ClientAdmin = true, Help = "Allow unity to batch together multiple draw calls")]
	public static bool multidraw = true;

	[ClientVar(ClientAdmin = true, Help = "0 = CPU, 1 = GPU")]
	public static int upload_multidraw_meshes_mode = 0;

	[ClientVar(ClientAdmin = true)]
	public static bool render_shadows = true;

	[ClientVar(Help = "Whether to call ComputeBuffer.SetData immediately or at the end of the frame")]
	public static bool computebuffer_setdata_immediate = true;

	[ClientVar(ClientAdmin = true, Help = "Set the amount of instanced renderers to show for debugging")]
	public static int max_renderers = 0;

	[ClientVar(Saved = true, Help = "Max distance for instanced rendering, can be higher than normal render distance")]
	public static float instancing_render_distance = 1000f;

	public static bool IsInstancingDisabled => true;

	public static bool IsInstancingEnabled => !IsInstancingDisabled;

	public static bool IsMultidrawEnabled => multidraw;

	[ClientVar(Name = "print_tree_counts", ClientAdmin = true, Help = "Print off count of trees to ensure server sent them all")]
	[ServerVar]
	public static void tree_entities(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Server Trees: {TreeManager.server.GetTreeCount()}");
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Name = "print_global_entities", Help = "Print off count of global building entities on the server")]
	[ClientVar(Name = "print_global_entities", ClientAdmin = true, Help = "Print off count of global building entities on the client")]
	public static void print_global_entities(Arg arg)
	{
		if (IsInstancingDisabled)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("SERVER ENTITIES:");
		Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
		foreach (GlobalEntityData value2 in GlobalNetworkHandler.server.serverData.Values)
		{
			dictionary.TryGetValue(value2.prefabId, out var value);
			dictionary[value2.prefabId] = value + 1;
		}
		KeyValuePair<uint, int>[] array = dictionary.OrderByDescending((KeyValuePair<uint, int> x) => x.Value).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<uint, int> keyValuePair = array[i];
			stringBuilder.AppendLine($"{StringPool.Get(keyValuePair.Key)} - {keyValuePair.Value}");
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	private static void global_entities_client(StringBuilder builder)
	{
		_ = IsInstancingDisabled;
	}

	[ClientVar(Name = "print_instanced_debug", Help = "Print off array size and memory usage to ensure no memory leaks & debug rendering system")]
	public static void instanced_memory_usage(Arg arg)
	{
		_ = IsInstancingDisabled;
	}

	[ClientVar(ClientAdmin = true, Help = "Spawn (default 50k) prefabs spread across the map to quickly test instanced rendering system in isolation")]
	public static void test_instancing_culling(Arg arg)
	{
		_ = IsInstancingDisabled;
	}

	[ClientVar(Name = "print_instanced_renderers", ClientAdmin = true, Help = "Print off number of each mesh inside instanced rendering system (including outside of render range)")]
	public static void instanced_renderers_debug(Arg arg)
	{
		_ = IsInstancingDisabled;
	}

	[ClientVar(Name = "print_instanced_cell", ClientAdmin = true, Help = "Print number of meshes inside a single grid")]
	public static void print_instanced_grid(Arg arg)
	{
		_ = IsInstancingDisabled;
	}

	[ClientVar(Name = "expand_instancing", ClientAdmin = true)]
	public static void expand_instancing(Arg arg)
	{
		_ = IsInstancingDisabled;
	}
}
