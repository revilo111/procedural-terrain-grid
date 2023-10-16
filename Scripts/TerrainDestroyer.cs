using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDestroyer : MonoBehaviour
{
    public bool destroyed;
    public Vector2 chunkPosition;
    public Vector2 wallCoords;
    public List<Vector2> coordList;
    public Vector2 wallChunk;
    public Vector4 chunkNumOfWalls;
    public Camera kamera;
    int wallNum;
    Vector4 noWalls = new Vector4(2,2,2,2);

    public List<Vector4> adjNumOfWalls;

    TerrainGenerator terrainGenerator;
    MapGenerator mapGenerator;


    bool debounce = true;


    

    void Start() {
        
        kamera = GameObject.Find("Camera").GetComponent<Camera>();

        destroyed = false;
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        coordList = TerrainGenerator.wallCoordList;
        chunkNumOfWalls = terrainGenerator.terrainChunkDictionary[wallChunk].numOfWalls;
        

        if (transform.rotation.eulerAngles.x == 90) {
            wallNum = 2;
        } else if (transform.rotation.eulerAngles.x == 270) {
            wallNum = 0;
        } else if (transform.rotation.eulerAngles.z == 90) {
            wallNum = 1;
        } else {
            wallNum = 3;
        }

    }

    void Update() {

        DetectObjectWithRaycast();
        chunkNumOfWalls = terrainGenerator.terrainChunkDictionary[wallChunk].numOfWalls;
        adjNumOfWalls = terrainGenerator.terrainChunkDictionary[wallChunk].adjNumOfWalls;
        if (debounce) {
            if (destroyed) {
                debounce = false;
                createChunk();
            }
        }
    }

    public void DetectObjectWithRaycast()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            Ray ray = kamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                print("lol");
                Debug.Log($"{hit.collider.name} Detected",
                    hit.collider.gameObject);
            }
        }
    }

    public void createChunk() {
        
        List<int> wallIndexToRemove = new List<int>();

        terrainGenerator.terrainChunkDictionary[wallChunk].numOfWalls[wallNum] = 0;

        for (int i = 0; i < coordList.Count; i++) {
            if (TerrainGenerator.terrainWallDictionary[coordList[i]].chunkPosition == chunkPosition * 240) {
                wallIndexToRemove.Add(i);
            }
        }
        wallIndexToRemove.Sort();
        wallIndexToRemove.Reverse();

        for (int i = 0; i < wallIndexToRemove.Count; i++) {
            TerrainGenerator.terrainWallDictionary.Remove(coordList[wallIndexToRemove[i]]);
            TerrainGenerator.wallCoordList.RemoveAt(wallIndexToRemove[i]);
        }
        TerrainGenerator.chunkCoordList.Add(chunkPosition);
        terrainGenerator.UpdateVisibleChunks();

        for (int i = 0; i < GameObject.Find("Map Generator").transform.childCount; i++) {
            GameObject wall = GameObject.Find("Map Generator").transform.GetChild(i).gameObject;
            if (wall.transform.position.y == 120) {
                Vector2 temp = wall.GetComponent<TerrainDestroyer>().chunkPosition;
                
                if (temp == chunkPosition && wall.GetComponent<TerrainDestroyer>().destroyed == false) {

                    if (wall.transform.rotation.eulerAngles.x == 90) {
                        wallNum = 2;
                    } else if (wall.transform.rotation.eulerAngles.x == 270) {
                        wallNum = 0;
                    } else if (wall.transform.rotation.eulerAngles.z == 90) {
                        wallNum = 1;
                    } else {
                        wallNum = 3;
                    }
                    
                    Vector2 coord = new Vector2(terrainGenerator.terrainChunkDictionary[wall.GetComponent<TerrainDestroyer>().wallChunk].meshObject.transform.position.x, terrainGenerator.terrainChunkDictionary[wall.GetComponent<TerrainDestroyer>().wallChunk].meshObject.transform.position.z) / 240;

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
                    

                    terrainGenerator.terrainChunkDictionary[wall.GetComponent<TerrainDestroyer>().wallChunk].numOfWalls[wallNum] = 0;
                    terrainGenerator.terrainChunkDictionary[wall.GetComponent<TerrainDestroyer>().wallChunk].UpdateLODMeshes();
                    Destroy(wall);

                    
                }
            } 
        }

        for (int i = -2; i < 3; i++) {
            for (int j = -2; j < 3; j++) {
                if (terrainGenerator.terrainChunkDictionary.ContainsKey(new Vector2(wallChunk.x + i, wallChunk.y + j))) {
                    terrainGenerator.terrainChunkDictionary[new Vector2(wallChunk.x + i, wallChunk.y + j)].UpdateLODMeshes();
                }
            }
        }

        terrainGenerator.terrainChunkDictionary[wallChunk].UpdateLODMeshes();
        terrainGenerator.UpdateVisibleChunks();
        Destroy(this.gameObject);
    }
}
