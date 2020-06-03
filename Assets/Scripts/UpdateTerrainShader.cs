using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateTerrainShader : MonoBehaviour
{
    World world;

    Texture3D texture;
    float[,,] voxels;

    void Start() {
        world = GetComponent<World>();
        PassVoxelsToShader();
    }

    void Update() {

    }

    Texture3D PassVoxelsToShader() {
        voxels = world.asteroidData;
        int len0 = voxels.GetLength(0);
        int len1 = voxels.GetLength(1);
        int len2 = voxels.GetLength(2);

        Color[] colorArray = new Color[len0 * len1 * len2];
        texture = new Texture3D(len0, len1, len2, TextureFormat.Alpha8, false);

        for (int x = 0; x < voxels.GetLength(0); x++) {
            for (int y = 0; y < voxels.GetLength(1); y++) {
                for (int z = 0; z < voxels.GetLength(2); z++) {
                    int thisID = Mathf.FloorToInt(voxels[x, y, z]);
                    colorArray[x + (y * len0) + (z * len0 * 1)] = new Color(0,0,0, thisID);
                }
            }
        }

        texture.SetPixels(colorArray);
        texture.Apply();
        return texture;
    }
}
