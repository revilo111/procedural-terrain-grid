using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MeshGenerator {

	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail, Vector4 numOfWalls, List<Vector4> adjNumOfWalls) {
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
		int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

		MeshData meshData = new MeshData (verticesPerLine, verticesPerLine);
		int vertexIndex = 0;

		
		for (int y = 0; y < height; y += meshSimplificationIncrement) {
			for (int x = 0; x < width; x += meshSimplificationIncrement) {
				float xVal = x;
				float yVal = y;
				float xNum = width;
				float yNum = height;

				float xMultiplier = (((xVal + 1f)/xNum) - 0.5f) * (((xVal + 1f)/xNum) - 0.5f);
				float yMultiplier = (((yVal + 1f)/yNum) - 0.5f) * (((yVal + 1f)/yNum) - 0.5f);
				
				float relativeMultiplier = 1;
				float cornerScale = 2;

				Vector4 nChunk = adjNumOfWalls[0];
				Vector4 eChunk = adjNumOfWalls[1];
				Vector4 sChunk = adjNumOfWalls[2];
				Vector4 wChunk = adjNumOfWalls[3];

				if (numOfWalls == new Vector4(0,0,0,0)) {
					if (xVal/xNum < 0.5f && yVal/yNum < 0.5f) {
						if (wChunk[0] == 1 && nChunk[3] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xVal + 1)/xNum * (yVal + 1)/yNum;
						}
					} else if (yVal/yNum < 0.5f) {
						if (eChunk[0] == 1 && nChunk[1] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xNum - xVal + 1)/xNum * (yVal + 1)/yNum;
						}
					} else if (xVal/xNum > 0.5f) {
						if (eChunk[2] == 1 && sChunk[1] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xNum - xVal + 1)/xNum * (yNum - yVal + 1)/yNum;
						}
					} else {
						if (wChunk[2] == 1 && sChunk[3] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xVal + 1)/xNum * (yNum - yVal + 1)/yNum;
						}
					}
				} else if (numOfWalls == new Vector4(1,0,0,0)) {
					if (yVal/yNum < 0.5f) {
						relativeMultiplier = 100 * yMultiplier;	
					} else if(xVal/xNum < 0.5f) {
						if (wChunk[2] == 1 && sChunk[3] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xVal + 1)/xNum * (yNum - yVal + 1)/yNum;
						}
					} else {
						if (eChunk[2] == 1 && sChunk[1] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xNum - xVal + 1)/xNum * (yNum - yVal + 1)/yNum;
						}
					}
				} else if (numOfWalls == new Vector4(0,1,0,0)) {
					if (xVal/xNum > 0.5f) {
						relativeMultiplier = 100 * xMultiplier;	
					} else if(yVal/yNum > 0.5f) {
						if (sChunk[3] == 1 && wChunk[2] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xVal + 1)/xNum * (yNum - yVal + 1)/yNum;
						}
					} else {
						if (nChunk[3] == 1 && wChunk[0] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xVal + 1)/xNum * (yVal + 1)/yNum;
						}
					}
				} else if (numOfWalls == new Vector4(0,0,1,0)) {
					if (yVal/yNum > 0.5f) {
						relativeMultiplier = 100 * yMultiplier;	
					} else if(xVal/xNum < 0.5f) {
						if (wChunk[0] == 1 && nChunk[3] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xVal + 1)/xNum * (yVal + 1)/yNum;
						}
					} else {
						if (eChunk[0] == 1 && nChunk[1] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xNum - xVal + 1)/xNum * (yVal + 1)/yNum;
						}
					}
				} else if (numOfWalls == new Vector4(0,0,0,1)) {
					if (xVal/xNum < 0.5f) {
						relativeMultiplier = 100 * xMultiplier;	
					} else if(yVal/yNum < 0.5f) {
						if (nChunk[1] == 1 && eChunk[0] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xNum - xVal + 1)/xNum * (yVal + 1)/yNum;
						}
					} else {
						if (eChunk[2] == 1 && sChunk[1] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xNum - xVal + 1)/xNum * (yNum - yVal + 1)/yNum;
						}
					}
				} else if (numOfWalls == new Vector4(1,0,1,0)) {
					relativeMultiplier = 100 * yMultiplier;
				} else if (numOfWalls == new Vector4(0,1,0,1)) {
					relativeMultiplier = 100 * xMultiplier;
				} else if (numOfWalls == new Vector4(1,1,0,1)) {
					if (yVal/yNum < 0.5f) {
						relativeMultiplier = 100 * (yMultiplier + xMultiplier);
					} else {
						relativeMultiplier = 100 * xMultiplier;
					}
				} else if (numOfWalls == new Vector4(0,1,1,1)) {
					if (yVal/yNum > 0.5f) {
						relativeMultiplier = 100 * (yMultiplier + xMultiplier);
					} else {
						relativeMultiplier = 100 * xMultiplier;
					}
				} else if (numOfWalls == new Vector4(1,0,1,1)) {
					if (xVal/xNum < 0.5f) {
						relativeMultiplier = 100 * (yMultiplier + xMultiplier);
					} else {
						relativeMultiplier = 100 * yMultiplier;
					}
				} else if (numOfWalls == new Vector4(1,1,1,0)) {
					if (xVal/xNum > 0.5f) {
						relativeMultiplier = 100 * (yMultiplier + xMultiplier);
					} else {
						relativeMultiplier = 100 * yMultiplier;
					}
				} else if (numOfWalls == new Vector4(0,1,1,0)) {
					if (yVal/yNum > 0.5f && xVal/xNum > 0.5f) {
						relativeMultiplier = 100 * (yMultiplier + xMultiplier);
					} else if (xVal/xNum > 0.5f) {
						relativeMultiplier = 100 * (xMultiplier);
					} else if (yVal/yNum > 0.5f) {
						relativeMultiplier = 100 * (yMultiplier);
					} else {
						if (nChunk[3] == 1 && wChunk[0] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xVal + 1)/xNum * (yVal + 1)/yNum;
						}
					}
				} else if (numOfWalls == new Vector4(1,0,0,1)) {
					if (yVal/yNum < 0.5f && xVal/xNum < 0.5f) {
						relativeMultiplier = 100 * (yMultiplier + xMultiplier);
					} else if (xVal/xNum < 0.5f) {
						relativeMultiplier = 100 * (xMultiplier);
					} else if (yVal/yNum < 0.5f) {
						relativeMultiplier = 100 * (yMultiplier);
					} else {
						if (eChunk[2] == 1 && sChunk[1] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xNum - xVal + 1)/xNum * (yNum - yVal + 1)/yNum;
						}
					}
				} else if (numOfWalls == new Vector4(0,0,1,1)) {
					if (yVal/yNum > 0.5f && xVal/xNum < 0.5f) {
						relativeMultiplier = 100 * (yMultiplier + xMultiplier);
					} else if (xVal/xNum < 0.5f) {
						relativeMultiplier = 100 * (xMultiplier);
					} else if (yVal/yNum > 0.5f) {
						relativeMultiplier = 100 * (yMultiplier);
					} else {
						if (nChunk[1] == 1 && eChunk[0] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xNum - xVal + 1)/xNum * (yVal + 1)/yNum;
						}
					}
				} else if (numOfWalls == new Vector4(1,1,0,0)) {
					if (yVal/yNum < 0.5f && xVal/xNum > 0.5f) {
						relativeMultiplier = 100 * (yMultiplier + xMultiplier);
					} else if (xVal/xNum > 0.5f) {
						relativeMultiplier = 100 * (xMultiplier);
					} else if (yVal/yNum < 0.5f) {
						relativeMultiplier = 100 * (yMultiplier);
					} else {
						if (sChunk[3] == 1 && wChunk[2] == 1) {
							relativeMultiplier = 400 * (yMultiplier * xMultiplier);
						} else {
							relativeMultiplier = 1 * (xVal + 1)/xNum * (yNum - yVal + 1)/yNum;
						}	
					}
				} 

				meshData.vertices [vertexIndex] = new Vector3 (topLeftX + x, heightCurve.Evaluate (heightMap [x, y]) * heightMultiplier + cornerScale * relativeMultiplier, topLeftZ - y);
				meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1) {
					meshData.AddTriangle (vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData;

	}
}

public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight) {
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
	}

}