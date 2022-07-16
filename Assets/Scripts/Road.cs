using System.Collections;
using System.Collections.Generic;
using Redcode.Paths;
using UnityEngine;

public class Road : MonoBehaviour {

	public RoadTypes type;
	public Path startPath;
	public Path path_Right;
	public Path path_Left;

	void OnTriggerEnter(Collider other) {
		
	}
}
