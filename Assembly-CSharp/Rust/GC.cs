using System;
using UnityEngine;

namespace Rust
{
	public class GC : MonoBehaviour, IClientComponent
	{
		public static bool Enabled => true;

		public static void Collect()
		{
			System.GC.Collect();
		}
	}
}
