using System.Collections;
using System.Collections.Generic;
using SimpleMan.CoroutineExtensions;
using UnityEngine;

public class ItemBox : MonoBehaviour {

	public Transform dice;
	public MeshRenderer box;
	public float boxRotSpeed;
	public float diceDirChangeSpeed;
	public float diceRotSpeed;
	public float explodeForce;
	public bool hasBeenCollected;

	private void Update() {
		if (hasBeenCollected == false) {
			dice.Rotate(new Vector3(Mathf.Sin(Time.time * diceDirChangeSpeed), 1, Mathf.Cos(Time.time * diceDirChangeSpeed)), diceRotSpeed * Time.deltaTime);
			transform.Rotate(new Vector3(Mathf.Cos(Time.time * diceDirChangeSpeed), 1, Mathf.Sin(Time.time * diceDirChangeSpeed)), boxRotSpeed * Time.deltaTime);
		}
	}

	public void OnTriggerEnter(Collider other) {
		if(other.transform.CompareTag("Player")) {
			hasBeenCollected = true;
			box.enabled = false;
			FindObjectOfType<PlayerController>().RollItem();
			var drig = dice.gameObject.AddComponent<Rigidbody>();
			drig.AddForce(Vector3.up * explodeForce, ForceMode.Force);

			this.Delay(1f, () => {
				dice.gameObject.SetActive(false);
			});
		}
	}

}
