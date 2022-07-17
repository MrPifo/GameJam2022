using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Coin : MonoBehaviour {

	public int amount;
	public float spinSpeed;
	private Transform mesh;
	private bool hasBeenCollected;

	void Awake() {
		mesh = transform.GetChild(0);
	}

	void Update() {
		if (hasBeenCollected == false) {
			mesh.Rotate(Vector3.forward, Time.deltaTime * spinSpeed);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if(hasBeenCollected == false && other.transform.CompareTag("Player")) {
			Collect();
		}
	}

	public void Collect() {
		hasBeenCollected = true;
		FindObjectOfType<PlayerController>().AddCoin(amount);
		transform.DOMove(transform.position + new Vector3(0, 2.5f, 0), 0.25f).SetEase(Ease.OutQuad);
		transform.DORotate(new Vector3(0, 1000, 0), 0.25f).SetEase(Ease.OutQuad);
	}
}
