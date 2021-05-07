using UnityEngine;
using UnityEngine.UI;

public class fxController : MonoBehaviour
{
	public int levelFX;

	public int FX;

	public GameObject[] fxObject;

	public Text levelFXTxt;

	public Text numberFXTxt;

	public Text nameFXTxt;

	public Camera myCamera;

	private int cameraZoom = 2;

	private float[] cameraFOV = new float[3] { 40f, 50f, 60f };

	private void Start()
	{
		for (int i = 0; i < 4; i++)
		{
			fxObject[i].SetActive(false);
		}
		SetlevelFXTxt(levelFX, FX);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			levelFX++;
			levelFX = Mathf.Clamp(levelFX, 0, 3);
			DisabledFX();
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			levelFX--;
			levelFX = Mathf.Clamp(levelFX, 0, 3);
			DisabledFX();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			FX--;
			FX = Mathf.Clamp(FX, 0, 7);
			DisabledFX();
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			FX++;
			FX = Mathf.Clamp(FX, 0, 7);
			DisabledFX();
		}
		if (Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			cameraZoom = Mathf.Clamp(cameraZoom + 1, 0, 2);
		}
		if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			cameraZoom = Mathf.Clamp(cameraZoom - 1, 0, 2);
		}
		myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, cameraFOV[cameraZoom], 0.2f);
		switch (FX)
		{
		case 0:
			if (levelFX == 0)
			{
				fxObject[0].SetActive(true);
				SetNameFXTxt(0);
			}
			if (levelFX == 1)
			{
				fxObject[1].SetActive(true);
				SetNameFXTxt(1);
			}
			if (levelFX == 2)
			{
				fxObject[2].SetActive(true);
				SetNameFXTxt(2);
			}
			if (levelFX == 3)
			{
				fxObject[3].SetActive(true);
				SetNameFXTxt(3);
			}
			break;
		case 1:
			if (levelFX == 0)
			{
				fxObject[4].SetActive(true);
				SetNameFXTxt(4);
			}
			if (levelFX == 1)
			{
				fxObject[5].SetActive(true);
				SetNameFXTxt(5);
			}
			if (levelFX == 2)
			{
				fxObject[6].SetActive(true);
				SetNameFXTxt(6);
			}
			if (levelFX == 3)
			{
				fxObject[7].SetActive(true);
				SetNameFXTxt(7);
			}
			break;
		case 2:
			if (levelFX == 0)
			{
				fxObject[8].SetActive(true);
				SetNameFXTxt(8);
			}
			if (levelFX == 1)
			{
				fxObject[9].SetActive(true);
				SetNameFXTxt(9);
			}
			if (levelFX == 2)
			{
				fxObject[10].SetActive(true);
				SetNameFXTxt(10);
			}
			if (levelFX == 3)
			{
				fxObject[11].SetActive(true);
				SetNameFXTxt(11);
			}
			break;
		case 3:
			if (levelFX == 0)
			{
				fxObject[12].SetActive(true);
				SetNameFXTxt(12);
			}
			if (levelFX == 1)
			{
				fxObject[13].SetActive(true);
				SetNameFXTxt(13);
			}
			if (levelFX == 2)
			{
				fxObject[14].SetActive(true);
				SetNameFXTxt(14);
			}
			if (levelFX == 3)
			{
				fxObject[15].SetActive(true);
				SetNameFXTxt(15);
			}
			break;
		case 4:
			if (levelFX == 0)
			{
				fxObject[16].SetActive(true);
				SetNameFXTxt(16);
			}
			if (levelFX == 1)
			{
				fxObject[17].SetActive(true);
				SetNameFXTxt(17);
			}
			if (levelFX == 2)
			{
				fxObject[18].SetActive(true);
				SetNameFXTxt(18);
			}
			if (levelFX == 3)
			{
				fxObject[19].SetActive(true);
				SetNameFXTxt(19);
			}
			break;
		case 5:
			if (levelFX == 0)
			{
				fxObject[20].SetActive(true);
				SetNameFXTxt(20);
			}
			if (levelFX == 1)
			{
				fxObject[21].SetActive(true);
				SetNameFXTxt(21);
			}
			if (levelFX == 2)
			{
				fxObject[22].SetActive(true);
				SetNameFXTxt(22);
			}
			if (levelFX == 3)
			{
				fxObject[23].SetActive(true);
				SetNameFXTxt(23);
			}
			break;
		case 6:
			if (levelFX == 0)
			{
				fxObject[24].SetActive(true);
				SetNameFXTxt(24);
			}
			if (levelFX == 1)
			{
				fxObject[25].SetActive(true);
				SetNameFXTxt(25);
			}
			if (levelFX == 2)
			{
				fxObject[26].SetActive(true);
				SetNameFXTxt(26);
			}
			if (levelFX == 3)
			{
				fxObject[27].SetActive(true);
				SetNameFXTxt(27);
			}
			break;
		case 7:
			if (levelFX == 0)
			{
				fxObject[28].SetActive(true);
				SetNameFXTxt(28);
			}
			if (levelFX == 1)
			{
				fxObject[29].SetActive(true);
				SetNameFXTxt(29);
			}
			if (levelFX == 2)
			{
				fxObject[30].SetActive(true);
				SetNameFXTxt(30);
			}
			if (levelFX == 3)
			{
				fxObject[31].SetActive(true);
				SetNameFXTxt(31);
			}
			break;
		case 8:
			if (levelFX == 0)
			{
				fxObject[32].SetActive(true);
				SetNameFXTxt(32);
			}
			if (levelFX == 1)
			{
				fxObject[33].SetActive(true);
				SetNameFXTxt(33);
			}
			if (levelFX == 2)
			{
				fxObject[34].SetActive(true);
				SetNameFXTxt(34);
			}
			if (levelFX == 3)
			{
				fxObject[35].SetActive(true);
				SetNameFXTxt(35);
			}
			break;
		case 9:
			if (levelFX == 0)
			{
				fxObject[36].SetActive(true);
				SetNameFXTxt(36);
			}
			if (levelFX == 1)
			{
				fxObject[37].SetActive(true);
				SetNameFXTxt(37);
			}
			if (levelFX == 2)
			{
				fxObject[38].SetActive(true);
				SetNameFXTxt(38);
			}
			if (levelFX == 3)
			{
				fxObject[39].SetActive(true);
				SetNameFXTxt(39);
			}
			break;
		}
	}

	private void DisabledFX()
	{
		for (int i = 0; i < 32; i++)
		{
			fxObject[i].SetActive(false);
		}
		SetlevelFXTxt(levelFX, FX);
	}

	private void SetlevelFXTxt(int i, int j)
	{
		switch (i)
		{
		case 0:
			levelFXTxt.text = "Level : Light";
			break;
		case 1:
			levelFXTxt.text = "Level : Medium";
			break;
		case 2:
			levelFXTxt.text = "Level : Heavy";
			break;
		case 3:
			levelFXTxt.text = "Level : VeryHeavy";
			break;
		}
		i++;
		j++;
		numberFXTxt.text = "No.  : " + j + " / 8";
	}

	private void SetNameFXTxt(int i)
	{
		nameFXTxt.text = "Name  : " + fxObject[i].gameObject.name;
	}
}
