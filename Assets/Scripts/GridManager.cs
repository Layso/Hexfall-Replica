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
	private Vector2 touchStartPos;
	private Hexagon selectedHexagon;
	private List<Hexagon> hexList;
	private List<Hexagon> outlineList;

	/* Coroutine status variables */
	private bool gameInitializiationStatus;
	private bool hexagonRotationStatus;



	/* Assigning singleton if available */
	private void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}

	private void Start() {
		gameInitializiationStatus = false;
		hexagonRotationStatus = false;
		hexList = new List<Hexagon>();
		outlineList = new List<Hexagon>();
	}



	/* Wrapper function for grid initializer coroutine. Width and height should be set before this call */
	public void InitializeGrid() {
		float startX = gridWidth/HALF * -HEX_DISTANCE_HORIZONTAL;
		Vector3 startPos = new Vector3(0, 10, 0);
		

		/* Controlling prefab to avoid null pointer exception */
		if (hexPrefab != null) {
			StartCoroutine(InitialHexagonProducer(startX, startPos));
		}
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



	/* 
		 * TODO
		 * Make here look nice 
		 */
	/* Function to rotate the hex group on touch position */
	public void Rotate(bool clockWise) {
		int x1, x2, x3, y1, y2, y3;
		Vector2 pos1, pos2, pos3;


		/* Specifying that rotation started and destroying outliner*/
		hexagonRotationStatus = true;
		DestructOutline();
		FreezeGrid();

		x1 = outlineList[0].GetX();
		x2 = outlineList[1].GetX();
		x3 = outlineList[2].GetX();

		y1 = outlineList[0].GetY();
		y2 = outlineList[1].GetY();
		y3 = outlineList[2].GetY();

		pos1 = outlineList[0].transform.position;
		pos2 = outlineList[1].transform.position;
		pos3 = outlineList[2].transform.position;


		if (clockWise) {
			outlineList[2].Rotate(x1, y1, pos1);
			outlineList[1].Rotate(x3, y3, pos3);
			outlineList[0].Rotate(x2, y2, pos2);
		}

		else {
			outlineList[1].Rotate(x1, y1, pos1);
			outlineList[2].Rotate(x2, y2, pos2);
			outlineList[0].Rotate(x3, y3, pos3);
		}

		StartCoroutine(RotationCheckCoroutine());


		
		List<Hexagon> explList = CheckExplosion();
		foreach (Hexagon hex in CheckExplosion()) {
			print("(" + hex.GetX() + ", " + hex.GetY() + ")");
		}
	}



	/* Checks coroutine status variables to see if game is ready to take input */
	public bool InputAvailabile() {
		return !gameInitializiationStatus && !hexagonRotationStatus;
	}



	/* Returns a list that contains hexagons which are ready to explode, returns an empty list if there is none */
	private List<Hexagon> CheckExplosion() {
		List<Hexagon> explosiveList = new List<Hexagon>();



		return explosiveList;
	}



	#region RotateHelpers
	/* Function to check if all hexagons finished rotating */
	private IEnumerator RotationCheckCoroutine() {
		bool exitStatus;

		do {
			exitStatus = true;
			foreach (Hexagon hex in outlineList) {
				if (hex.IsRotating())
					exitStatus = false;
			}
			yield return new WaitForSeconds(0.2f);
		} while (exitStatus);

		hexagonRotationStatus = false;
		ConstructOutline();
	}
	#endregion



	#region SelectHelpers
	/* Helper function for Select() to find all 3 hexagons to be outlined */
	private List<Hexagon> FindHexagonGroup() {
		List<Hexagon> returnValue = new List<Hexagon>();
		Hexagon firstHex = null, secondHex = null;
		Vector2 firstPos, secondPos;

		/* Finding 2 other required hexagon coordinates on grid */
		FindOtherHexagons(out firstPos, out secondPos);

		/* Searching for other 2 hexagons */
		foreach (Hexagon hex in hexList) {
			if (firstPos.x == hex.GetX() && firstPos.y == hex.GetY()) {
				firstHex = hex;
			}

			else if(secondPos.x == hex.GetX() && secondPos.y == hex.GetY()) {
				secondHex = hex;
			}
		}

		/* Adding selected hexagons in pivot-first-second order to list */
		returnValue.Add(selectedHexagon);
		returnValue.Add(firstHex);
		returnValue.Add(secondHex);



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
		bool status = true;
		gameInitializiationStatus = true;


		/* Create desired number of Hexagons in given grid size */
		for (int i = 0; i<gridHeight; ++i) {
			startVector.x = startPos;
			for (int j = 0; j<gridWidth; ++j) {
				GameObject newObj = Instantiate(hexPrefab, startVector, Quaternion.identity, hexParent.transform);
				Hexagon newHex = newObj.GetComponent<Hexagon>();
				newHex.SetX(j);
				newHex.SetY(i);
				hexList.Add(newHex);

				startVector.x += HEX_DISTANCE_HORIZONTAL;
				yield return new WaitForSeconds(0.02f);
			}
		}


		/* Make sure if all hexagons completed their fall state */
		do {
			status = true;
			foreach (Hexagon hex in hexList)
				if (hex.IsMoving())
					status = false;

			yield return new WaitForSeconds(0.01f);
		} while (!status);


		/* Freeze the grid for better visualasion and indicate the end of initializiation */
		gameInitializiationStatus = false;
		//FreezeGrid();
	}



	/* Freezes all hexagons on game grid */
	private void FreezeGrid() {
		foreach (Hexagon hex in hexList) {
			hex.Freeze();
		}
	}



	/* Unfreezes all the hexagons on game grid */
	private void UnfreezeGrid() {
		foreach (Hexagon hex in hexList) {
			hex.SetFree();
		}
	}



	/* Setters & Getters */
	public void SetGridWidth(int width) { gridWidth = width; }
	public void SetGridHeight(int height) { gridHeight = height; }

	public int GetGridWidth() { return gridWidth; }
	public int GetGridHeiht() { return gridHeight; }
}
