using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatEntry : MonoBehaviour
{
	public TextMeshProUGUI text;

	public RawImage avatar;

	public CanvasGroup canvasGroup;

	public float lifeStarted;

	public ulong steamid;

	public Translate.Phrase LocalPhrase = new Translate.Phrase("local", "local");

	public Translate.Phrase CardsPhrase = new Translate.Phrase("cards", "cards");

	public Translate.Phrase TeamPhrase = new Translate.Phrase("team", "team");
}
