using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GridManager : SuperClass {
	/* One "instance" to rule them all! */
	public static GridManager instance = null;

	/* Variables to assign from editor */
	public GameObject hexPrefab;
	public GameObject hexParent;
	public GameObject outParent;
	public Sprite outlineSprite;
	public Sprite hexagonSprite;

	/* Member variables */
	private int gridWidth;
	private int gridHeight;
	private int selectionStatus;
	private bool colorblindMode;
	private Vector2 selectedPosition;
	private Hexagon selectedHexagon;
	private List<List<Hexagon>> gameGrid;
	private List<Hexagon> selectedGroup;
	private List<Color> colorList;

	/* Coroutine status variables */
	private bool gameInitializiationStatus;
	private bool hexagonRotationStatus;
	private bool hexagonExplosionStatus;
	private bool hexagonProductionStatus;



	/* Assigning singleton if available */
	void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}

	void Start() {
		hexagonRotationStatus = false;
		hexagonExplosionStatus = false;
		hexagonProductionStatus = false;
		selectedGroup = new List<Hexagon>();
		gameGrid = new List<List<Hexagon>>();
	}

	void Update() {
	}


	/* Wrapper function for grid initializer coroutine. Width and height should be set before this call */
	public void InitializeGrid() {
		List<int> missingCells = new List<int>();


		/* Initialize gameGrid and fill missingCells */
		for (int i = 0; i<GetGridWidth(); ++i) {
			for (int j = 0; j<GetGridHeight(); ++j)
				missingCells.Add(i);

			gameGrid.Add(new List<Hexagon>());
		}

		/* Fill grid with hexagons */
		StartCoroutine(ProduceHexagons(missingCells));
	}



	/* Function to select the hex group on touch position, returns the selected hexagon */
	public void Select(Collider2D collider) {
		/* If selection is different than current hex, reset status and variable */
		if (selectedHexagon == null || !selectedHexagon.GetComponent<Collider2D>().Equals(collider)) {
			selectedHexagon = collider.gameObject.GetComponent<Hexagon>();
			selectedPosition.x = selectedHexagon.GetX();
			selectedPosition.y = selectedHexagon.GetX();
			selectionStatus = 0;
		}

		/* Else increase selection status without exceeding total number */
		else {
			selectionStatus = (++selectionStatus) % SELECTION_STATUS_COUNT;
		}

		FindHexagonGroup();
		ConstructOutline();
	}



	/* Function to rotate the hex group on touch position */
	public void Rotate(bool clockWise) {
		/* Specifying that rotation started and destroying outliner*/
		StartCoroutine(RotationCheckCoroutine(clockWise));
	}




	#region SelectHelpers
	/* Helper function for Select() to find all 3 hexagons to be outlined */
	private void FindHexagonGroup() {
		List<Hexagon> returnValue = new List<Hexagon>();
		Vector2 firstPos, secondPos;


		/* Finding 2 other required hexagon coordinates on grid */
		FindOtherHexagons(out firstPos, out secondPos);
		selectedGroup.Clear();
		selectedGroup.Add(selectedHexagon);
		selectedGroup.Add(gameGrid[(int)firstPos.x][(int)firstPos.y].GetComponent<Hexagon>());
		selectedGroup.Add(gameGrid[(int)secondPos.x][(int)secondPos.y].GetComponent<Hexagon>());
	}


	/* Helper function for FindHexagonGroup() to locate neighbours of selected hexagon */
	private void FindOtherHexagons(out Vector2 first, out Vector2 second) {
		Hexagon.NeighbourHexes neighbours = selectedHexagon.GetNeighbours();
		bool breakLoop = false;


		/* Picking correct neighbour according to selection position */
		do {
			switch (selectionStatus) {
				case 0: first = neighbours.up; second = neighbours.upRight; break;
				case 1: first = neighbours.upRight; second = neighbours.downRight; break;
				case 2: first = neighbours.downRight; second = neighbours.down; break;
				case 3: first = neighbours.down; second = neighbours.downLeft; break;
				case 4: first = neighbours.downLeft; second = neighbours.upLeft; break;
				case 5: first = neighbours.upLeft; second = neighbours.up; break;
				default: first = Vector2.zero; second = Vector2.zero; break;
			}

			/* Loop until two neighbours with valid positions are found */
			if (first.x < ZERO || first.x >= gridWidth || first.y < ZERO || first.y >= gridHeight || second.x < ZERO || second.x >= gridWidth || second.y < ZERO || second.y >= gridHeight) {
				selectionStatus = (++selectionStatus) % SELECTION_STATUS_COUNT;
			}
			else {
				breakLoop = true;
			}
		} while (!breakLoop);
	}
	#endregion



	#region RotateHelpers
	/* Function to check if all hexagons finished rotating */
	private IEnumerator RotationCheckCoroutine(bool clockWise) {
		List<Hexagon> explosiveList = null;
		bool flag = true;

		hexagonRotationStatus = true;
		DestructOutline();

		/* Rotate selected group until an explosive hexagon found or maximum rotation reached */
		hexagonRotationStatus = true;
		for (int i=0; i<selectedGroup.Count; ++i) {
			/* Swap hexagons and wait until they are completed rotation */
			SwapHexagons(clockWise);
			yield return new WaitForSeconds(0.3f);

			/* Check if there is any explosion available, break loop if it is */
			explosiveList = CheckExplosion(gameGrid);
			if (explosiveList.Count > ZERO)
				break;
		}


		/* Chaining to prevent input interruption */
		hexagonProductionStatus = true;
		hexagonRotationStatus = false;


		/* Explode the hexagons until no explosive hexagons are available */
		/*while (explosiveList.Count > ZERO) {
			if (flag) {
				StartCoroutine(ProduceHexagons(GetMissingCells(CheckExplosion(gameGrid))));
				flag = false;
			}
				
			else if (InputAvailabile()) {
				explosiveList = CheckExplosion(gameGrid);
				flag = true;
			}

			yield return new WaitForSeconds(0.3f);
		}*/
		hexagonProductionStatus = false;

		FindHexagonGroup();
		ConstructOutline();
	}



	/* Helper function to swap positions of currently selected 3 hexagons 
	 * TODO: Bad function, try to improve (look for more clean 3 variable swap)
	 */
	private void SwapHexagons(bool clockWise) {
		int x1, x2, x3, y1, y2, y3;
		Vector2 pos1, pos2, pos3;
		Hexagon first, second, third;


		/* Taking each position to local variables to prevent data loss during rotation */
		first = selectedGroup[0];
		second = selectedGroup[1];
		third = selectedGroup[2];

		x1 = first.GetX();
		x2 = second.GetX();
		x3 = third.GetX();

		y1 = first.GetY();
		y2 = second.GetY();
		y3 = third.GetY();

		pos1 = first.transform.position;
		pos2 = second.transform.position;
		pos3 = third.transform.position;


		/* If rotation is clokwise, rotate to the position of element on next index, else rotate to previous index */
		if (clockWise) {
			first.Rotate(x2, y2, pos2);
			gameGrid[x2][y2] = first;

			second.Rotate(x3, y3, pos3);
			gameGrid[x3][y3] = second;

			third.Rotate(x1, y1, pos1);
			gameGrid[x1][y1] = third;
		}
		else {
			first.Rotate(x3, y3, pos3);
			gameGrid[x3][y3] = first;

			second.Rotate(x1, y1, pos1);
			gameGrid[x1][y1] = second;

			third.Rotate(x2, y2, pos2);
			gameGrid[x2][y2] = third;
		}
	}
	#endregion



	#region ExplosionHelpers
	/* Returns a list that contains hexagons which are ready to explode, returns an empty list if there is none */
	private List<Hexagon> CheckExplosion(List<List<Hexagon>> listToCheck) {
		List<Hexagon> neighbourList = new List<Hexagon>();
		List<Hexagon> explosiveList = new List<Hexagon>();
		Hexagon currentHexagon;
		Hexagon.NeighbourHexes currentNeighbours;
		Color currentColor;


		for (int i = 0; i<listToCheck.Count; ++i) {
			for (int j = 0; j<listToCheck[i].Count; ++j) {
				/* Take current hexagon informations */
				currentHexagon = listToCheck[i][j];
				currentColor = currentHexagon.GetColor();
				currentNeighbours = currentHexagon.GetNeighbours();

				/* Fill neighbour list with up-upright-downright neighbours with valid positions */
				if (IsValid(currentNeighbours.up)) neighbourList.Add(gameGrid[(int)currentNeighbours.up.x][(int)currentNeighbours.up.y]);
				else neighbourList.Add(null);

				if (IsValid(currentNeighbours.upRight)) neighbourList.Add(gameGrid[(int)currentNeighbours.upRight.x][(int)currentNeighbours.upRight.y]);
				else neighbourList.Add(null);

				if (IsValid(currentNeighbours.downRight)) neighbourList.Add(gameGrid[(int)currentNeighbours.downRight.x][(int)currentNeighbours.downRight.y]);
				else neighbourList.Add(null);


				/* If current 3 hexagons are all same color then add them to explosion list */
				for (int k = 0; k<neighbourList.Count-1; ++k) {
					if (neighbourList[k] != null && neighbourList[k+1] != null) {
						if (neighbourList[k].GetColor() == currentColor && neighbourList[k+1].GetColor() == currentColor) {
							if (!explosiveList.Contains(neighbourList[k]))
								explosiveList.Add(neighbourList[k]);
							if (!explosiveList.Contains(neighbourList[k+1]))
								explosiveList.Add(neighbourList[k+1]);
							if (!explosiveList.Contains(currentHexagon))
								explosiveList.Add(currentHexagon);
						}
					}
				}

				neighbourList.Clear();
			}
		}


		return explosiveList;
	}



	/* Function to clear explosive hexagons and tidy up the grid */
	private List<int> GetMissingCells(List<Hexagon> list) {
		List<int> missingColumns = new List<int>();
		float positionX, positionY;


		/* Remove hexagons from game grid */
		foreach (Hexagon hex in list) {
			gameGrid[hex.GetX()].Remove(hex);
			missingColumns.Add(hex.GetX());
			Destroy(hex.gameObject);
		}

		/* Re-assign left hexagon positions */
		foreach (int i in missingColumns) {
			for (int j=0; j<gameGrid[i].Count; ++j) {
				positionX = GetGridStartCoordinateX() + (HEX_DISTANCE_HORIZONTAL * i);
				positionY = (HEX_DISTANCE_VERTICAL * j * 2) + GRID_VERTICAL_OFFSET + (OnStepper(i) ? HEX_DISTANCE_VERTICAL : ZERO);
				gameGrid[i][j].SetY(j);
				gameGrid[i][j].ChangeWorldPosition(new Vector3(positionX, positionY, ZERO));
			}
		}

		/* Increase score */
		UserInterfaceManager.instance.Score(list.Count);
		/* Construct new outline */
		selectedHexagon = gameGrid[(int)selectedPosition.x][(int)selectedPosition.y];
		FindHexagonGroup();
		
		return missingColumns;
	}
	#endregion



	#region OutlineMethods
	/* Function to clear the outline objects */
	private void DestructOutline() {
		if (outParent.transform.childCount > ZERO) {
			foreach (Transform child in outParent.transform)
				Destroy(child.gameObject);
		}
	}
	


	/* Function to build outline */
	private void ConstructOutline() {
		/* Creating outlines by creating black hexagons on same position with selected 
		 * hexagons and making them bigger than actual hexagons. AKA fake shader programming 
		 * Yes, I should learn shader programming... 
		 */
		foreach (Hexagon outlinedHexagon in selectedGroup) {
			GameObject go = outlinedHexagon.gameObject;
			GameObject outline = new GameObject("Outline");
			GameObject outlineInner = new GameObject("Inner Object");

			outline.transform.parent = outParent.transform;

			outline.AddComponent<SpriteRenderer>();
			outline.GetComponent<SpriteRenderer>().sprite = outlineSprite;
			outline.GetComponent<SpriteRenderer>().color = Color.white;
			outline.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -1);
			outline.transform.localScale = HEX_OUTLINE_SCALE;

			outlineInner.AddComponent<SpriteRenderer>();
			outlineInner.GetComponent<SpriteRenderer>().sprite = hexagonSprite;
			outlineInner.GetComponent<SpriteRenderer>().color = go.GetComponent<SpriteRenderer>().color;
			outlineInner.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -2);
			outlineInner.transform.localScale = go.transform.localScale;
			outlineInner.transform.parent = outline.transform;
		}
	}
	#endregion


	
	/* Produces new hexagons on given columns */
	private IEnumerator ProduceHexagons(List<int> columns) {
		Vector3 startPosition;
		float positionX, positionY;
		float startX = GetGridStartCoordinateX();


		/* Indication for the beginning of hexagon production */
		hexagonProductionStatus = true;

		/* Produce new hexagon, set variables  */
		foreach (int i in columns) {
			/* Instantiate new hexagon and give a little delay */
			positionX = startX + (HEX_DISTANCE_HORIZONTAL * i);
			positionY = (HEX_DISTANCE_VERTICAL * gameGrid[i].Count * 2)  + GRID_VERTICAL_OFFSET + (OnStepper(i) ? HEX_DISTANCE_VERTICAL : ZERO);
			startPosition = new Vector3(positionX, positionY, ZERO);

			GameObject newObj = Instantiate(hexPrefab, HEX_START_POSITION, Quaternion.identity, hexParent.transform);
			Hexagon newHex = newObj.GetComponent<Hexagon>();
			yield return new WaitForSeconds(DELAY_TO_PRODUCE_HEXAGON);


			/* Set world and grid positions of hexagon */
			newHex.ChangeGridPosition(new Vector2(i, gameGrid[i].Count));
			newHex.SetColor(colorList[(int)(Random.value * RANDOM_SEED)%colorList.Count]);
			newHex.ChangeWorldPosition(startPosition);
			gameGrid[i].Add(newHex);
		}

		/* Indication for the end of hexagon production */
		hexagonProductionStatus = false;
	}



	#region GeneralHelpers
	public bool OnStepper(int x) {
		int midIndex = GetGridWidth()/HALF;
		return (midIndex%2 == x%2);
	}

	/* Checks coroutine status variables to see if game is ready to take input */
	public bool InputAvailabile() {
		return !hexagonProductionStatus;
	}



	/* Helper function to find the x coordinate of the world position of first column */
	private float GetGridStartCoordinateX() {
		return gridWidth/HALF * -HEX_DISTANCE_HORIZONTAL;
	}



	/* Helper function to validate a position if it is in game grid */
	private bool IsValid(Vector2 pos) {
		return pos.x >= ZERO && pos.x < GetGridWidth() && pos.y >= ZERO && pos.y <GetGridHeight();
	}



	private void PrintGameGrid() {
		string map = "";


		for (int i = GetGridHeight()-1; i>=0; --i) {
			for (int j = 0; j<GetGridWidth(); ++j) {
				if (gameGrid[j][i] == null)
					map +=  "0 - ";
				else
					map += "1 - ";
			}

			map += "\n";
		}

		print(map);
	}
	#endregion


	/* Setters & Getters */
	public void SetGridWidth(int width) { gridWidth = width; }
	public void SetGridHeight(int height) { gridHeight = height; }
	public void SetColorblindMode(bool mode) { colorblindMode = mode; }
	public void SetColorList(List<Color> list) { colorList = list; }

	public int GetGridWidth() { return gridWidth; }
	public int GetGridHeight() { return gridHeight; }
	public bool GetColorblindMode() { return colorblindMode; }
	public Hexagon GetSelectedHexagon() { return selectedHexagon; }
}
