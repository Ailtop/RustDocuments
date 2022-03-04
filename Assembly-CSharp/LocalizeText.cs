using Rust.Localization;
using UnityEngine;

public class LocalizeText : MonoBehaviour, IClientComponent, ILocalize
{
	public enum SpecialMode
	{
		None = 0,
		AllUppercase = 1,
		AllLowercase = 2
	}

	public string token;

	[TextArea]
	public string english;

	public string append;

	public SpecialMode specialMode;

	public string LanguageToken
	{
		get
		{
			return token;
		}
		set
		{
			token = value;
		}
	}

	public string LanguageEnglish
	{
		get
		{
			return english;
		}
		set
		{
			english = value;
		}
	}
}
