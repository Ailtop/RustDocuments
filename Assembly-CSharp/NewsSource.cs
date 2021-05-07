using System;
using System.Text;
using System.Text.RegularExpressions;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class NewsSource : MonoBehaviour
{
	private static readonly Regex BbcodeParse = new Regex("([^\\[]*)(?:\\[(\\w+)(?:=([^\\]]+))?\\](.*?)\\[\\/\\2\\])?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	public RustText title;

	public RustText date;

	public RustText authorName;

	public HttpImage coverImage;

	public RectTransform container;

	public Button button;

	public RustText paragraphTemplate;

	public HttpImage imageTemplate;

	public HttpImage youtubeTemplate;

	private static readonly string[] BulletSeparators = new string[1] { "[*]" };

	public void Awake()
	{
		GA.DesignEvent("news:view");
	}

	public void OnEnable()
	{
		if (SteamNewsSource.Stories != null && SteamNewsSource.Stories.Length != 0)
		{
			SetStory(SteamNewsSource.Stories[0]);
		}
	}

	public void SetStory(SteamNewsSource.Story story)
	{
		PlayerPrefs.SetInt("lastNewsDate", story.date);
		container.DestroyAllChildren();
		title.text = story.name;
		authorName.text = "by " + story.author;
		string text = NumberExtensions.FormatSecondsLong(Epoch.Current - story.date);
		date.text = "Posted " + text + " ago";
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(delegate
		{
			Debug.Log("Opening URL: " + story.url);
			UnityEngine.Application.OpenURL(story.url);
		});
		string firstImage = null;
		StringBuilder currentParagraph = new StringBuilder();
		ParseBbcode(currentParagraph, story.text, ref firstImage);
		AppendParagraph(currentParagraph);
		if (firstImage != null)
		{
			coverImage.Load(firstImage);
		}
	}

	private void ParseBbcode(StringBuilder currentParagraph, string bbcode, ref string firstImage, int depth = 0)
	{
		foreach (Match item in BbcodeParse.Matches(bbcode))
		{
			string value = item.Groups[1].Value;
			string value2 = item.Groups[2].Value;
			string value3 = item.Groups[3].Value;
			string value4 = item.Groups[4].Value;
			currentParagraph.Append(value);
			switch (value2.ToLowerInvariant())
			{
			case "previewyoutube":
				if (depth == 0)
				{
					string[] array2 = value3.Split(';');
					AppendYouTube(currentParagraph, array2[0]);
				}
				break;
			case "h1":
			case "h2":
				currentParagraph.Append("<size=200%>");
				currentParagraph.Append(value4);
				currentParagraph.Append("</size>");
				break;
			case "b":
				currentParagraph.Append("<b>");
				ParseBbcode(currentParagraph, value4, ref firstImage, depth + 1);
				currentParagraph.Append("</b>");
				break;
			case "u":
				currentParagraph.Append("<u>");
				ParseBbcode(currentParagraph, value4, ref firstImage, depth + 1);
				currentParagraph.Append("</u>");
				break;
			case "i":
				currentParagraph.Append("<i>");
				ParseBbcode(currentParagraph, value4, ref firstImage, depth + 1);
				currentParagraph.Append("</i>");
				break;
			case "strike":
				currentParagraph.Append("<s>");
				ParseBbcode(currentParagraph, value4, ref firstImage, depth + 1);
				currentParagraph.Append("</s>");
				break;
			case "noparse":
				currentParagraph.Append(value4);
				break;
			case "url":
				currentParagraph.Append(value4);
				currentParagraph.Append(" (");
				currentParagraph.Append(value3);
				currentParagraph.Append(")");
				break;
			case "list":
			{
				currentParagraph.AppendLine();
				string[] array = GetBulletPoints(value4);
				foreach (string text3 in array)
				{
					if (!string.IsNullOrWhiteSpace(text3))
					{
						currentParagraph.Append("\t• ");
						currentParagraph.Append(text3.Trim());
						currentParagraph.AppendLine();
					}
				}
				break;
			}
			case "olist":
			{
				currentParagraph.AppendLine();
				string[] bulletPoints = GetBulletPoints(value4);
				int num = 1;
				string[] array = bulletPoints;
				foreach (string text2 in array)
				{
					if (!string.IsNullOrWhiteSpace(text2))
					{
						currentParagraph.AppendFormat("\t{0} ", num++);
						currentParagraph.Append(text2.Trim());
						currentParagraph.AppendLine();
					}
				}
				break;
			}
			case "img":
				if (depth == 0)
				{
					string text = value4.Trim();
					if (firstImage == null)
					{
						firstImage = text;
					}
					AppendImage(currentParagraph, text);
				}
				break;
			}
		}
	}

	private static string[] GetBulletPoints(string listContent)
	{
		return listContent?.Split(BulletSeparators, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
	}

	private void AppendParagraph(StringBuilder currentParagraph)
	{
		if (currentParagraph.Length != 0)
		{
			string text = currentParagraph.ToString();
			currentParagraph.Clear();
			RustText rustText = UnityEngine.Object.Instantiate(paragraphTemplate, container);
			rustText.SetActive(true);
			rustText.SetText(text);
		}
	}

	private void AppendImage(StringBuilder currentParagraph, string url)
	{
		AppendParagraph(currentParagraph);
		HttpImage httpImage = UnityEngine.Object.Instantiate(imageTemplate, container);
		httpImage.SetActive(true);
		httpImage.Load(url);
	}

	private void AppendYouTube(StringBuilder currentParagraph, string videoId)
	{
		AppendParagraph(currentParagraph);
		HttpImage httpImage = UnityEngine.Object.Instantiate(youtubeTemplate, container);
		httpImage.SetActive(true);
		httpImage.Load("https://img.youtube.com/vi/" + videoId + "/maxresdefault.jpg");
		RustButton component = httpImage.GetComponent<RustButton>();
		if (component != null)
		{
			string videoUrl = "https://www.youtube.com/watch?v=" + videoId;
			component.OnReleased.AddListener(delegate
			{
				Debug.Log("Opening URL: " + videoUrl);
				UnityEngine.Application.OpenURL(videoUrl);
			});
		}
	}
}
