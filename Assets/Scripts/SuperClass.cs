using UnityEngine;



public class SuperClass : MonoBehaviour {
	/* Constant variables */
	protected const int ZERO = 0;
	protected const int DOUBLE = 2;
	protected const int HALF = 2;


	protected const float HEX_DISTANCE_HORIZONTAL = 0.445f;
	protected const float HEX_DISTANCE_VERTICAL = 0.23f;
	protected const int MINIMUM_GRID_WIDTH = 5;

	protected const int DEFAULT_GRID_WIDTH = 7;
	protected const int DEFAULT_GRID_HEIGHT = 8;
	protected const bool DEFAULT_COLORBLIND_ON = false;
	protected const bool DEFAULT_COLORBLIND_OFF = true;

	protected const string TAG_HEXAGON = "Hexagon";

	protected const int SELECTION_STATUS_COUNT = 6;
	protected const int HEX_ROTATE_SLIDE_DISTANCE = 5;
	protected readonly Vector3 HEX_OUTLINE_SCALE = new Vector3(0.65f, 0.65f, 0.65f);
	protected const float HEXAGON_ROTATE_THRESHHOLD = 0.05f;
	protected const int HEXAGON_ROTATE_CONSTANT = 9;
	protected bool gameInitializiation;
	protected bool hexagonRotation;
}
