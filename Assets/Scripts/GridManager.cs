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
	private int gridWidth;
	private int gridHeight;
	private int selectionStatus;
	private bool validTouch;
	private Vector2 touchStartPos;
	private Hexagon selectedHexagon;
	private List<Hexagon> hexList;
	private List<Hexagon> outlineList;

	





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
		outlineList = new List<Hexagon>();
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


			/* Set start poisiton at the beginning of touch[0] */
			if (Input.GetTouch(ZERO).phase == TouchPhase.Began) {
				validTouch = true;
				touchStartPos = Input.GetTouch(ZERO).position;
			}

			/* If there is a collider and its tag match with any Hexagon continue operate */
			else if (collider != null && collider.transform.tag == TAG_HEXAGON) {
				/* Select hexagon if touch ended */
				if (Input.GetTouch(ZERO).phase == TouchPhase.Ended && validTouch) {
					/* If selection is different than current hex, reset status and variable */
					if (selectedHexagon == null || !selectedHexagon.GetComponent<Collider2D>().Equals(collider)) {
						selectedHexagon = collider.gameObject.GetComponent<Hexagon>();
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

			/* Calculate distance to decide rotation of hexagons */
			else if (Input.GetTouch(ZERO).phase == TouchPhase.Moved && validTouch) {
				Vector2 touchCurrentPos = Input.GetTouch(ZERO).position;
				float distanceX = touchCurrentPos.x - touchStartPos.x;
				float distanceY = touchCurrentPos.y - touchStartPos.y;


				if ((Mathf.Abs(distanceX) > HEX_ROTATE_SLIDE_DISTANCE || Mathf.Abs(distanceY) > HEX_ROTATE_SLIDE_DISTANCE) && selectedHexagon != null) {
					Vector3 screenPosition = Camera.main.WorldToScreenPoint(selectedHexagon.transform.position);

					/* Simplifying long boolean expression thanks to ternary condition
					 * triggerOnX specifies if rotate action triggered from a horizontal or vertical swipe 
					 * swipeRightUp specifies if swipe direction was right or up
					 * touchThanHex specifies if touch position value is bigger than hexagon position on triggered axis
					 * If X axis triggered rotation with same direction swipe then rotate clockwise else rotate counter clockwise
					 * If Y axis triggered rotation with different direction swipe then rotate clockwise else rotate counter clocwise
					 */
					bool triggerOnX = Mathf.Abs(distanceX) > Mathf.Abs(distanceY);
					bool swipeRightUp = triggerOnX ? distanceX > ZERO : distanceY > ZERO;
					bool touchThanHex = triggerOnX ? touchCurrentPos.y > screenPosition.y : touchCurrentPos.x > screenPosition.x;
					bool clockWise = triggerOnX ? swipeRightUp == touchThanHex : swipeRightUp != touchThanHex;

					Rotate(clockWise);
					validTouch = false;
				}
			}
		}
	}
	
	
	
	/* Function to rotate the hex group on touch position */
	private void Rotate(bool clockWise) {
		/* Clear previous outlines */
		DestructOutline();

		foreach(Hexagon hex in hexList) {
			hex.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
		}

		if (!clockWise) {
			outlineList[0].SetLerpPosition(outlineList[2].transform.position);
			outlineList[1].SetLerpPosition(outlineList[0].transform.position);
			outlineList[2].SetLerpPosition(outlineList[1].transform.position);
		}

		else {
			outlineList[0].SetLerpPosition(outlineList[1].transform.position);
			outlineList[1].SetLerpPosition(outlineList[2].transform.position);
			outlineList[2].SetLerpPosition(outlineList[0].transform.position); 
		}
	}



	/* Helper function to Rotate() for rotation animation */
	private IEnumerator RotationCoroutine() {
		for(int i=0; i<outParent.transform.childCount; ++i) {
			print(outParent.transform.GetChild(i).gameObject.transform.position);
		}
		yield return new WaitForSeconds(0.01f);
		
	}



	/* Function to select the hex group on touch position */
	private void Select() {
		/* Clear previous outlines */
		DestructOutline();

		/* Find hex group to be outlined */
		outlineList = FindHexagonGroup();

		/* Build outline around hex group */
		ConstructOutline();
	}



	/* Helper function for Select() to find all 3 hexagons to be outlined */
	private List<Hexagon> FindHexagonGroup() {
		List<Hexagon> returnValue = new List<Hexagon>();
		Vector2 firstPos, secondPos;


		/* Finding 2 other required hexagon coordinates on grid */
		FindOtherHexagons(out firstPos, out secondPos);

		/* Adding selected hexagons to selected list */
		returnValue.Add(selectedHexagon);
		foreach (Hexagon hex in hexList) {
			if (firstPos.x == hex.GetX() && firstPos.y == hex.GetY()) {
				returnValue.Add(hex.GetComponent<Hexagon>());
			}

			else if(secondPos.x == hex.GetX() && secondPos.y == hex.GetY()) {
				returnValue.Add(hex.GetComponent<Hexagon>());
			}
		}


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
			outline.GetComponent<SpriteRenderer>().sprite = hexagonSprite;
			outline.GetComponent<SpriteRenderer>().color = Color.black;
			outline.transform.position = go.transform.position;
			outline.transform.localScale = HEX_OUTLINE_SCALE;

			outlineInner.AddComponent<SpriteRenderer>();
			outlineInner.GetComponent<SpriteRenderer>().sprite = hexagonSprite;
			outlineInner.GetComponent<SpriteRenderer>().color = go.GetComponent<SpriteRenderer>().color;
			outlineInner.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -1);
			outlineInner.transform.localScale = go.transform.localScale;
			outlineInner.transform.parent = outline.transform;
		}
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



	/* Setters & Getters */
	public void SetGridWidth(int width) { gridWidth = width; }
	public void SetGridHeight(int height) { gridHeight = height; }

	public int GetGridWidth() { return gridWidth; }
	public int GetGridHeiht() { return gridHeight; }
}
