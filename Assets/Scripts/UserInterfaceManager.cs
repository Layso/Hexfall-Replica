using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class UserInterfaceManager : SuperClass {
	public Text score;
	public Text widthValueText;
	public Text heightValueText;
	public Text colorCountText;
	public Slider widthSlider;
	public Slider heightSlider;
	public Slider colorCountSlider;
	public Dropdown colorblindDropdown;
	public GameObject preparationScreen;
	public GameObject colorSelectionParent;
	public List<Color> availableColors;
	public bool tick;

	private GridManager GridManagerObject;
	private int colorCount;
	private int blownHexagons;

	public static UserInterfaceManager instance;



	void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}

	void Start () {
		GridManagerObject = GridManager.instance;
		blownHexagons = 0;
		colorCount = 7;
		InitializeUI();
	}
	
	void Update () {
		if (tick) {
			StartGameButton();
			tick = false;
		}
	}



	public void Score(int x) {
		blownHexagons += x;
		score.text = (SCORE_CONSTANT * blownHexagons).ToString();
	}

	public void WidthSliderChange() {
		/* This will allow only odd numbers to protect symmetrical visual */
		widthValueText.text = ((widthSlider.value-MINIMUM_GRID_WIDTH)*DOUBLE + MINIMUM_GRID_WIDTH).ToString(); 
	}

	public void HeightSliderChange() {
		heightValueText.text = heightSlider.value.ToString();
	}

	public void ColorCountSliderChange() {
		int childCount = colorSelectionParent.transform.childCount;
		int newCount = (int)colorCountSlider.value;
		colorCountText.text = newCount.ToString();
		

		if (newCount > colorCount) {
			for (int i=0; i<childCount; ++i) {
				if (!colorSelectionParent.transform.GetChild(i).gameObject.activeSelf) {
					colorSelectionParent.transform.GetChild(i).gameObject.SetActive(true);
					break;
				}
			}
		}

		else if (newCount < colorCount) {
			for (int i = 0; i<childCount; ++i) {
				if (i+1>=childCount) {
					colorSelectionParent.transform.GetChild(i).gameObject.SetActive(false);
					break;
				}

				else if (!colorSelectionParent.transform.GetChild(i+1).gameObject.activeSelf) {
					colorSelectionParent.transform.GetChild(i).gameObject.SetActive(false);
					break;
				}
			}
		}

		colorCount = newCount;
	}



	/* Sends options to required objects and starts game */
	public void StartGameButton() {
		preparationScreen.SetActive(false);
		GridManagerObject.SetColorblindMode(colorblindDropdown.value != ZERO);
		GridManagerObject.SetGridHeight((int)heightSlider.value);
		GridManagerObject.SetGridWidth((int)(widthSlider.value-MINIMUM_GRID_WIDTH)*DOUBLE + MINIMUM_GRID_WIDTH);

		List<Color> colors = new List<Color>();

		colors.Add(Color.blue);
		colors.Add(Color.red);
		colors.Add(Color.yellow);
		colors.Add(Color.green);
		colors.Add(Color.cyan);
		GridManagerObject.SetColorList(colors);
		GridManagerObject.InitializeGrid();
	}



	/* Sets default values to UI elements */
	private void InitializeUI() {
		// if option file exists
			// load option file
		// else
			// load defaults

		Default();

		for (int i=0; i<colorSelectionParent.transform.childCount-colorCount; ++i) {
			colorSelectionParent.transform.GetChild(colorSelectionParent.transform.childCount - i -1).gameObject.SetActive(false);
		}
	}



	/* Assigns all options to default values */
	private void Default() {
		heightSlider.value = DEFAULT_GRID_HEIGHT;
		widthSlider.value = DEFAULT_GRID_WIDTH;
		colorCountSlider.value = DEFAULT_COLOR_COUNT;
		colorCount = DEFAULT_COLOR_COUNT;
		widthValueText.text = ((widthSlider.value-MINIMUM_GRID_WIDTH)*DOUBLE + MINIMUM_GRID_WIDTH).ToString();
		heightValueText.text = heightSlider.value.ToString();
		score.text = blownHexagons.ToString();
		// remove option file
	}
}
