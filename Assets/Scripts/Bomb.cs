using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SimpleMan.CoroutineExtensions;
using Sperlich.Extensions;
using UnityEngine;

public class Bomb : MonoBehaviour {

    public float throwDistance;
    public float throwHeight;
    public float explosionRadius;
    public GameObject bigExplosion;
    private Rigidbody rig;
    private AudioSource explodeSound;
    private Transform mesh;
    private bool disableCollision;

	private void Awake() {
        rig = GetComponent<Rigidbody>();
        explodeSound = GetComponent<AudioSource>();
        mesh = transform.GetChild(0);

        mesh.localScale = Vector3.zero;
        mesh.DOScale(1, 1f).SetEase(Ease.OutCirc);
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
        if (disableCollision == false) {
            disableCollision = true;
            bigExplosion.Spawn(transform.position, 2);
            Collider[] colls = Physics.OverlapSphere(transform.position, explosionRadius);

            if (colls.Length > 0) {
                foreach (var c in colls) {
                    if (c.TryGetComponent(out Obstacle obs)) {
                        obs.DestroyWall(rig.position, rig.velocity.normalized);
                    }
                }
            }

            explodeSound.Play();
            transform.GetChild(0).gameObject.SetActive(false);

            this.Delay(2f, () => {
                Destroy(gameObject);
            });
        }
    }


	private void OnCollisionEnter(Collision collision) {
        if (collision.rigidbody == null || (collision.rigidbody != null && collision.rigidbody.gameObject.layer != 13)) {
            Explode();
        }
    }
}
