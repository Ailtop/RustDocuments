using System;
using Facepunch;

[Serializable]
public class TokenisedPhrase : Translate.Phrase
{
	public override string translated
	{
		get
		{
			string translated = base.translated;
			if (!translated.Contains("["))
			{
				return translated;
			}
			translated = translated.Replace("[inventory.toggle]", string.Format("[{0}]", Input.GetButtonWithBind("inventory.toggle").ToUpper()));
			translated = translated.Replace("[inventory.togglecrafting]", string.Format("[{0}]", Input.GetButtonWithBind("inventory.togglecrafting").ToUpper()));
			translated = translated.Replace("[+map]", string.Format("[{0}]", Input.GetButtonWithBind("+map").ToUpper()));
			translated = translated.Replace("[inventory.examineheld]", string.Format("[{0}]", Input.GetButtonWithBind("inventory.examineheld").ToUpper()));
			translated = translated.Replace("[slot2]", string.Format("[{0}]", Input.GetButtonWithBind("+slot2").ToUpper()));
			translated = translated.Replace("[attack]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+attack")).ToUpper()));
			translated = translated.Replace("[attack2]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+attack2")).ToUpper()));
			translated = translated.Replace("[+use]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+use")).ToUpper()));
			translated = translated.Replace("[+altlook]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+altlook")).ToUpper()));
			translated = translated.Replace("[+reload]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+reload")).ToUpper()));
			translated = translated.Replace("[+voice]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+voice")).ToUpper()));
			translated = translated.Replace("[+lockBreakHealthPercent]", $"{0.15f:0%}");
			return translated.Replace("[+gestures]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+gestures")).ToUpper()));
		}
	}

	public TokenisedPhrase(string t = "", string eng = "")
		: base(t, eng)
	{
	}

	public static string TranslateMouseButton(string mouseButton)
	{
		switch (mouseButton)
		{
		case "mouse0":
			return "Left Mouse";
		case "mouse1":
			return "Right Mouse";
		case "mouse2":
			return "Center Mouse";
		default:
			return mouseButton;
		}
	}
}
