using Rust.UI;
using TMPro;
using UnityEngine;

namespace Facepunch.UI;

public class ESPPlayerInfo : MonoBehaviour
{
	public Vector3 WorldOffset;

	public RustText Text;

	public TextMeshProUGUI[] TextElements;

	public RustIcon Loading;

	public GameObject ClanElement;

	public RustText ClanText;

	public CanvasGroup group;

	public Gradient gradientNormal;

	public Gradient gradientTeam;

	public Color TeamColor;

	public Color ClanColor;

	public Color AllyColor = Color.blue;

	public Color EnemyColor;

	public QueryVis visCheck;

	public BasePlayer Entity { get; set; }
}
