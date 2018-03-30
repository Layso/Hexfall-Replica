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
	public Sprite hexagonSprite;

	/* Member variables */
	private bool validTouch;
	private Vector2 touchStartPos;
	private GameObject selectedHex;
	private int selectionStatus;
	private int gridWidth;
	private int gridHeight;
	private List<Hexagon> hexList;
	private List<GameObject> outlineList;



	/* Struct to hold neighboor grid coordinates */
	private struct NeighbourHexes {
		public Vector2 up;
		public Vector2 upLeft;
		public Vector2 upRight;
		public Vector2 down;
		public Vector2 downLeft;
		public Vector2 downRight;
	}
	


	/* Assigning singleton if available */
	private void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}

	/* Initializing variables */
	private void Start() {
		hexList = new List<Hexagon>();
		outlineList = new List<GameObject>();
	}

	/* Checking input changes on all frames */
	private void Update() {
		InputManager();
	}



	/* Wrapper function for coroutine. Width and height should be set before this call */
	public void InitializeGrid() {
		float startX = gridWidth/HALF * -HEX_DISTANCE_HORIZONTAL;
		Vector3 startPos = new Vector3(0, 10, 0);
		

		/* Controlling prefab to avoid null pointer exception */
		if (hexPrefab != null) {
			StartCoroutine(InitialHexagonProducer(startX, startPos));
		}
	}



	/* Function to take and analyze input */
	private void InputManager() {
		if (Input.touchCount > ZERO) {
			/* Taking collider of touched object to a variable */
			Vector3 wp = Camera.main.ScreenToWorldPoint(Input.GetTouch(ZERO).position);
			Vector2 touchPos = new Vector2(wp.x, wp.y);
			Collider2D collider = Physics2D.OverlapPoint(touchPos);


			/* If there is a collider and its tag match with any Hexagon continue operate */
			if (collider != null && collider.transform.tag == TAG_HEXAGON) {
				/* Set start poisiton at the beginning of touch[0] */
				if (Input.GetTouch(ZERO).phase == TouchPhase.Began) {
					validTouch = true;
					touchStartPos = Input.GetTouch(ZERO).position;
				}

				/* Calculate distance to decide rotation of hexagons */
				else if (Input.GetTouch(ZERO).phase == TouchPhase.Moved && validTouch) {
					Vector2 touchCurrentPos = Input.GetTouch(ZERO).position;
					float distance;

					if (Mathf.Abs((distance = touchCurrentPos.x - touchStartPos.x)) > HEX_ROTATE_SLIDE_DISTANCE) {
						print("dön bebeğim");
						validTouch = false;
					}

					else if (Mathf.Abs((distance = touchCurrentPos.y - touchStartPos.y)) > HEX_ROTATE_SLIDE_DISTANCE) {
						print("dön bebeğim");
						validTouch = false;
					}
				}

				/* Select new hexagon if touch ended */
				else if (Input.GetTouch(ZERO).phase == TouchPhase.Ended && validTouch) {
					/* If selection is different than current hex, reset status and variable */
					if (selectedHex == null || !selectedHex.GetComponent<Collider2D>().Equals(collider)) {
						selectedHex = collider.gameObject;
						selectionStatus = 0;
					}

					/* Else increase selection status without exceeding total number */
					else {
						selectionStatus = (++selectionStatus) % SELECTION_STATUS_COUNT;
					}

					Select();
					validTouch = false;
				}
			}
		}
	}
	
	
	
	/* Function to select the hex group on touch position */
	private void Select() {
		
		Hexagon hex = selectedHex.GetComponent<Hexagon>();
		


		/* Clear previous outlines */
		if (outParent.transform.childCount > ZERO) {
			foreach (Transform child in outParent.transform)
				Destroy(child.gameObject);
		}

		/* Find hex group to be outlined */
		outlineList = FindHexagonGroup();
		
		/* Creating outlines by creating black hexagons on same position with selected 
		 * hexagons and making them bigger than actual hexagons. AKA fake shader programming 
		 * Yes, I should learn shader programming... 
		 */
		foreach (GameObject go in outlineList) {
			GameObject outline = new GameObject("Outline");
			GameObject outlineInner = new GameObject("Inner Object");

			outline.transform.parent = outParent.transform;
			outlineInner.transform.parent = outParent.transform;

			outline.AddComponent<SpriteRenderer>();
			outline.GetComponent<SpriteRenderer>().sprite = hexagonSprite;
			outline.GetComponent<SpriteRenderer>().color = Color.black;
			outline.transform.position = go.transform.position;
			outline.transform.localScale = HEX_OUTLINE_SCALE;

			outlineInner.AddComponent<SpriteRenderer>();
			outlineInner.GetComponent<SpriteRenderer>().sprite = hexagonSprite;
			outlineInner.GetComponent<SpriteRenderer>().color = go.GetComponent<SpriteRenderer>().color;
			outlineInner.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -1);
			outlineInner.transform.localScale = go.transform.localScale;
		}
	}



	/* Helper function for Select() to find all 3 hexagons to be outlined */
	private List<GameObject> FindHexagonGroup() {
		List<GameObject> returnValue = new List<GameObject>();
		Hexagon selectedHexagon = selectedHex.GetComponent<Hexagon>();
		Vector2 firstPos, secondPos;


		/* Finding 2 other required hexagon coordinates on grid */
		FindOtherHexagons(out firstPos, out secondPos);

		/* Adding selected hexagons to selected list */
		returnValue.Add(selectedHex);
		foreach (Hexagon hex in hexList) {
			if (firstPos.x == hex.GetX() && firstPos.y == hex.GetY()) {
				returnValue.Add(hex.gameObject);
			}

			else if(secondPos.x == hex.GetX() && secondPos.y == hex.GetY()) {
				returnValue.Add(hex.gameObject);
			}
		}


		return returnValue;
	}



	/* Helper function for FindHexagonGroup() to locate two other hexagons */
	private void FindOtherHexagons(out Vector2 first, out Vector2 second) {
		Hexagon selected = selectedHex.GetComponent<Hexagon>();
		NeighbourHexes neighbours = new NeighbourHexes();
		int x = selected.GetX();
		int y = selected.GetY();
		bool onStepper = OnStepper(selected);
		bool breakLoop = false;

		/* Filling neighbours according to hexagons position (onstepper or onbase) */
		neighbours.down = new Vector2(x, y-1);
		neighbours.up = new Vector2(x, y+1);
		neighbours.upLeft = new Vector2(x-1, onStepper ? y+1 : y);
		neighbours.upRight = new Vector2(x+1, onStepper ? y+1 : y);
		neighbours.downLeft = new Vector2(x-1, onStepper ? y : y-1);
		neighbours.downRight = new Vector2(x+1, onStepper ? y : y-1);

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



	/* Function to initialize grid with a delay between each hexagon instantiation */
	private IEnumerator InitialHexagonProducer (float startPos, Vector3 startVector) {
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
	}



	/* Helper function to find out if Hexagon standing on stepper or on base.
	 * Doesn't required new function to operate but needs space to be explained.
	 * First part checks if middle column (index of mid-col (int)(GridWidth/2))
	 * is on an even or odd numbered index. This specifies whether grid starts with
	 * a stepper or without. Second part checks if hexagon is on an even or odd numbered
	 * index. If their indexes are both even or both even, then hexagon is on a stepper */
	private bool OnStepper(Hexagon hex) {
		return ((GetGridWidth()/HALF)%2 == hex.GetX()%2);
	}



	/* Setters & Getters */
	public void SetGridWidth(int width) { gridWidth = width; }
	public void SetGridHeight(int height) { gridHeight = height; }

	public int GetGridWidth() { return gridWidth; }
	public int GetGridHeiht() { return gridHeight; }
}
