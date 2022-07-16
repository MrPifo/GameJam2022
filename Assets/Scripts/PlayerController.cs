using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Rewired;
using SimpleMan.CoroutineExtensions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	public float driveSpeed;
	public float turnSpeed;
	public float turnAngle;
	public Vector3 accDir;
	public Transform camFocusPoint;
	public Transform steerWheel;
	public Transform wheelFR;
	public Transform wheelFL;
	[Header("Dice Item")]
	public Rigidbody dice;
	public Camera diceCamera;
	public RenderTexture diceRenderTexture;
	public RawImage diceDisplayImage;
	public float diceRollStrength;

	public Rigidbody Motor { get; private set; }
	public Vector3 Position => Motor.position;
	public new Transform transform;
	public Transform mesh;
	private float currentTurnAngle;
	public static Player Input { get; private set; }
	public static Texture2D DiceTexture { get; private set; }

	void Awake() {
		Motor = GetComponentInChildren<Rigidbody>();
		Input = ReInput.players.GetPlayer(0);
		DiceTexture = new Texture2D(diceRenderTexture.width, diceRenderTexture.height, TextureFormat.RGBA32, false);
		dice.gameObject.SetActive(false);
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

		if (Motor.velocity != Vector3.zero) {
			Quaternion rot = Quaternion.LookRotation(Motor.velocity.normalized, Vector3.up);
			mesh.rotation = rot;
		}
		steerWheel.localRotation = Quaternion.Slerp(steerWheel.localRotation, Quaternion.Euler(48, 0, -steerSign * turnAngle), Time.fixedDeltaTime * 4);

		wheelFR.localRotation = Quaternion.Slerp(wheelFR.localRotation, Quaternion.Euler(0, steerSign * turnAngle, 0), Time.fixedDeltaTime * 4);
		wheelFL.localRotation = Quaternion.Slerp(wheelFL.localRotation, Quaternion.Euler(0, steerSign * turnAngle, 0), Time.fixedDeltaTime * 4);
	}

	private void LateUpdate() {
		//ToTexture2D(diceRenderTexture, DiceTexture);
		//diceDisplayImage.texture = ConvertColoredPixelsToTransparent(DiceTexture, DiceTexture.GetPixel(0, 0));
	}

	public void RollItem() {
		dice.gameObject.SetActive(true);
		dice.isKinematic = false;
		dice.useGravity = true;
		dice.transform.localPosition = new Vector3(0, 6, 0);
		dice.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * diceRollStrength, ForceMode.VelocityChange);

		this.Delay(1f, () => {
			Vector3 up = dice.transform.up;
			dice.isKinematic = true;
			dice.useGravity = false;
			dice.transform.DOLocalMove(new Vector3(0, 1f, 0), 0.3f);
			dice.transform.DOLocalRotate(Quaternion.LookRotation((dice.transform.position - diceCamera.transform.position).normalized, up).eulerAngles, 0.6f);
		});
	}

	public static void ToTexture2D(RenderTexture rTex, Texture2D tex) {
		var old_rt = RenderTexture.active;
		RenderTexture.active = rTex;

		tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
		tex.Apply();

		RenderTexture.active = old_rt;
	}
	public static Texture2D ConvertColoredPixelsToTransparent(Texture2D texture, Color backgroundColor) {
		for (int x = 0; x < texture.width; x++) {
			for (int y = 0; y < texture.height; y++) {
				Color color = texture.GetPixel(x, y);
				if (color == backgroundColor) {
					texture.SetPixel(x, y, new Color(0, 0, 0, 0));
				}
			}
		}
		return texture;
	}
}
