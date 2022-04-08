using System;
using UnityEngine;

namespace Windows;

public class ConsoleInput
{
	public string inputString = "";

	public string[] statusText = new string[3] { "", "", "" };

	internal float nextUpdate;

	public bool valid => Console.BufferWidth > 0;

	public int lineWidth => Console.BufferWidth;

	public event Action<string> OnInputText;

	public void ClearLine(int numLines)
	{
		Console.CursorLeft = 0;
		Console.Write(new string(' ', lineWidth * numLines));
		Console.CursorTop -= numLines;
		Console.CursorLeft = 0;
	}

	public void RedrawInputLine()
	{
		ConsoleColor backgroundColor = Console.BackgroundColor;
		ConsoleColor foregroundColor = Console.ForegroundColor;
		try
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.CursorTop++;
			for (int i = 0; i < statusText.Length; i++)
			{
				Console.CursorLeft = 0;
				Console.Write(statusText[i].PadRight(lineWidth));
			}
			Console.CursorTop -= statusText.Length + 1;
			Console.CursorLeft = 0;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Green;
			ClearLine(1);
			if (inputString.Length == 0)
			{
				Console.BackgroundColor = backgroundColor;
				Console.ForegroundColor = foregroundColor;
				return;
			}
			if (inputString.Length < lineWidth - 2)
			{
				Console.Write(inputString);
			}
			else
			{
				Console.Write(inputString.Substring(inputString.Length - (lineWidth - 2)));
			}
		}
		catch (Exception)
		{
		}
		Console.BackgroundColor = backgroundColor;
		Console.ForegroundColor = foregroundColor;
	}

	internal void OnBackspace()
	{
		if (inputString.Length >= 1)
		{
			inputString = inputString.Substring(0, inputString.Length - 1);
			RedrawInputLine();
		}
	}

	internal void OnEscape()
	{
		inputString = "";
		RedrawInputLine();
	}

	internal void OnEnter()
	{
		ClearLine(statusText.Length);
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("> " + inputString);
		string obj = inputString;
		inputString = "";
		if (this.OnInputText != null)
		{
			this.OnInputText(obj);
		}
		RedrawInputLine();
	}

	public void Update()
	{
		if (!valid)
		{
			return;
		}
		if (nextUpdate < Time.realtimeSinceStartup)
		{
			RedrawInputLine();
			nextUpdate = Time.realtimeSinceStartup + 0.5f;
		}
		try
		{
			if (!Console.KeyAvailable)
			{
				return;
			}
		}
		catch (Exception)
		{
			return;
		}
		ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
		if (consoleKeyInfo.Key == ConsoleKey.Enter)
		{
			OnEnter();
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Backspace)
		{
			OnBackspace();
		}
		else if (consoleKeyInfo.Key == ConsoleKey.Escape)
		{
			OnEscape();
		}
		else if (consoleKeyInfo.KeyChar != 0)
		{
			inputString += consoleKeyInfo.KeyChar;
			RedrawInputLine();
		}
	}
}
