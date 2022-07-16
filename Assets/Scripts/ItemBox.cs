using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour {

	public Transform dice;
	public float boxRotSpeed;
	public float diceDirChangeSpeed;
	public float diceRotSpeed;

	private void Update() {
		dice.Rotate(new Vector3(Mathf.Sin(Time.time * diceDirChangeSpeed), 1, Mathf.Cos(Time.time * diceDirChangeSpeed)), diceRotSpeed * Time.deltaTime);
		transform.Rotate(new Vector3(Mathf.Cos(Time.time * diceDirChangeSpeed), 1, Mathf.Sin(Time.time * diceDirChangeSpeed)), boxRotSpeed * Time.deltaTime);
	}

	public void OnTriggerEnter(Collider other) {
		if(other.transform.CompareTag("Player")) {

		}
	}

}
