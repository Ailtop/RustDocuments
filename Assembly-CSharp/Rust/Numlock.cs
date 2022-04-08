using System.Runtime.InteropServices;

namespace Rust;

public static class Numlock
{
	private const byte VK_NUMLOCK = 144;

	private const uint KEYEVENTF_EXTENDEDKEY = 1u;

	private const int KEYEVENTF_KEYUP = 2;

	private const int KEYEVENTF_KEYDOWN = 0;

	public static bool IsOn => ((ushort)GetKeyState(144) & 0xFFFF) != 0;

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	private static extern short GetKeyState(int keyCode);

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

	public static void TurnOn()
	{
		if (!IsOn)
		{
			keybd_event(144, 69, 1u, 0);
			keybd_event(144, 69, 3u, 0);
		}
	}
}
