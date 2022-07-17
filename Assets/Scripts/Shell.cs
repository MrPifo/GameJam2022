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

	public void Throw(Vector3 from, Vector3 direction) {
		rig = GetComponent<Rigidbody>();
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
}
