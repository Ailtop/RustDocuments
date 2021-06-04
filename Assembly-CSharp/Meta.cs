using System.Collections.Generic;
using Facepunch;
using UnityEngine;

[Factory("meta")]
public class Meta : ConsoleSystem
{
	[ServerVar(Clientside = true, Help = "add <convar> <amount> - adds amount to convar")]
	public static void add(Arg args)
	{
		string @string = args.GetString(0);
		float @float = args.GetFloat(1, 0.1f);
		Command command = Find(@string);
		float result;
		if (command == null)
		{
			args.ReplyWith("Convar not found: " + (@string ?? "<null>"));
		}
		else if (args.IsClientside && command.Replicated)
		{
			args.ReplyWith("Cannot set replicated convars from the client (use sv to do this)");
		}
		else if (args.IsServerside && command.ServerAdmin && !args.IsAdmin)
		{
			args.ReplyWith("Permission denied");
		}
		else if (!float.TryParse(command.String, out result))
		{
			args.ReplyWith("Convar value cannot be parsed as a number");
		}
		else
		{
			command.Set(result + @float);
		}
	}

	[ClientVar(Help = "if_true <command> <condition> - runs a command if the condition is true")]
	public static void if_true(Arg args)
	{
		string @string = args.GetString(0);
		bool @bool = args.GetBool(1);
		if (@bool)
		{
			ConsoleSystem.Run(Option.Client, @string, @bool);
		}
	}

	[ClientVar(Help = "if_false <command> <condition> - runs a command if the condition is false")]
	public static void if_false(Arg args)
	{
		string @string = args.GetString(0);
		bool @bool = args.GetBool(1, true);
		if (!@bool)
		{
			ConsoleSystem.Run(Option.Client, @string, @bool);
		}
	}

	[ClientVar(Help = "reset_cycle <key> - resets a cycled bind to the beginning")]
	public static void reset_cycle(Arg args)
	{
		string name = args.GetString(0);
		List<KeyCode> keys;
		KeyCombos.TryParse(ref name, out keys);
		Facepunch.Input.Button button = Facepunch.Input.GetButton(name);
		if (button == null)
		{
			args.ReplyWith("Button not found");
		}
		else if (!button.Cycle)
		{
			args.ReplyWith("Button does not have a cycled bind");
		}
		else
		{
			button.CycleIndex = 0;
		}
	}

	private static Command Find(string name)
	{
		Command command = Index.Server.Find(name);
		if (command != null)
		{
			return command;
		}
		return null;
	}
}
