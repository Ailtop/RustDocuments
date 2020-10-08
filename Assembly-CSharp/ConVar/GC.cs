using Rust;
using UnityEngine;
using UnityEngine.Scripting;

namespace ConVar
{
	[Factory("gc")]
	public class GC : ConsoleSystem
	{
		[ClientVar]
		public static bool buffer_enabled = true;

		[ClientVar]
		public static int debuglevel = 1;

		private static int m_buffer = 256;

		[ClientVar]
		public static int buffer
		{
			get
			{
				return m_buffer;
			}
			set
			{
				m_buffer = Mathf.Clamp(value, 64, 4096);
			}
		}

		[ServerVar]
		[ClientVar]
		public static bool incremental_enabled
		{
			get
			{
				return GarbageCollector.isIncremental;
			}
			set
			{
				Debug.LogWarning("Cannot set gc.incremental as it is read only");
			}
		}

		[ServerVar]
		[ClientVar]
		public static int incremental_milliseconds
		{
			get
			{
				return (int)(GarbageCollector.incrementalTimeSliceNanoseconds / 1000000uL);
			}
			set
			{
				GarbageCollector.incrementalTimeSliceNanoseconds = (ulong)(1000000L * (long)Mathf.Max(value, 0));
			}
		}

		[ServerVar]
		[ClientVar]
		public static bool enabled
		{
			get
			{
				return Rust.GC.Enabled;
			}
			set
			{
				Debug.LogWarning("Cannot set gc.enabled as it is read only");
			}
		}

		[ClientVar]
		[ServerVar]
		public static void collect()
		{
			Rust.GC.Collect();
		}

		[ClientVar]
		[ServerVar]
		public static void unload()
		{
			Resources.UnloadUnusedAssets();
		}

		[ServerVar]
		[ClientVar]
		public static void alloc(Arg args)
		{
			byte[] array = new byte[args.GetInt(0, 1048576)];
			args.ReplyWith("Allocated " + array.Length + " bytes");
		}
	}
}
