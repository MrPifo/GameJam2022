using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float driveSpeed;
	public float turnSpeed;
	public float turnAngle;
	public Vector3 accDir;
	public Transform camFocusPoint;
	public Transform steerWheel;
	public Transform wheelFR;
	public Transform wheelFL;
	public Rigidbody Motor { get; private set; }
	public Vector3 Position => Motor.position;
	public new Transform transform;
	public Transform mesh;
	private float currentTurnAngle;
	public static Player Input { get; private set; }

	void Awake() {
		Motor = GetComponentInChildren<Rigidbody>();
		Input = ReInput.players.GetPlayer(0);
	}

	private void FixedUpdate() {
		float steerSign = 0;
		if (Input.GetAxis("Steer") < 0) {
			currentTurnAngle -= Time.fixedDeltaTime * turnSpeed;
			steerSign = -1;
		} else if(Input.GetAxis("Steer") > 0) {
			currentTurnAngle += Time.fixedDeltaTime * turnSpeed;
			steerSign = 1;
		}


		accDir = Quaternion.AngleAxis(currentTurnAngle, Vector3.up) * Vector3.forward;
		Motor.AddForce(accDir * driveSpeed);
		
		Quaternion rot = Quaternion.LookRotation(Motor.velocity.normalized, Vector3.up);
		mesh.rotation = rot;
		steerWheel.localRotation = Quaternion.Slerp(steerWheel.localRotation, Quaternion.Euler(48, 0, -steerSign * turnAngle), Time.fixedDeltaTime * 4);

		wheelFR.localRotation = Quaternion.Slerp(wheelFR.localRotation, Quaternion.Euler(0, steerSign * turnAngle, 0), Time.fixedDeltaTime * 4);
		wheelFL.localRotation = Quaternion.Slerp(wheelFL.localRotation, Quaternion.Euler(0, steerSign * turnAngle, 0), Time.fixedDeltaTime * 4);
	}

}
