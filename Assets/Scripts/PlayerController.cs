using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Rewired;
using SimpleMan.CoroutineExtensions;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using Sperlich.Extensions;

public class PlayerController : MonoBehaviour {

	public enum CubeFace { Face_1, Face_2, Face_3, Face_4, Face_5, Face_6}

	public CubeFace Item;
	public float acceleration;
	public float maxSpeed;
	public float turnSpeed;
	public float turnAngle;
	public float explosionForce;
	public float forceDown;
	[Range(0f, 1f)]
	public float motorVolume;
	public Vector3 accDir;
	public Vector3 massCenter;
	public Transform camFocusPoint;
	public Transform steerWheel;
	public Transform wheelFR;
	public Transform wheelFL;
	public Transform meshContainer;
	public Material ghostMaterial;
	public Material boostPowerupMaterial;
	public Material goldEffect;
	public GameObject endScreen;
	public Shell shellPrefab;
	public Text coinCounter;
	public AudioSource idleMotorSound;
	public AudioSource activeMotorSound;
	public Bomb bombPrefab;
	public List<WheelCollider> wheels;
	public List<WheelCollider> frontWheels;
	public new CameraController camera;
	[Header("Dice Item")]
	public Rigidbody dice;
	public Camera diceCamera;
	public RenderTexture diceRenderTexture;
	public RawImage diceDisplayImage;
	public float diceRollStrength;
	public bool hasItem;
	public bool isInvincible;
	public bool lockItemCollection;

	[Header("Particles")]
	public GameObject BigExplosion;

	[Header("Item Properties")]
	public float boosterDuration;
	public float bigDuration;
	public float ghostDuration;
	public int shellAmount;
	public float shellThrowDelay;
	public int coinGainAmount;
	public float coinIntervall;

	public Rigidbody Motor { get; private set; }
	public Vector3 Position => Motor.position;
	public new Transform transform;
	public Transform mesh;
	private float currentTurnAngle;
	public float SpeedMS => Motor.velocity.magnitude;
	public int coins { get; private set; }
	private SphereCollider sphereCollider;
	public static Player Input { get; private set; }
	public static Texture2D DiceTexture { get; private set; }

	void Awake() {
		Motor = GetComponentInChildren<Rigidbody>();
		Input = ReInput.players.GetPlayer(0);
		sphereCollider = Motor.GetComponent<SphereCollider>();
		DiceTexture = new Texture2D(diceRenderTexture.width, diceRenderTexture.height, TextureFormat.RGBA32, false);
		dice.gameObject.SetActive(false);
		endScreen.SetActive(false);

		activeMotorSound.Play();
		idleMotorSound.Play();
		coins = 0;
		coinCounter.text = 0 + "";
		Motor.centerOfMass = massCenter;
		ghostMaterial.SetFloat("_Fade", 0);
		boostPowerupMaterial.SetFloat("_Fade", 0);
		goldEffect.SetFloat("_Fade", 0);

		FindObjectsOfType<WheelCollider>().ToList().ForEach(w => {
			w.ConfigureVehicleSubsteps(5, 12, 15);
		});
	}

	private void FixedUpdate() {
		if (Motor.isKinematic) return;

		float steerSign = 0;
		if (Input.GetAxis("Steer") < 0) {
			currentTurnAngle -= Time.fixedDeltaTime * turnSpeed;
			steerSign = -1;
		} else if(Input.GetAxis("Steer") > 0) {
			currentTurnAngle += Time.fixedDeltaTime * turnSpeed;
			steerSign = 1;
		}

		if(Vector3.Angle(Vector3.up, transform.up) > 120 || transform.position.y < 0.1f) {
			Die();
		}
		if(Physics.Raycast(transform.position, transform.forward, 4) && Motor.velocity.magnitude < 3) {
			Die();
		}

		accDir = Quaternion.AngleAxis(currentTurnAngle, Vector3.up) * Vector3.forward;
		//Motor.AddForce(accDir * driveSpeed);

		if (Motor.velocity != Vector3.zero && Motor.velocity.magnitude > 1) {
			//Quaternion rot = Quaternion.LookRotation(Motor.velocity.normalized, Vector3.up);
			//mesh.rotation = rot;
		}

		foreach(var w in wheels) {
			if(Motor.velocity.magnitude > maxSpeed) {
				w.motorTorque = 0;
			} else {
				w.motorTorque = acceleration;
			}
			w.GetWorldPose(out Vector3 wPos, out Quaternion qRot);
			Transform mesh = w.transform.GetChild(0);
			mesh.position = wPos;
			mesh.rotation = qRot;
		}

		foreach(var w in frontWheels) {
			w.steerAngle = Mathf.Lerp(w.steerAngle, steerSign * turnAngle, Time.fixedDeltaTime * 4f);
		}

		steerWheel.localRotation = Quaternion.Slerp(steerWheel.localRotation, Quaternion.Euler(48, 0, -steerSign * turnAngle), Time.fixedDeltaTime * 4);
		Motor.AddForce(Vector3.down * forceDown);

		//wheelFR.localRotation = Quaternion.Slerp(wheelFR.localRotation, Quaternion.Euler(0, steerSign * turnAngle, 0), Time.fixedDeltaTime * 4);
		//wheelFL.localRotation = Quaternion.Slerp(wheelFL.localRotation, Quaternion.Euler(0, steerSign * turnAngle, 0), Time.fixedDeltaTime * 4);
	}

