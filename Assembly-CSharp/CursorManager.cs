using UnityEngine;

public class CursorManager : SingletonComponent<CursorManager>
{
	private static int iHoldOpen;

	private static int iPreviousOpen;

	private static float lastTimeVisible;

	private static float lastTimeInvisible;

	private void Update()
	{
		if (!(SingletonComponent<CursorManager>.Instance != this))
		{
			if (iHoldOpen == 0 && iPreviousOpen == 0)
			{
				SwitchToGame();
			}
			else
			{
				SwitchToUI();
			}
			iPreviousOpen = iHoldOpen;
			iHoldOpen = 0;
		}
	}

	public void SwitchToGame()
	{
		if (Cursor.lockState != CursorLockMode.Locked)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
		if (Cursor.visible)
		{
			Cursor.visible = false;
		}
		lastTimeInvisible = Time.time;
	}

	private void SwitchToUI()
	{
		if (Cursor.lockState != 0)
		{
			Cursor.lockState = CursorLockMode.None;
		}
		if (!Cursor.visible)
		{
			Cursor.visible = true;
		}
		lastTimeVisible = Time.time;
	}

	public static void HoldOpen(bool cursorVisible = false)
	{
		iHoldOpen++;
	}

	public static bool WasVisible(float deltaTime)
	{
		return Time.time - lastTimeVisible <= deltaTime;
	}

	public static bool WasInvisible(float deltaTime)
	{
		return Time.time - lastTimeInvisible <= deltaTime;
	}
}
