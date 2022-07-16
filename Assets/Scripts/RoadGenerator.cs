using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoadGenerator : MonoBehaviour {

	public int generateAhead;
	public int despawnAt;
	public PlayerController player;
	// Roads that have no connection yet
	public Road playerOnRoad;
	public List<Road> roads = new List<Road>();
	public List<Road> roadPrefabs = new List<Road>();
	public static RoadGenerator Instance { get; private set; }

	void Awake() {
		Instance = this;
		roads.Add(FindObjectOfType<Road>());
		roadPrefabs = Resources.LoadAll<Road>("Roads").ToList();
	}

	void Update() {
		
	}

	public void EvaluateNext(Road connector, int iteration = 0) {
		if (iteration < generateAhead) {
			var junctions = connector.PossibleConnections.GroupBy(r => r.ancor);

			for (int i = 0; i < junctions.Count(); i++) {
				var connections = junctions.ToArray()[i].ToList();
				int rand = Random.Range(0, connections.Count - 1);
				Road.Connection conn = connections[rand];
				if (connections.All(c => c.connected == null)) {
					Road road = GenerateRoad(conn.roadType, conn.ancor.position, conn.ancor.forward);
					conn.connected = road;
					EvaluateNext(road, iteration + 1);
				}
			}

			if (roads.Count > despawnAt) {
				Destroy(roads[0].gameObject);
				roads.RemoveAt(0);
			}
		}
	}

	public Road GenerateRoad(RoadTypes nextType, Vector3 ancor, Vector3 normal) {
		Road road = Instantiate(roadPrefabs.Find(r => r.type == nextType), transform).GetComponent<Road>();
		road.Initialize(ancor, normal);
		roads.Add(road);

		return road;
	}
}
