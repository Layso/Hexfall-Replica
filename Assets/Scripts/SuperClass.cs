using UnityEngine;



public class SuperClass : MonoBehaviour {
	/* Constant variables */
	protected const int ZERO = 0;
	protected const int DOUBLE = 2;
	protected const int HALF = 2;
	protected const int MINIMUM_GRID_WIDTH = 5;
	protected const int DEFAULT_GRID_WIDTH = 7;
	protected const int DEFAULT_GRID_HEIGHT = 8;
	protected const int DEFAULT_COLOR_COUNT = 5;
	protected const int SELECTION_STATUS_COUNT = 6;
	protected const int HEX_ROTATE_SLIDE_DISTANCE = 5;
	protected const int HEXAGON_ROTATE_CONSTANT = 9;
	protected const int SCORE_CONSTANT = 5;
	protected const int RANDOM_SEED = 75486;
	protected const int GRID_VERTICAL_OFFSET = -3;


	protected const float HEX_DISTANCE_HORIZONTAL = 0.445f;
	protected const float HEX_DISTANCE_VERTICAL = 0.23f;
	protected const float HEXAGON_ROTATE_THRESHOLD = 0.05f;
	protected const float DELAY_TO_PRODUCE_HEXAGON = 0.025f;


	protected const string TAG_HEXAGON = "Hexagon";


	protected readonly Vector3 HEX_OUTLINE_SCALE = new Vector3(0.685f, 0.685f, 0.685f);
	protected readonly Vector2 HEX_START_POSITION = new Vector3(0f, 5.5f, 0f);
}
