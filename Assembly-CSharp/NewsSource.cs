using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using Rust.UI;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsSource : MonoBehaviour
{
	public TextMeshProUGUI title;

	public TextMeshProUGUI date;

	public TextMeshProUGUI text;

	public TextMeshProUGUI authorName;

	public HttpImage image;

	public VerticalLayoutGroup layoutGroup;

	public Button button;

	private void Awake()
	{
		GA.DesignEvent("news:view");
	}

	private void OnEnable()
	{
		if (SteamNewsSource.Stories != null && SteamNewsSource.Stories.Length != 0)
		{
			SetStory(SteamNewsSource.Stories[0]);
		}
	}

	public void SetStory(SteamNewsSource.Story story)
	{
		PlayerPrefs.SetInt("lastNewsDate", story.date);
		title.text = story.name;
		string str = NumberExtensions.FormatSecondsLong(Epoch.Current - story.date);
		date.text = "Posted " + str + " ago";
		string text = Regex.Replace(story.text, "\\[img\\].*\\[\\/img\\]", string.Empty, RegexOptions.IgnoreCase);
		text = text.Replace("\\n", "\n").Replace("\\r", "").Replace("\\\"", "\"");
		text = text.Replace("[list]", "<color=#F7EBE1aa>");
		text = text.Replace("[/list]", "</color>");
		text = text.Replace("[*]", "\t\t» ");
		text = Regex.Replace(text, "\\[(.*?)\\]", string.Empty, RegexOptions.IgnoreCase);
		text = text.Trim();
		Match match = Regex.Match(story.text, "url=(http|https):\\/\\/([\\w\\-_]+(?:(?:\\.[\\w\\-_]+)+))([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])");
		Match match2 = Regex.Match(story.text, "(http|https):\\/\\/([\\w\\-_]+(?:(?:\\.[\\w\\-_]+)+))([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])(.png|.jpg)");
		if (match != null)
		{
			string url = match.Value.Replace("url=", "");
			if (url == null || url.Trim().Length <= 0)
			{
				url = story.url;
			}
			button.gameObject.SetActive(true);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				Debug.Log("Opening URL: " + url);
				UnityEngine.Application.OpenURL(url);
			});
		}
		else
		{
			button.gameObject.SetActive(false);
		}
		this.text.text = text;
		authorName.text = $"by {story.author}";
		if (match2 != null)
		{
			image.Load(match2.Value);
		}
	}
}
