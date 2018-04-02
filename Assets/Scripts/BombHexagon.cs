using UnityEngine;

public class BombHexagon : Hexagon {
	public TextMesh output;
	private int clock;


	public void Tick() { --clock; output.text = clock.ToString(); }
	public int GetClock() { return clock; }
	public void SetClock(int value) { clock = value; output.text = clock.ToString(); }
}