	private void Update() {
		if (Motor.isKinematic) return;

		if(Input.GetButtonDown("Use") && hasItem) {
			UseItem();
		}
		if (lockItemCollection == false) {
			if (UnityEngine.Input.GetKey(KeyCode.F1)) {
				Item = CubeFace.Face_1;
				UseItem();
			}
			if (UnityEngine.Input.GetKey(KeyCode.F2)) {
				Item = CubeFace.Face_2;
				UseItem();
			}
			if (UnityEngine.Input.GetKey(KeyCode.F3)) {
				Item = CubeFace.Face_3;
				UseItem();
			}
			if (UnityEngine.Input.GetKey(KeyCode.F4)) {
				Item = CubeFace.Face_4;
				UseItem();
			}
			if (UnityEngine.Input.GetKey(KeyCode.F5)) {
				Item = CubeFace.Face_5;
				UseItem();
			}
			if (UnityEngine.Input.GetKey(KeyCode.F6)) {
				Item = CubeFace.Face_6;
				UseItem();
			}
		}
		MotorAudio();

		if (sphereCollider != null && Physics.Raycast(Motor.position, Vector3.down, out RaycastHit hit, sphereCollider.radius + 0.1f)) {
			if(hit.transform.gameObject.layer == 11) {
				Die();
			}
		}
	}

	public void UseItem() {
		switch (Item) {
			case CubeFace.Face_1:
				Booster();
				break;
			case CubeFace.Face_2:
				Bomb();
				break;
			case CubeFace.Face_3:
				Ghost();
				break;
			case CubeFace.Face_4:
				Bigger();
				break;
			case CubeFace.Face_5:
				Shell();
				break;
			case CubeFace.Face_6:
				FreeCoins();
				break;
		}

		hasItem = false;
		dice.transform.localScale = Vector3.one * 2;
		dice.transform.DOScale(0f, 0.25f).SetEase(Ease.OutElastic);
	}

	void Booster() {
		StartCoroutine(IStart());

		IEnumerator IStart() {
			float ogDriveSpeed = acceleration;
			float ogTurnSpeed = turnSpeed;
			float ogMaxSpeed = maxSpeed;
			acceleration *= 1.5f;
			turnSpeed *= 2;
			maxSpeed *= 1.2f;
			lockItemCollection = true;
			isInvincible = true;
			boostPowerupMaterial.SetFloat("_Fade", 1f);

			yield return new WaitForSeconds(boosterDuration);
			boostPowerupMaterial.SetFloat("_Fade", 0f);
			lockItemCollection = false;
			maxSpeed = ogMaxSpeed;
			acceleration = ogDriveSpeed;
			turnSpeed = ogTurnSpeed;
			isInvincible = false;
		}
	}

	void Bomb() {
		StartCoroutine(IStart());

		IEnumerator IStart() {
			Bomb bomb = Instantiate(bombPrefab.gameObject).GetComponent<Bomb>();
			Vector3 dir = Motor.velocity.normalized;
			bomb.Throw(transform.position + Vector3.up * 2 + dir * 4, Motor.velocity.normalized);
			lockItemCollection = true;

			yield return new WaitForSeconds(1f);
			lockItemCollection = false;
		}
	}

