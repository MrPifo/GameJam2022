using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTrigger : MonoBehaviour {

	public enum TriggerType { Entrance, Junction }

	public TriggerType type;
	private bool hasBeenTriggered;
	private Road road;

	private void Awake() {
		road = GetComponentInParent<Road>();
	}

	private void OnTriggerEnter(Collider other) {
		if(hasBeenTriggered == false && other.transform.CompareTag("Player")) {
			hasBeenTriggered = true;

			switch (type) {
				case TriggerType.Entrance:
					road.TriggerEntrance();
					break;
				case TriggerType.Junction:
					road.TriggerJunction();
					break;
			}
		}
	}

}
