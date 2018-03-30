using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : SuperClass {
	public int x;
	public int y;

	

	/* Using awake since Start() will be too late for instantiation */
	void Awake () {
		/* Assigning invalid indexes to indicate unused hexagon */
		x = -1;
		y = -1;
	}



	/* Setters & Getters */
	public void SetX(int value) { x = value; }
	public void SetY(int value) { y = value; }

	public int GetX() { return x; }
	public int GetY() { return y; }
}
