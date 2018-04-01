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
	private Hexagon selectedHexagon;
	private List<List<Hexagon>> gameGrid;
	private List<Hexagon> outlineList;
	private List<Color> colorList;

	/* Coroutine status variables */
	private bool gameInitializiationStatus;
	private bool hexagonRotationStatus;
	private bool startExplosion;
	private bool hexagonExplosionStatus;



	/* Assigning singleton if available */
	void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}

	void Start() {
		startExplosion = false;
		hexagonRotationStatus = false;
		hexagonExplosionStatus = false;
		gameInitializiationStatus = false;
		outlineList = new List<Hexagon>();
		gameGrid = new List<List<Hexagon>>();
	}

	void FixedUpdate() {
		if (startExplosion) {
			startExplosion = false;
			UnfreezeGrid();
			StartCoroutine(FillTheBlanks(GetMissingCells(CheckExplosion(gameGrid))));
		}
	}


	/* Wrapper function for grid initializer coroutine. Width and height should be set before this call */
	public void InitializeGrid() {
		float startX = GetGridStartCoordinateX();
		Vector3 startPos = new Vector3(0, 10, 0);


		/* Initialize grid list */
		for (int i = 0; i<GetGridWidth(); ++i) {
			List<Hexagon> subList = new List<Hexagon>();
			gameGrid.Add(subList);
		}

		/* Fill grid with hexagons */
		StartCoroutine(InitialHexagonProducer(startX, startPos));
	}



	/* Function to select the hex group on touch position, returns the selected hexagon */
	public Hexagon Select(Collider2D collider) {
		/* If selection is different than current hex, reset status and variable */
		if (selectedHexagon == null || !selectedHexagon.GetComponent<Collider2D>().Equals(collider)) {
			selectedHexagon = collider.gameObject.GetComponent<Hexagon>();
			selectionStatus = 0;
		}

		/* Else increase selection status without exceeding total number */
		else {
			selectionStatus = (++selectionStatus) % SELECTION_STATUS_COUNT;
		}

		/* Clear previous outlines */
		DestructOutline();

		/* Find hex group to be outlined */
		outlineList = FindHexagonGroup();
		/* Build outline around hex group */
		ConstructOutline();

		return selectedHexagon;
	}



	/* Function to rotate the hex group on touch position */
	public void Rotate(bool clockWise) {
		/* Specifying that rotation started and destroying outliner*/
		hexagonRotationStatus = true;
		DestructOutline();
		//FreezeGrid();
		StartCoroutine(RotationCheckCoroutine(clockWise));
	}



	/* Returns a list that contains hexagons which are ready to explode, returns an empty list if there is none */
	private List<Hexagon> CheckExplosion(List<List<Hexagon>> listToCheck) {
		List<Hexagon> neighbourList = new List<Hexagon>();
		List<Hexagon> explosiveList = new List<Hexagon>();
		Hexagon currentHexagon;
		Hexagon.NeighbourHexes currentNeighbours;
		Color currentColor;

		
		for (int i=0; i<listToCheck.Count; ++i) {
			for (int j=0; j<listToCheck[i].Count; ++j) {
				/* Take current hexagon informations */
				currentHexagon = listToCheck[i][j];
				currentColor = currentHexagon.GetColor();
				currentNeighbours = currentHexagon.GetNeighbours();

				/* Fill neighbour list with up-upright-downright neighbours with valid positions */
				if (IsValid(currentNeighbours.up)) neighbourList.Add(gameGrid[(int)currentNeighbours.up.x][(int)currentNeighbours.up.y]);
				else neighbourList.Add(null);

				if (IsValid(currentNeighbours.upRight))	neighbourList.Add(gameGrid[(int)currentNeighbours.upRight.x][(int)currentNeighbours.upRight.y]);
				else neighbourList.Add(null);

				if (IsValid(currentNeighbours.downRight)) neighbourList.Add(gameGrid[(int)currentNeighbours.downRight.x][(int)currentNeighbours.downRight.y]);
				else neighbourList.Add(null);


				/* If current 3 hexagons are all same color then add them to explosion list */
				for (int k=0; k<neighbourList.Count-1; ++k) {
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


	#region ExplosionMethods
	/* Function to clear explosive hexagons and tidy up the grid */
	private List<int> GetMissingCells(List<Hexagon> list) {
		List<int> missingColumns = new List<int>();

		UserInterfaceManager.instance.Score(list.Count);
		/* Remove hexagons from game grid */
		foreach (Hexagon hex in list) {
			gameGrid[hex.GetX()].Remove(hex);
			missingColumns.Add(hex.GetX());
			Destroy(hex.gameObject);
		}

		/* Re-assign left hexagon positions */
		foreach (int i in missingColumns) {
			for (int j=0; j<gameGrid[i].Count; ++j) {
				gameGrid[i][j].SetY(j);
			}
		}


		return missingColumns;
	}



	/* Function to produce new hexagons on missing columns */
	private IEnumerator FillTheBlanks(List<int> columns) {
		float startX = GetGridStartCoordinateX();
		Vector3 startVector = HEX_START_POSITION;
		
		/* Create new Hexagon prefab and assign to grid */
		foreach (int col in columns) {

			startVector.x = startX + (HEX_DISTANCE_HORIZONTAL * col);
			GameObject newObj = Instantiate(hexPrefab, startVector, Quaternion.identity, hexParent.transform);
			Hexagon newHex = newObj.GetComponent<Hexagon>();

			gameGrid[col].Add(newHex);
			newHex.SetX(col);
			newHex.SetY(gameGrid[col].IndexOf(newHex));
			newHex.SetColor(colorList[(int)(Random.value * 10)%colorList.Count]);

			startVector.x += HEX_DISTANCE_HORIZONTAL;
			yield return new WaitForSeconds(0.03f);
		}

		/* Freeze grid after filling with new hexagons */
		//FreezeGrid();
	}
	#endregion



	#region SelectHelpers
	/* Helper function for Select() to find all 3 hexagons to be outlined */
	private List<Hexagon> FindHexagonGroup() {
		List<Hexagon> returnValue = new List<Hexagon>();
		Vector2 firstPos, secondPos;


		/* Finding 2 other required hexagon coordinates on grid */
		FindOtherHexagons(out firstPos, out secondPos);

		/* Adding selected hexagons in pivot-first-second order to list */
		if (selectedHexagon == null || gameGrid[(int)firstPos.x][(int)firstPos.y] == null || gameGrid[(int)secondPos.x][(int)secondPos.y] == null)
			PrintGameGrid();

		returnValue.Add(selectedHexagon);
		returnValue.Add(gameGrid[(int)firstPos.x][(int)firstPos.y].GetComponent<Hexagon>());
		returnValue.Add(gameGrid[(int)secondPos.x][(int)secondPos.y].GetComponent<Hexagon>());


		return returnValue;
	}

	
	/* Helper function for FindHexagonGroup() to locate two other hexagons */
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
			} else {
				breakLoop = true;
			}
		} while (!breakLoop);
	}
	#endregion



	#region RotateHelpers
	/* Function to check if all hexagons finished rotating */
	private IEnumerator RotationCheckCoroutine(bool clockWise) {
		List<Hexagon> explosiveList = null;


		for (int i=0; i<outlineList.Count; ++i) {
			/* Swap hexagons and wait until they are completed rotation */
			SwapHexagons(clockWise);
			yield return new WaitForSeconds(0.3f);

			/* Check if there is any explosion available, break loop if it is */
			explosiveList = CheckExplosion(gameGrid);
			if (explosiveList.Count > ZERO)
				break;
		}


		/* If there are explosive hexagons set status flag and indicate rotation is over */
		startExplosion = explosiveList.Count > ZERO;
		hexagonRotationStatus = false;
	}



	/* Helper function to swap positions of currently selected 3 hexagons 
	 * TODO: Bad function, try to improve (look for more clean 3 variable swap)
	 */
	private void SwapHexagons(bool clockWise) {
		int x1, x2, x3, y1, y2, y3;
		Vector2 pos1, pos2, pos3;
		Hexagon first, second, third;


		/* Taking each position to local variables to prevent data loss during rotation */
		first = outlineList[0];
		second = outlineList[1];
		third = outlineList[2];

		x1 = first.GetX();
		x2 = second.GetX();
		x3 = third.GetX();

		y1 = first.GetY();
		y2 = second.GetY();
		y3 = outlineList[2].GetY();

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
		foreach (Hexagon outlinedHexagon in outlineList) {
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



	/* Function to initialize grid with a delay between each hexagon instantiation */
	private IEnumerator InitialHexagonProducer (float startPos, Vector3 startVector) {
		gameInitializiationStatus = true;


		/* Create desired number of Hexagons in given grid size */
		for (int i = 0; i<gridHeight; ++i) {
			startVector.x = startPos;
			for (int j = 0; j<gridWidth; ++j) {
				GameObject newObj = Instantiate(hexPrefab, startVector, Quaternion.identity, hexParent.transform);
				Hexagon newHex = newObj.GetComponent<Hexagon>();
				newHex.SetX(j);
				newHex.SetY(i);
				newHex.SetColor(colorList[(int)(Random.value * 10)%colorList.Count]);
				//newHex.SetColor(new Color(Random.value, Random.value, Random.value));
				gameGrid[j].Add(newHex);

				startVector.x += HEX_DISTANCE_HORIZONTAL;
				yield return new WaitForSeconds(0.01f);
			}
		}

		

		/* Freeze the grid for better visualasion and indicate the end of initializiation */
		gameInitializiationStatus = false;
		/*StartCoroutine(FreezeGrid());*/
	}



	/* Freezes all hexagons on game grid */
	private IEnumerator FreezeGrid() {
		bool status;

		/* Make sure if all hexagons completed their fall state */
		do {
			status = true;
			foreach (List<Hexagon> hexList in gameGrid)
				foreach (Hexagon hex in hexList)
					if (hex.IsMoving()) {
						status = false;
					}
			yield return new WaitForSeconds(0.035f);
		} while (!status);


		foreach (List<Hexagon> hexList in gameGrid) {
			foreach (Hexagon hex in hexList) {
				hex.Freeze();
			}
		}
	}



	/* Unfreezes all the hexagons on game grid */
	private void UnfreezeGrid() {
		foreach (List<Hexagon> hexList in gameGrid) {
			foreach (Hexagon hex in hexList) {
				hex.SetFree();
			}
		}
	}



	/* Checks coroutine status variables to see if game is ready to take input */
	public bool InputAvailabile() {
		return !gameInitializiationStatus && !hexagonRotationStatus && !hexagonExplosionStatus;
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



	/* Setters & Getters */
	public void SetGridWidth(int width) { gridWidth = width; }
	public void SetGridHeight(int height) { gridHeight = height; }
	public void SetColorblindMode(bool mode) { colorblindMode = mode; }
	public void SetColorList(List<Color> list) { colorList = list; }

	public int GetGridWidth() { return gridWidth; }
	public int GetGridHeight() { return gridHeight; }
	public bool GetColorblindMode() { return colorblindMode; }
}
