using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public LODInfo[] detailLevels;
	public static float maxViewDst;
	public static Vector4 noWalls = new Vector4(2,2,2,2);

	public Transform viewer;
	public static Material mapMaterial;

	public static Vector2 viewerPosition;
	Vector2 viewerPositionOld;
	static MapGenerator mapGenerator;
	static int chunkSize;
	int chunksVisibleInViewDst;

	public Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	public static Dictionary<Vector2, TerrainWall> terrainWallDictionary = new Dictionary<Vector2, TerrainWall>();
	public static List<Vector2> chunkCoordList = new List<Vector2>();
	public static List<Vector2> wallCoordList = new List<Vector2>();

	static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	public Chunks[] chunks;

	void Start() {
		mapGenerator = FindObjectOfType<MapGenerator> ();

		for (int i = 0; i < chunks.Length; i++ ) {
			chunkCoordList.Add(chunks[i].position);
		}

		maxViewDst = detailLevels[detailLevels.Length-1].visibleDstThreshold;
		chunkSize = MapGenerator.mapChunkSize - 1;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

		UpdateVisibleChunks();
	}

	void Update() {
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);

		if ((viewerPositionOld-viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}
		
	public void UpdateVisibleChunks() {

		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate [i].SetVisible(false);
		}
		terrainChunksVisibleLastUpdate.Clear ();
			
		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

		for (int i = 0; i < chunkCoordList.Count; i++) {
			Vector2 viewedChunkCoord = chunkCoordList[i];

			if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
				terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
			} else {
				terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
			}
		}

		for (int i = 0; i < wallCoordList.Count; i++) {
			terrainWallDictionary[wallCoordList[i]].UpdateTerrainChunk();
		}
	}

	public class TerrainChunk {

		
		public Vector4 numOfWalls = new Vector4(0,0,0,0);
		public List<Vector4> adjNumOfWalls = new List<Vector4>() {noWalls,noWalls,noWalls,noWalls};

		public GameObject meshObject;
		Vector2 coord;
		Vector2 position;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;

		MapData mapData;
		bool mapDataRevieved;
		int previousLODIndex = -1;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
			this.detailLevels = detailLevels;
			
			position = coord * size;
			bounds = new Bounds(position,Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x,0,position.y);

			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;

			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			SetVisible(false);

			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}

			mapGenerator.RequestMapData(position, OnMapDataReceived);

			chunkWallCreator(coord);

			
		}

		void OnMapDataReceived(MapData mapData) {
			this.mapData = mapData;
			mapDataRevieved = true;

			Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateTerrainChunk();
		}

		public void UpdateLODMeshes() {
			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}
			previousLODIndex = -1;
			UpdateTerrainChunk();
		}


		public void UpdateTerrainChunk() {
			if (mapDataRevieved) {
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if (visible) {
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length-1; i++) {
						if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
							lodIndex = i + 1;
						} else {
							break;
						}
					}

					coord = new Vector2(meshObject.transform.position.x, meshObject.transform.position.z) / 240;


					if (mapGenerator.GetComponent<TerrainGenerator>().terrainChunkDictionary.ContainsKey(new Vector2(coord[0], coord[1] + 1))) {
						adjNumOfWalls[0] = mapGenerator.GetComponent<TerrainGenerator>().terrainChunkDictionary[new Vector2(coord[0], coord[1] + 1)].numOfWalls;
					} else {adjNumOfWalls[0] = noWalls;}
					if (mapGenerator.GetComponent<TerrainGenerator>().terrainChunkDictionary.ContainsKey(new Vector2(coord[0] + 1, coord[1]))) {
						adjNumOfWalls[1] = mapGenerator.GetComponent<TerrainGenerator>().terrainChunkDictionary[new Vector2(coord[0] + 1, coord[1])].numOfWalls;
					} else {adjNumOfWalls[1] = noWalls;}
					if (mapGenerator.GetComponent<TerrainGenerator>().terrainChunkDictionary.ContainsKey(new Vector2(coord[0], coord[1] - 1))) {
						adjNumOfWalls[2] = mapGenerator.GetComponent<TerrainGenerator>().terrainChunkDictionary[new Vector2(coord[0], coord[1] - 1)].numOfWalls;
					} else {adjNumOfWalls[2] = noWalls;}
					if (mapGenerator.GetComponent<TerrainGenerator>().terrainChunkDictionary.ContainsKey(new Vector2(coord[0] - 1, coord[1]))) {
						adjNumOfWalls[3] = mapGenerator.GetComponent<TerrainGenerator>().terrainChunkDictionary[new Vector2(coord[0] - 1, coord[1])].numOfWalls;
					} else {adjNumOfWalls[3] = noWalls;}

					if (lodIndex != previousLODIndex) {
						LODMesh lodMesh = lodMeshes[lodIndex];
						if (lodMesh.hasMesh) {
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						}
						else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh(mapData, numOfWalls, adjNumOfWalls);
						}
					}
				}

				SetVisible (visible);
			}
		}

		public void SetVisible(bool visible) {
			meshObject.SetActive (visible);
		}

		public bool IsVisible() {
			return meshObject.activeSelf;
		}

		public void chunkWallCreator(Vector2 coord) {

			Vector2 northChunk = new Vector2(coord[0], coord[1] + 1);
			Vector2 eastChunk = new Vector2(coord[0] + 1, coord[1]);
			Vector2 southChunk = new Vector2(coord[0], coord[1] - 1);
			Vector2 westChunk = new Vector2(coord[0] - 1, coord[1]);

			if (!chunkCoordList.Contains(northChunk)) {
				numOfWalls[0] = 1;
				Vector2 wallCoords = new Vector2(coord[0], coord[1] + 0.5f);
				Vector2 chunkCoords = new Vector2(coord[0], coord[1] + 1f);
				Vector3 orientation = new Vector3(-90f,0f,0f);
				wallCoordList.Add(wallCoords);
				terrainWallDictionary.Add(wallCoords, new TerrainWall (wallCoords, chunkCoords, chunkSize, detailLevels, meshObject.transform.parent, mapMaterial, orientation, coord));

			} if (!chunkCoordList.Contains(eastChunk)) {
				numOfWalls[1] = 1;
				Vector2 wallCoords = new Vector2(coord[0] + 0.5f, coord[1]);
				Vector2 chunkCoords = new Vector2(coord[0] + 1f, coord[1]);
				Vector3 orientation = new Vector3(0f,0f,90f);
				wallCoordList.Add(wallCoords);
				terrainWallDictionary.Add(wallCoords, new TerrainWall (wallCoords, chunkCoords, chunkSize, detailLevels, meshObject.transform.parent, mapMaterial, orientation, coord));

			} if (!chunkCoordList.Contains(southChunk)) {
				numOfWalls[2] = 1;
				Vector2 wallCoords = new Vector2(coord[0], coord[1] - 0.5f);
				Vector2 chunkCoords = new Vector2(coord[0], coord[1] - 1f);
				Vector3 orientation = new Vector3(90f,0f,0f);
				wallCoordList.Add(wallCoords);
				terrainWallDictionary.Add(wallCoords, new TerrainWall (wallCoords, chunkCoords, chunkSize, detailLevels, meshObject.transform.parent, mapMaterial, orientation, coord));

			} if (!chunkCoordList.Contains(westChunk)) {
				numOfWalls[3] = 1;
				Vector2 wallCoords = new Vector2(coord[0] - 0.5f, coord[1]);
				Vector2 chunkCoords = new Vector2(coord[0] - 1f, coord[1]);
				Vector3 orientation = new Vector3(0f,0f,-90f);
				wallCoordList.Add(wallCoords);
				terrainWallDictionary.Add(wallCoords, new TerrainWall (wallCoords, chunkCoords, chunkSize, detailLevels, meshObject.transform.parent, mapMaterial, orientation, coord));

			}
		}

	}

	public class TerrainWall {

		public GameObject meshObject;
		public Vector2 chunkPosition;
		Vector2 wallPosition;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;

		MapData mapData;
		bool mapDataRevieved;
		int previousLODIndex = -1;

		public TerrainWall(Vector2 wallCoords, Vector2 chunkCoords, int size, LODInfo[] detailLevels, Transform parent, Material material, Vector3 rotation, Vector2 wallChunk) {
			this.detailLevels = detailLevels;
			
			
			chunkPosition = chunkCoords * size;
			wallPosition = wallCoords * size;
			bounds = new Bounds(chunkPosition,Vector2.one * size);
			Vector3 positionV3 = new Vector3(wallPosition.x,120,wallPosition.y);

			meshObject = new GameObject("Terrain Wall");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;

			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			meshObject.transform.Rotate(rotation[0], rotation[1], rotation[2], Space.World);
			meshObject.AddComponent<TerrainDestroyer>();
			meshObject.GetComponent<TerrainDestroyer>().chunkPosition = chunkCoords;
			meshObject.GetComponent<TerrainDestroyer>().wallCoords = wallCoords;
			meshObject.GetComponent<TerrainDestroyer>().wallChunk = wallChunk;

			SetVisible(false);

			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}

			mapGenerator.RequestMapData(chunkPosition, OnMapDataReceived);
		}

		void OnMapDataReceived(MapData mapData) {
			this.mapData = mapData;
			mapDataRevieved = true;

			Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateTerrainChunk();
		}

		public void UpdateTerrainChunk() {
			if (mapDataRevieved) {
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if (visible) {
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length-1; i++) {
						if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
							lodIndex = i + 1;
						} else {
							break;
						}
					}

					if (lodIndex != previousLODIndex) {
						LODMesh lodMesh = lodMeshes[lodIndex];
						if (lodMesh.hasMesh) {
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						}
						else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh(mapData, noWalls, new List<Vector4>() {noWalls,noWalls,noWalls,noWalls});
						}
					}
				}

				SetVisible(visible);
			}
		}

		public void SetVisible(bool visible) {
			meshObject.SetActive(visible);
		}

		public bool IsVisible() {
			return meshObject.activeSelf;
		}
	}

	class LODMesh {

		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		System.Action updateCallback;
		
		public LODMesh(int lod, System.Action updateCallback) {
			this.lod = lod;
			this.updateCallback = updateCallback;
		}

		void OnMeshDataReceived(MeshData meshData) {
			mesh = meshData.CreateMesh();
			hasMesh = true;

			updateCallback();
		}

		public void RequestMesh(MapData mapData, Vector4 numOfWalls, List<Vector4> adjNumOfWalls) {
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived, numOfWalls, adjNumOfWalls);
		}
		
	}

	[System.Serializable]
	public struct LODInfo {
		public int lod;
		public float visibleDstThreshold;
	}

	[System.Serializable]
	public struct Chunks {
		public Vector2 position;
	}

}