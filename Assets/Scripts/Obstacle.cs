using System.Collections;
using System.Collections.Generic;
using SimpleMan.CoroutineExtensions;
using Sperlich.Extensions;
using UnityEngine;

public class Obstacle : MonoBehaviour {

	public float explosionForce;
	public float despawnTime;
	public bool hasBeenHit;
	public bool canBeHit;
	private PlayerController player;
	private new Collider collider;
	private List<(Rigidbody, Collider)> bricks = new List<(Rigidbody, Collider)>();

	private void Awake() {
		player = FindObjectOfType<PlayerController>();
		collider = GetComponent<Collider>();
		foreach(Transform t in transform.GetChild(0)) {
			bricks.Add((t.GetComponent<Rigidbody>(), t.GetComponent<Collider>()));
		}
		canBeHit = true;
	}

	private void OnTriggerEnter(Collider other) {
		if (hasBeenHit == false && canBeHit) {
			if (other.gameObject.layer == 9 || other.gameObject.layer == 3) {
				if (player.isInvincible == false) {
					FindObjectOfType<PlayerController>().Die();
				} else {
					DestroyWall(other.attachedRigidbody.position, other.attachedRigidbody.velocity.normalized);
				}
			}
			if (other.transform.TrySearchComponent(out Bomb bomb)) {
				bomb.Explode();
				DestroyWall(other.attachedRigidbody.position, other.attachedRigidbody.velocity.normalized);
			}
			if (other.transform.TrySearchComponent(out Shell shell)) {
				shell.Destroy();
				DestroyWall(other.attachedRigidbody.position, other.attachedRigidbody.velocity.normalized);
			}
		}
	}

	public void DisableCollision() {
		collider.enabled = false;
		canBeHit = false;
	}

	public void EnableCollision() {
		collider.enabled = true;
		canBeHit = true;
	}

	public void DestroyWall(Vector3 origin, Vector3 explodeDir) {
		if (hasBeenHit == false) {
			hasBeenHit = true;
			GetComponent<AudioSource>().Play();

			foreach((Rigidbody, Collider) brick in bricks) {
				brick.Item2.enabled = true;
				brick.Item1.useGravity = true;
				brick.Item1.isKinematic = false;
				brick.Item1.AddExplosionForce(explosionForce, origin, 20);
			}

			this.Delay(despawnTime, () => {
				Destroy(gameObject);
			});
		}
	}
}
