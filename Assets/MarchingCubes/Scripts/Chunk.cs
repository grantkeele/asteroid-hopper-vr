using UnityEngine;

public class Chunk : MonoBehaviour
{
    [HideInInspector] public bool readyForUpdate;
    [HideInInspector] public VoxelPoint[,,] points;
    [HideInInspector] public int chunkSize;
    [HideInInspector] public Vector3Int position;

    private float _isolevel;
    private int _seed;

    private MarchingCubes _marchingCubes;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private DensityGenerator _densityGenerator;

    private AsteroidGeneration asteroidGeneration;

    int midLength;
    int midWidth;
    int midHeight;

    private void Awake(){
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    private void Start()
    {
        Generate();
    }

    private void Update()
    {
        if (readyForUpdate)
        {
            Generate();
            readyForUpdate = false;
        }
    }

    public void Initialize(World world, int chunkSize, Vector3Int position)
    {
        asteroidGeneration = world.gameObject.GetComponent<AsteroidGeneration>();

        this.chunkSize = chunkSize;
        this.position = position;
        _isolevel = world.isolevel;

        _densityGenerator = world.densityGenerator;
        
        int worldPosX = position.x;
        int worldPosY = position.y;
        int worldPosZ = position.z;

        midLength = chunkSize * asteroidGeneration.worldLength / 2;
        midWidth = chunkSize * asteroidGeneration.worldWidth / 2;
        midHeight = chunkSize * asteroidGeneration.worldHeight / 2;

        points = new VoxelPoint[chunkSize + 1, chunkSize + 1, chunkSize + 1];
           
        asteroidGeneration = world.asteroidGeneration;
        _seed = asteroidGeneration.seed;
        _marchingCubes = new MarchingCubes(points, _isolevel, _seed);

        var asteroidLength = asteroidGeneration.radius * 2;
        var asteroidWidth = asteroidGeneration.radius * 2;
        var asteroidHeight = asteroidGeneration.radius * 2;

        // Fill array with density and material ID of 0
        for (int x = 0; x < points.GetLength(0); x++)
        {
            for (int y = 0; y < points.GetLength(1); y++)
            {
                for (int z = 0; z < points.GetLength(2); z++)
                {
                    points[x, y, z] = new VoxelPoint(
                        new Vector3Int(x, y, z),
                        0.0f,
                        0
                    );
                }
            }
        }

        for (int x = 0; x < points.GetLength(0); x++) {
            for (int y = 0; y < points.GetLength(1); y++) {
                for (int z = 0; z < points.GetLength(2); z++) {
                    int currX = x + worldPosX;
                    int currY = y + worldPosY;
                    int currZ = z + worldPosZ;

                    float density = GetVoxelDensity(world.asteroidData, currX, currY, currZ);
                    SetDensity(density, x, y, z);

                    int material = GetVoxelMaterial(world.asteroidData, currX, currY, currZ);
                    SetMaterial(material, x, y, z);
                }
            }
        }
    }

    public void Generate()
    {
        Mesh mesh = _marchingCubes.CreateMeshData(points);
        
        _meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;

        mesh.RecalculateNormals();
    }

    public VoxelPoint GetVoxelPoint(int x, int y, int z)
    {
        return points[x, y, z];
    }

    public void SetMaterial(int material, int x, int y, int z) {
        points[x, y, z].materialID = material;
    }

    public void SetDensity(float density, int x, int y, int z)
    {
        points[x, y, z].density = density;
    }

    public void SetDensity(float density, Vector3Int pos)
    {
        SetDensity(density, pos.x, pos.y, pos.z);
    }

    public float GetVoxelDensity(float[,,] dataArray, int x, int y, int z) {
        // Only return a value if within bounds
        if (x >= 0 && x < dataArray.GetLength(0)) {
            if (y >= 0 && y < dataArray.GetLength(1)) {
                if (z >= 0 && z < dataArray.GetLength(2)) {
                    float rawData = dataArray[x, y, z];
                    float density = rawData - Mathf.Floor(rawData);
                    return density;
                }
            }
        }
        // If any of those fail, return empty
        return 0;
    }

    public int GetVoxelMaterial(float[,,] dataArray, int x, int y, int z) {
        // Only return a value if within bounds
        if (x >= 0 && x < dataArray.GetLength(0)) {
            if (y >= 0 && y < dataArray.GetLength(1)) {
                if (z >= 0 && z < dataArray.GetLength(2)) {
                    float rawData = dataArray[x, y, z];
                    int material = Mathf.FloorToInt(rawData);
                    return material;
                }
            }
        }
        // If any of those fail, return empty
        return 0;
    }
}