	void Ghost() {
		StartCoroutine(IStart());

		IEnumerator IStart() {
			FindObjectsOfType<Obstacle>().ToList().ForEach(o => o.DisableCollision());
			lockItemCollection = true;
			isInvincible = true;
			DOTween.To(() => ghostMaterial.GetFloat("_Fade"), x => ghostMaterial.SetFloat("_Fade", x), 0.7f, 0.35f);

			yield return new WaitForSeconds(ghostDuration);
			DOTween.To(() => ghostMaterial.GetFloat("_Fade"), x => ghostMaterial.SetFloat("_Fade", x), 0f, 0.35f);
			FindObjectsOfType<Obstacle>().ToList().ForEach(o => o.EnableCollision());
			lockItemCollection = false;
			isInvincible = false;
		}
	}

	void Bigger() {
		StartCoroutine(IStart());

		IEnumerator IStart() {
			float transitionSpeed = 0.4f;
			transform.DOScale(2f, transitionSpeed).SetEase(Ease.OutBounce);
			DOTween.To(() => sphereCollider.radius, x => sphereCollider.radius = x, 3f, transitionSpeed).SetEase(Ease.OutBounce);
			DOTween.To(() => camera.followDistance, x => camera.followDistance = x, 8f, transitionSpeed);
			DOTween.To(() => camera.followHeight, x => camera.followHeight = x, 5f, transitionSpeed);
			lockItemCollection = true;
			isInvincible = true;
			AudioPlayer.Play("Grow");
			foreach(var w in wheels) {
				DOTween.To(() => w.radius, x => w.radius = x, 0.7f, transitionSpeed);
			}

			yield return new WaitForSeconds(bigDuration);
			lockItemCollection = false;
			isInvincible = false;
			transform.DOScale(1f, transitionSpeed).SetEase(Ease.OutBounce);
			DOTween.To(() => sphereCollider.radius, x => sphereCollider.radius = x, 3f, transitionSpeed).SetEase(Ease.OutBounce);
			DOTween.To(() => camera.followDistance, x => camera.followDistance = x, 3f, transitionSpeed);
			DOTween.To(() => camera.followHeight, x => camera.followHeight = x, 3f, transitionSpeed);
			foreach (var w in wheels) {
				DOTween.To(() => w.radius, x => w.radius = x, 0.35f, transitionSpeed);
			}
		}
	}

	void Shell() {
		StartCoroutine(IStart());

		IEnumerator IStart() {
			lockItemCollection = true;
			for (int i = 0; i < shellAmount; i++) {
				Shell shell = Instantiate(shellPrefab.gameObject).GetComponent<Shell>();
				Vector3 dir = Motor.velocity.normalized;
				shell.Throw(transform.position + Vector3.up + dir * 4, dir);
				yield return new WaitForSeconds(shellThrowDelay);
			}
			

			yield return new WaitForSeconds(1f);
			lockItemCollection = false;
		}
	}

	void FreeCoins() {
		StartCoroutine(IStart());

		IEnumerator IStart() {
			int amount = 0;
			lockItemCollection = true;
			isInvincible = true;
			DOTween.To(() => goldEffect.GetFloat("_Fade"), x => goldEffect.SetFloat("_Fade", x), 1, 0.35f).SetEase(Ease.OutBounce);

			while (amount < coinGainAmount) {
				AddCoin(1, ((float)amount).Remap(0, coinGainAmount, 0.7f, 1.3f));
				amount++;
				yield return new WaitForSeconds(coinIntervall);
			}

			isInvincible = false;
			lockItemCollection = false;
			DOTween.To(() => goldEffect.GetFloat("_Fade"), x => goldEffect.SetFloat("_Fade", x), 0f, 0.35f).SetEase(Ease.OutBounce);
		}
	}

	void MotorAudio() {
		idleMotorSound.volume = Mathf.Clamp(SpeedMS, 0, 40).Remap(0f, 20, 1f, 0f) * motorVolume;
		activeMotorSound.volume = Mathf.Clamp(SpeedMS, 20, 40).Remap(0f, 40f, 0f, 1f) * motorVolume;
		activeMotorSound.pitch = Mathf.Clamp(SpeedMS, 30, 100).Remap(30f, 100f, 0.8f, 1.3f);
	}

