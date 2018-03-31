using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : SuperClass {
	GridManager GridManagerObject;
	public int x;
	public int y;
	private Vector2 lerpPosition;
	private bool lerp;



	/* Struct to hold neighboor grid coordinates */
	public struct NeighbourHexes {
		public Vector2 up;
		public Vector2 upLeft;
		public Vector2 upRight;
		public Vector2 down;
		public Vector2 downLeft;
		public Vector2 downRight;
	}



	/* Using awake since Start() will be too late for instantiation */
	void Awake () {
		/* Assigning invalid indexes to indicate unused hexagon */
		lerp = false;
		x = -1;
		y = -1;
		GetComponent<SpriteRenderer>().color = Color.cyan;
	}

	void Start() {
		GridManagerObject = GridManager.instance;
	}

	void Update() {
		if (lerp) {
			float newX = Mathf.Lerp(transform.position.x, lerpPosition.x, Time.deltaTime*HEXAGON_ROTATE_CONSTANT);
			float newY = Mathf.Lerp(transform.position.y, lerpPosition.y, Time.deltaTime*HEXAGON_ROTATE_CONSTANT);
			transform.position = new Vector2(newX, newY);

			if (Vector3.Distance(transform.position, lerpPosition) < HEXAGON_ROTATE_THRESHHOLD) {
				print("ehe");
				transform.position = lerpPosition;
				lerp = false;
			}
		}
	}



	/* Builds a struct from grid position of neighbour hexagons and returns it */
	public NeighbourHexes GetNeighbours() {
		NeighbourHexes neighbours;
		bool onStepper = OnStepper();


		neighbours.down = new Vector2(x, y-1);
		neighbours.up = new Vector2(x, y+1);
		neighbours.upLeft = new Vector2(x-1, onStepper ? y+1 : y);
		neighbours.upRight = new Vector2(x+1, onStepper ? y+1 : y);
		neighbours.downLeft = new Vector2(x-1, onStepper ? y : y-1);
		neighbours.downRight = new Vector2(x+1, onStepper ? y : y-1);


		return neighbours;
	}



	/* Helper function to find out if Hexagon standing on stepper or on base.
	 * Doesn't required new function to operate but needs space to be explained.
	 * First part checks if middle column (index of mid-col (int)(GridWidth/2))
	 * is on an even or odd numbered index. This specifies whether grid starts with
	 * a stepper or without. Second part checks if hexagon is on an even or odd numbered
	 * index. If their indexes are both even or both even, then hexagon is on a stepper 
	 * Implemented using Karnough maps */
	private bool OnStepper() {
		return ((GridManagerObject.GetGridWidth()/HALF)%2 == GetX()%2);
	}



	/* Setters & Getters */
	public void SetX(int value) { x = value; }
	public void SetY(int value) { y = value; }
	public void SetLerpPosition(Vector2 vector) { lerp = true; lerpPosition = vector; }


	public int GetX() { return x; }
	public int GetY() { return y; }
	public Vector2 GetLerpPosition() { return lerpPosition; }
}
