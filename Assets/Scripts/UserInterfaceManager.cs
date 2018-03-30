using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class UserInterfaceManager : SuperClass {
	public Text widthValueText;
	public Text heightValueText;
	public Slider widthSlider;
	public Slider heightSlider;
	public Toggle colorblindOnToggle;
	public Toggle colorblindOffToggle;
	public GameObject preparationScreen;
	public bool tick;

	private GridManager GridManagerObject;



	// Use this for initialization
	void Start () {
		GridManagerObject = GridManager.instance;


		InitializeUI();
	}
	
	// Update is called once per frame
	void Update () {
		if (tick) {
			StartGameButton();
			tick = false;
		}
	}


	public void WidthSliderChange() {
		/* This will allow only odd numbers to protect symmetric visual */
		widthValueText.text = ((widthSlider.value-MINIMUM_GRID_WIDTH)*DOUBLE + MINIMUM_GRID_WIDTH).ToString(); 
	}



	public void HeightSliderChange() {
		heightValueText.text = heightSlider.value.ToString();
	}



	/* Sends options to required objects and starts game */
	public void StartGameButton() {
		GridManagerObject.SetGridHeight((int)heightSlider.value);
		GridManagerObject.SetGridWidth((int)(widthSlider.value-MINIMUM_GRID_WIDTH)*DOUBLE + MINIMUM_GRID_WIDTH);
		GridManagerObject.InitializeGrid();
		preparationScreen.SetActive(false);
	}



	/* Sets default values to UI elements */
	private void InitializeUI() {
		heightSlider.value = DEFAULT_GRID_HEIGHT;
		widthSlider.value = DEFAULT_GRID_WIDTH;
		colorblindOnToggle.isOn = DEFAULT_COLORBLIND_ON;
		colorblindOffToggle.isOn = DEFAULT_COLORBLIND_OFF;
		widthValueText.text = ((widthSlider.value-MINIMUM_GRID_WIDTH)*DOUBLE + MINIMUM_GRID_WIDTH).ToString();
		heightValueText.text = heightSlider.value.ToString();
	}
}
