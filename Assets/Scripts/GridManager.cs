using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : SuperClass {
	public GameObject hexPrefab;



	private int gridWidth;
	private int gridHeight;
	public GameObject hexParent;


	/* One "instance" to rule them all */
	public static GridManager instance = null;



	private void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}


	private void Start() {
		gridWidth = 7;
		gridHeight = 5;
	}


	private void Update()
	{
		/*if (Input.anyKeyDown)
		{
			InitializeGrid();
		}*/
	}

	public void InitializeGrid()
	{
		float startX = gridWidth/2 * HEX_DISTANCE_HORIZONTAL;
		Vector3 startPos = new Vector3(0, 10, 0);
		

		if (hexPrefab != null) {
			StartCoroutine(HexagonProducer(startX, startPos));
		}
	}


	IEnumerator HexagonProducer (float startPos, Vector3 startVector) {
		for (int i = 0; i<gridHeight; ++i)
		{
			startVector.x = startPos;
			for (int j = 0; j<gridWidth; ++j)
			{
				Instantiate(hexPrefab, startVector, Quaternion.identity, hexParent.transform);
				startVector.x -= HEX_DISTANCE_HORIZONTAL;
				yield return new WaitForSeconds(0.02f);
			}
		}
	}


	/* Setters & Getters */
	public int GetGridWidth() { return gridWidth; }
	public int GetGridHeiht() { return gridHeight; }

	public void SetGridWidth(int width) { gridWidth = width; }
	public void SetGridHeight(int height) { gridHeight = height; }
}
