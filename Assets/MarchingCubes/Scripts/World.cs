using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int chunkSize;
    
    public float polySize;
    public float isolevel;

    [HideInInspector] public int boundaryLength;
    [HideInInspector] public int boundaryWidth;
    [HideInInspector] public int boundaryHeight;

    public GameObject thisAsteroid;
    public GameObject chunkPrefab;

    public Dictionary<Vector3Int, Chunk> chunks;

    private Bounds worldBounds;

    public DensityGenerator densityGenerator;
    public AsteroidGeneration asteroidGeneration;

    public float[,,] asteroidData;
    public int[,,] asteroidMaterials;

    public int asteroidTypeID = 0;

    public Material terrainMat;


    private void Awake()
    {
        asteroidGeneration = GetComponent<AsteroidGeneration>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(worldBounds.center * polySize, worldBounds.size * polySize);
    }
    
    private void Start()
    {   
        boundaryLength = asteroidGeneration.worldLength * chunkSize;
        boundaryWidth = asteroidGeneration.worldWidth * chunkSize;
        boundaryHeight = asteroidGeneration.worldHeight * chunkSize;

        asteroidData = asteroidGeneration.GenerateAsteroidData(asteroidTypeID, boundaryLength, boundaryWidth, boundaryHeight);

        thisAsteroid = new GameObject("Asteroid " + asteroidGeneration.seed.ToString());

        densityGenerator = new DensityGenerator(asteroidGeneration.seed);

        worldBounds = new Bounds();
        UpdateBounds();

        chunks = new Dictionary<Vector3Int, Chunk>(asteroidGeneration.worldWidth * asteroidGeneration.worldHeight * asteroidGeneration.worldLength);
        CreateChunks();

        PassVoxelsToShader();
    }

    private void CreateChunks()
    {
        for (int x = 0; x < asteroidGeneration.worldLength; x++)
        {
            for (int y = 0; y < asteroidGeneration.worldWidth; y++)
            {
                for (int z = 0; z < asteroidGeneration.worldHeight; z++)
                {
                    CreateChunk(x * chunkSize, y * chunkSize, z * chunkSize);
                }
            }
        }

        // Increase resolution of voxels by scaling down the asteroid
        thisAsteroid.transform.localScale = new Vector3(polySize, polySize, polySize);
    }

    private Chunk GetChunk(Vector3Int pos)
    {
        return GetChunk(pos.x, pos.y, pos.z);
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        int newX = Utils.FloorToNearestX(x, chunkSize);
        int newY = Utils.FloorToNearestX(y, chunkSize);
        int newZ = Utils.FloorToNearestX(z, chunkSize);

        return chunks[new Vector3Int(newX, newY, newZ)];
    }

    public float GetDensity(int x, int y, int z)
    {
        VoxelPoint p = GetVoxelPoint(x, y, z);

        return p.density;
    }

    public float GetDensity(Vector3Int pos)
    {
        return GetDensity(pos.x, pos.y, pos.z);
    }

    public VoxelPoint GetVoxelPoint(int x, int y, int z)
    {
        Chunk chunk = GetChunk(x, y, z);

        VoxelPoint p = chunk.GetVoxelPoint(x.Mod(chunkSize),
                                 y.Mod(chunkSize),
                                 z.Mod(chunkSize));

        return p;
    }

    public void SetDensity(float density, int worldPosX, int worldPosY, int worldPosZ, bool setReadyForUpdate, Chunk[] initChunks)
    {
        Vector3Int dp = new Vector3Int(worldPosX, worldPosY, worldPosZ);

        Vector3Int lastChunkPos = dp.FloorToNearestX(chunkSize);

        for (int i = 0; i < 8; i++)
        {
            Vector3Int chunkPos = (dp - MarchingCubes.CubePoints[i]).FloorToNearestX(chunkSize);

            if (i != 0 && chunkPos == lastChunkPos)
            {
                continue;
            }

            Chunk chunk = GetChunk(chunkPos);
            
            lastChunkPos = chunk.position;

            Vector3Int localPos = (dp - chunk.position).Mod(chunkSize + 1);

            chunk.SetDensity(density, localPos);
            if (setReadyForUpdate) 
                chunk.readyForUpdate = true;
        }
    }

    public void SetDensity(float density, Vector3Int pos, bool setReadyForUpdate, Chunk[] initChunks)
    {
        SetDensity(density, pos.x, pos.y, pos.z, setReadyForUpdate, initChunks);
    }

    private void UpdateBounds()
    {
        float middleX = asteroidGeneration.worldLength * chunkSize / 2f;
        float middleY = asteroidGeneration.worldWidth * chunkSize / 2f;
        float middleZ = asteroidGeneration.worldHeight * chunkSize / 2f;
        
        Vector3 midPos = new Vector3(middleX, middleY, middleZ);

        Vector3Int size = new Vector3Int(
            asteroidGeneration.worldWidth * chunkSize,
            asteroidGeneration.worldHeight * chunkSize,
            asteroidGeneration.worldLength * chunkSize);

        worldBounds.center = midPos;
        worldBounds.size = size;
    }

    public bool IsVoxelPointInsideWorld(int x, int y, int z)
    {
        return IsVoxelPointInsideWorld(new Vector3Int(x, y, z));
    }

    public bool IsVoxelPointInsideWorld(Vector3Int point)
    {
        return worldBounds.Contains(point);
    }

    private void CreateChunk(int x, int y, int z)
    {
        Vector3Int position = new Vector3Int(x, y, z);

        Chunk chunk = Instantiate(chunkPrefab, position, Quaternion.identity).GetComponent<Chunk>();
        chunk.Initialize(this, chunkSize, position);
        chunks.Add(position, chunk);
        chunk.transform.SetParent(thisAsteroid.transform);
    }

    public void PassVoxelsToShader() {
        float[,,] voxels = asteroidData;
        int len0 = voxels.GetLength(0);
        int len1 = voxels.GetLength(1);
        int len2 = voxels.GetLength(2);

        Color[] colorArray = new Color[len0 * len1 * len2];
        Texture3D texture = new Texture3D(len0, len1, len2, TextureFormat.ARGB32, false);

        for (int x = 0; x < len0; x++) {
            for (int y = 0; y < len1; y++) {
                for (int z = 0; z < len2; z++) {
                    float thisID = 3;//Mathf.Floor(voxels[x, y, z]);
                    float IDFloat255 = thisID / 255.0f;
                    colorArray[x + (y * len0) + (z * len0 * 1)] = new Color(IDFloat255, 0, 0, 0);
                }
            }
        }

        texture.SetPixels(colorArray);
        texture.Apply();

        terrainMat.SetTexture("_VoxelArray", texture);
    }
}
