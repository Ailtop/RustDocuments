using Facepunch;

namespace ConVar;

public class Manifest
{
	[ServerVar]
	[ClientVar]
	public static object PrintManifest()
	{
		return Application.Manifest;
	}

	[ServerVar]
	[ClientVar]
	public static object PrintManifestRaw()
	{
		return Facepunch.Manifest.Contents;
	}
}
