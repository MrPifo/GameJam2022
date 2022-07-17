using System.Collections;
using System.Collections.Generic;
using SimpleMan.CoroutineExtensions;
using Sperlich.Extensions;
using UnityEngine;

public class Shell : MonoBehaviour {

	public float travelSpeed;
	public float initHeightForce;
	public float despawnTime;
	public float torque;
	public Vector3 dir;
	private Rigidbody rig;
	private AudioSource diceHit;
	private bool lockHitSound;

	private void Awake() {
		rig = GetComponent<Rigidbody>();
		diceHit = GetComponent<AudioSource>();
	}

	public void Throw(Vector3 from, Vector3 direction) {
		transform.position = from;
		dir = direction;
		rig.AddForce(Vector3.up * initHeightForce);
		rig.AddTorque(new Vector3(Random.Range(0f, 1f) * torque, Random.Range(0f, 1f) * torque, Random.Range(0f, 1f) * torque));

		this.Delay(despawnTime, () => {
			Destroy();
		});
	}

	private void Update() {
		Vector3 vel = dir * travelSpeed;
		rig.velocity = new Vector3(vel.x, rig.velocity.y, vel.z);
	}

	public void Destroy() {
		Destroy(gameObject);
	}

	private void OnCollisionEnter(Collision collision) {
		if (lockHitSound == false) {
			diceHit.Play();
			diceHit.pitch = Random.Range(0.85f, 1.15f);
			lockHitSound = true;

			this.Delay(0.2f, () => lockHitSound = false); {

			}
		}
	}
}
