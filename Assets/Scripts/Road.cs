using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Road : MonoBehaviour {

	public RoadTypes type;
	public List<ItemBox> items;
	public GameObject presetContainer;
	public List<Connection> PossibleConnections;

	private void Awake() {
		items = GetComponentsInChildren<ItemBox>().ToList();
		foreach(Transform t in presetContainer.transform) {
			t.gameObject.SetActive(false);
		}
	}

	public void Initialize(Vector3 ancor, Vector3 normal) {
		transform.position = ancor;
		transform.forward = -normal;

		if (presetContainer.transform.childCount > 0) { //1
			int rand = Random.Range(0, presetContainer.transform.childCount - 1);
			presetContainer.transform.GetChild(rand).gameObject.SetActive(true);
		}
	}

	void RemoveObstacles() {
		foreach(var item in items) {
			Destroy(item.gameObject);
		}
	}

	public void TriggerEntrance() {
		RoadGenerator.Instance.playerOnRoad = this;
		RoadGenerator.Instance.EvaluateNext(this);
	}

	public void TriggerJunction() {
		RoadGenerator.Instance.EvaluateNext(this);
	}

	private void OnDrawGizmosSelected() {
		if(PossibleConnections != null && PossibleConnections.Count > 0) {
			foreach(var con in PossibleConnections.Select(r => r.ancor).Distinct()) {
				if (con != null) {
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(con.position, 2f);
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(con.position, con.position + con.forward * 8);
				}
			}
		}
	}

	[System.Serializable]
	public class Connection {
		public RoadTypes roadType;
		public Transform ancor;
		public Road connected;
	}
}
