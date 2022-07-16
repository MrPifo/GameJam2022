using System.Collections;
using System.Collections.Generic;
using Redcode.Paths;
using UnityEngine;
using System.Linq;

public class Road : MonoBehaviour {

	public RoadTypes type;
	public List<Connection> PossibleConnections;

	public void Initialize(Vector3 ancor, Vector3 normal) {
		transform.position = ancor;
		transform.forward = -normal;
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
