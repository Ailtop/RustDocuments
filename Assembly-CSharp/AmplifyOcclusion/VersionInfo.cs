using System;
using UnityEngine;

namespace AmplifyOcclusion;

[Serializable]
public class VersionInfo
{
	public const byte Major = 2;

	public const byte Minor = 0;

	public const byte Release = 0;

	private static string StageSuffix = "_dev002";

	[SerializeField]
	private int m_major;

	[SerializeField]
	private int m_minor;

	[SerializeField]
	private int m_release;

	public int Number => m_major * 100 + m_minor * 10 + m_release;

	public static string StaticToString()
	{
		return $"{(byte)2}.{(byte)0}.{(byte)0}" + StageSuffix;
	}

	public override string ToString()
	{
		return $"{m_major}.{m_minor}.{m_release}" + StageSuffix;
	}

	private VersionInfo()
	{
		m_major = 2;
		m_minor = 0;
		m_release = 0;
	}

	private VersionInfo(byte major, byte minor, byte release)
	{
		m_major = major;
		m_minor = minor;
		m_release = release;
	}

	public static VersionInfo Current()
	{
		return new VersionInfo(2, 0, 0);
	}

	public static bool Matches(VersionInfo version)
	{
		if (2 == version.m_major && version.m_minor == 0)
		{
			return version.m_release == 0;
		}
		return false;
	}
}
