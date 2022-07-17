using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public PlayerController player;
	public float followSpeed;
	public float followDistance;
	public float followHeight;
	public float focusSpeed;
	public float xOffset;
	public bool follow;

	private void Awake() {
		follow = true;
	}

	void LateUpdate() {
		if (follow == false) return;

		Vector3 pos = player.Position;
		pos += (player.Position - player.transform.position).normalized * -followDistance;
		pos += new Vector3((player.camFocusPoint.position - transform.position).normalized.x * xOffset, followHeight, 0);

		transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * followSpeed);
		Quaternion focusRot = Quaternion.LookRotation((player.camFocusPoint.position - transform.position).normalized, Vector3.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, focusRot, Time.deltaTime * focusSpeed);
	}

}
