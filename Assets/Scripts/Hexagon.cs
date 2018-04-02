using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : SuperClass {
	GridManager GridManagerObject;
	public int x;
	public int y;
	public Color color;
	public Vector2 lerpPosition;
	public bool lerp;
	public Vector2 speed;


	/* Struct to hold neighboor grid coordinates */
	public struct NeighbourHexes {
		public Vector2 up;
		public Vector2 upLeft;
		public Vector2 upRight;
		public Vector2 down;
		public Vector2 downLeft;
		public Vector2 downRight;
	}



	void Start() {
		GridManagerObject = GridManager.instance;
		lerp = false;
	}

	void Update() {
		if (lerp) {
			float newX = Mathf.Lerp(transform.position.x, lerpPosition.x, Time.deltaTime*HEXAGON_ROTATE_CONSTANT);
			float newY = Mathf.Lerp(transform.position.y, lerpPosition.y, Time.deltaTime*HEXAGON_ROTATE_CONSTANT);
			transform.position = new Vector2(newX, newY);

			
			if (Vector3.Distance(transform.position, lerpPosition) < HEXAGON_ROTATE_THRESHOLD) {
				transform.position = lerpPosition;
				lerp = false;
			}
		}
	}


	/* Function to save rotate changes */
	public void Rotate(int newX, int newY, Vector2 newPos) {
		lerpPosition = newPos;
		SetX(newX);
		SetY(newY);
		lerp = true;
	}



	/* Sets rigidbody constraints to disable any movement */
	public void Freeze() {
		/*GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;*/
	}

	

	/* Sets rigidbody constraints to enable vertical movement */
	public void SetFree() {
		/*GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;*/
	}



	/* Returns the LERP status to indicate the end of rotation */
	public bool IsRotating() {
		return lerp;
	}


	
	public bool IsMoving() {
		return !(GetComponent<Rigidbody2D>().velocity == Vector2.zero);
	}



	public void Exploded() {
		GetComponent<Collider2D>().isTrigger = true;
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



	/* Set new world position for hexagon */
	public void ChangeWorldPosition(Vector2 newPosition) {
		lerpPosition = newPosition;
		lerp = true;
	}



	/* Set new grid position for hexagon */
	public void ChangeGridPosition(Vector2 newPosition) {
		x = (int)newPosition.x;
		y = (int)newPosition.y;
	}



	/* Helper function to find out if Hexagon standing on stepper or on base.
	 * midIndex is the index of middle column of the grid
	 * If index of both middleColumn and current column has same parity then hexagon is on stepper
	 */
	public bool OnStepper() {
		int midIndex = GridManagerObject.GetGridWidth()/HALF;
		return (midIndex%2 == GetX()%2);
	}



	/* Setters & Getters */
	public void SetX(int value) { x = value; }
	public void SetY(int value) { y = value; }
	public void SetColor(Color newColor) { GetComponent<SpriteRenderer>().color = newColor; color=newColor; }

	public int GetX() { return x; }
	public int GetY() { return y; }
	public Color GetColor() { return GetComponent<SpriteRenderer>().color; }
}
