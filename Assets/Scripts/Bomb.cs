using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    public float throwDistance;
    public float throwHeight;
    public float explosionRadius;
    private Rigidbody rig;
    private Transform mesh;
    private bool disableCollision;

	private void Awake() {
        rig = GetComponent<Rigidbody>();
        mesh = transform.GetChild(0);
	}

	private void Update() {
        mesh.LookAt(transform.position + rig.velocity.normalized);
        mesh.rotation *= Quaternion.Euler(-90, 0, 0);
	}

	public void Throw(Vector3 origin, Vector3 direction) {
        transform.position = origin;

        rig.AddForce(new Vector3(direction.x * throwDistance, throwHeight, direction.z * throwDistance), ForceMode.VelocityChange);


	}

    public void Explode() {
        Collider[] colls = Physics.OverlapSphere(transform.position, explosionRadius);

        if (colls.Length > 0) {
            foreach(var c in colls) {
                if(c.TryGetComponent(out Obstacle obs)) {
                    obs.DestroyWall(rig.position, rig.velocity.normalized);
				}
			}
        }

        Destroy(gameObject);
    }


	private void OnCollisionEnter(Collision collision) {
        if (collision.rigidbody == null || (collision.rigidbody != null && collision.rigidbody.gameObject.layer != 13)) {
            Explode();
        }
    }
}