	public void Die() {
		if (Motor.isKinematic == false) {
			Motor.isKinematic = true;
			Motor.useGravity = false;
			camera.follow = false;

			foreach (Transform t in meshContainer) {
				if (t.gameObject.TryGetComponent(out Rigidbody rig) == false) {
					rig = t.gameObject.AddComponent<Rigidbody>();
				}

				if (t.gameObject.GetComponent<Collider>() == null) {
					BoxCollider coll = t.gameObject.AddComponent<BoxCollider>();
				}

				if (rig != null) {
					rig.AddForce((Vector3.up + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f))) * explosionForce);
				}
			}

			activeMotorSound.Stop();
			idleMotorSound.Stop();
			endScreen.SetActive(true);
			BigExplosion.Spawn(transform.position, 2);
			AudioPlayer.Play("Car_Crash", 1f, 1f, 0.2f);
		}
	}

	public bool RollItem() {
		AudioPlayer.Play("Item_Collect", 0.9f, 1.2f, 0.8f);
		if (lockItemCollection == false && hasItem == false) {
			dice.gameObject.SetActive(true);
			dice.isKinematic = false;
			dice.useGravity = true;
			dice.transform.localPosition = new Vector3(0, 6, 0);
			dice.transform.localScale = Vector3.one;
			Vector3 before = dice.transform.up;
			dice.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * diceRollStrength, ForceMode.VelocityChange);

			this.Delay(0.5f, () => {
				AudioPlayer.Play("Dice_Roll", 0.9f, 1.1f, 0.6f);
			});
			this.Delay(1.5f, () => {
				Vector3 after = -dice.transform.forward;
				dice.isKinematic = true;
				dice.useGravity = false;

				dice.transform.DOLocalMove(new Vector3(0, 1f, 0), 0.3f);

				List<(CubeFace, float, Vector3)> faces = new List<(CubeFace, float, Vector3)>();
				faces.Add((CubeFace.Face_5, Vector3.Angle(dice.transform.up, Vector3.up), dice.transform.up));
				faces.Add((CubeFace.Face_6, Vector3.Angle(-dice.transform.up, Vector3.up), -dice.transform.up));
				faces.Add((CubeFace.Face_4, Vector3.Angle(dice.transform.right, Vector3.up), dice.transform.right));
				faces.Add((CubeFace.Face_2, Vector3.Angle(-dice.transform.right, Vector3.up), -dice.transform.right));
				faces.Add((CubeFace.Face_1, Vector3.Angle(dice.transform.forward, Vector3.up), dice.transform.forward));
				faces.Add((CubeFace.Face_3, Vector3.Angle(-dice.transform.forward, Vector3.up), -dice.transform.forward));

				var face = faces.Where(f => f.Item2 == faces.Min(f => f.Item2)).First();
				Item = face.Item1;

				Quaternion rot = Quaternion.LookRotation((diceCamera.transform.position - dice.transform.position).normalized, Vector3.up);
				switch (face.Item1) {
					case CubeFace.Face_1:
						rot *= Quaternion.Euler(0, 0, 0);
						break;
					case CubeFace.Face_2:
						rot *= Quaternion.Euler(90, 0, -90);
						break;
					case CubeFace.Face_3:
						rot *= Quaternion.Euler(180, 0, 0);
						break;
					case CubeFace.Face_4:
						rot *= Quaternion.Euler(90, 0, 90);
						break;
					case CubeFace.Face_5:
						rot *= Quaternion.Euler(90, 0, 0);
						break;
					case CubeFace.Face_6:
						rot *= Quaternion.Euler(-90, 0, 0);
						break;
				}
				dice.transform.DORotateQuaternion(rot, 0.3f).SetEase(Ease.OutBounce);
				dice.transform.DOScale(1.7f, 0.3f).SetEase(Ease.OutQuad);
				hasItem = true;
			});
			return true;
		}
		return false;
	}

	public void Respawn() {
		SceneManager.LoadScene("Main");
	}

	public void AddCoin(int amount, float pitch = 0) {
		coins += amount;
		coinCounter.text = coins + "";

		if (pitch == 0) {
			AudioPlayer.Play("Coin_Collect", 0.8f, 1.2f, 0.6f);
		} else {
			AudioPlayer.Play("Coin_Collect", pitch, pitch, 0.6f);
		}
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
