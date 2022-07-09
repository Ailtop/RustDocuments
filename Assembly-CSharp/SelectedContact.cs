using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class SelectedContact : SingletonComponent<SelectedContact>
{
	public RustText nameText;

	public RustText seenText;

	public RawImage mugshotImage;

	public Texture2D unknownMugshot;

	public InputField noteInput;

	public GameObject[] relationshipTypeTags;

	public Translate.Phrase lastSeenPrefix;

	public Translate.Phrase nowPhrase;

	public Translate.Phrase agoSuffix;

	public RustButton chatMute;
}
