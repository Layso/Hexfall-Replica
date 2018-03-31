using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SuperClass {
	private bool validTouch;
	private GridManager GridManagerObject;
	private Vector2 touchStartPosition;
	private Hexagon selectedHexagon;


	
	void Start () {
		GridManagerObject = GridManager.instance;
	}
	
	void Update () {
		if (GridManagerObject.InputAvailabile() && Input.touchCount > ZERO) {
			/* Taking collider of touched object (if exists) to a variable */
			Vector3 wp = Camera.main.ScreenToWorldPoint(Input.GetTouch(ZERO).position);
			Vector2 touchPos = new Vector2(wp.x, wp.y);
			Collider2D collider = Physics2D.OverlapPoint(touchPos);

			/* Processing input */
			TouchDetection();
			CheckSelection(collider);
			CheckRotation();
		}
	}



	/* Checks if first touch arrived */
	private void TouchDetection() {
		/* Set start poisiton at the beginning of touch[0] */
		if (Input.GetTouch(ZERO).phase == TouchPhase.Began) {
			validTouch = true;
			touchStartPosition = Input.GetTouch(ZERO).position;
		}
	}



	/* Checks if selection condition provided and calls grid manager to handle selection */
	private void CheckSelection(Collider2D collider) {
		/* If there is a collider and its tag match with any Hexagon continue operate */
		if (collider != null && collider.transform.tag == TAG_HEXAGON) {
			/* Select hexagon if touch ended */
			if (Input.GetTouch(ZERO).phase == TouchPhase.Ended && validTouch) {
				validTouch = false;
				selectedHexagon = GridManagerObject.Select(collider);
			}
		}
	}



	/* Checks if rotation condition provided and calls grid manager to handle rotation */
	private void CheckRotation() {
		if (Input.GetTouch(ZERO).phase == TouchPhase.Moved && validTouch) {
			Vector2 touchCurrentPosition = Input.GetTouch(ZERO).position;
			float distanceX = touchCurrentPosition.x - touchStartPosition.x;
			float distanceY = touchCurrentPosition.y - touchStartPosition.y;


			/* Check if rotation triggered by comparing distance between first touch position and current touch position */
			if ((Mathf.Abs(distanceX) > HEX_ROTATE_SLIDE_DISTANCE || Mathf.Abs(distanceY) > HEX_ROTATE_SLIDE_DISTANCE) && selectedHexagon != null) {
				Vector3 screenPosition = Camera.main.WorldToScreenPoint(selectedHexagon.transform.position);

				/* Simplifying long boolean expression thanks to ternary condition
					* triggerOnX specifies if rotate action triggered from a horizontal or vertical swipe 
					* swipeRightUp specifies if swipe direction was right or up
					* touchThanHex specifies if touch position value is bigger than hexagon position on triggered axis
					* If X axis triggered rotation with same direction swipe, then rotate clockwise else rotate counter clockwise
					* If Y axis triggered rotation with opposite direction swipe, then rotate clockwise else rotate counter clocwise
					*/
				bool triggerOnX = Mathf.Abs(distanceX) > Mathf.Abs(distanceY);
				bool swipeRightUp = triggerOnX ? distanceX > ZERO : distanceY > ZERO;
				bool touchThanHex = triggerOnX ? touchCurrentPosition.y > screenPosition.y : touchCurrentPosition.x > screenPosition.x;
				bool clockWise = triggerOnX ? swipeRightUp == touchThanHex : swipeRightUp != touchThanHex;

				validTouch = false;
				GridManagerObject.Rotate(clockWise);
			}
		}
	}
}
