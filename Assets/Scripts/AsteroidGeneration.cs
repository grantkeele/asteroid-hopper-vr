using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidGeneration : MonoBehaviour
{
    public int worldLength;
    public int worldWidth;
    public int worldHeight;

    public int radius = 20;
    private int diameter;

    World world;

    [HideInInspector] public float[,,] voxels;

    private FastNoise fastNoise;
    public int seed;
    public float noiseFreq;
    public float noiseScale;

    public int shellThickness = 1;

    // Start is called before the first frame update
    void Awake() {
        Random.InitState(System.DateTime.Now.Second);

        seed = (int)Random.Range(1, 9999);
        fastNoise = new FastNoise(seed);

        fastNoise.SetSeed(seed);
        fastNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
        fastNoise.SetFrequency(noiseFreq);
    }

    void Start() {
        world = GetComponent<World>();
    }

    // Update is called once per frame
    void Update() {

    }
    
    public float[,,] GenerateAsteroidData(int type, int l, int w, int h) {
        voxels = new float[l, w, h];
        switch (type) {
            case 0:
                return AsteroidSphere(l, w, h);
            case 1:
                return AsteroidPerlin(l, w, h);
            case 2:
                return AsteroidHollow(l, w, h);
            default:
                return new float[0,0,0];
        }
    }

    private float[,,] AsteroidSphere(int l, int w, int h) {
        diameter = radius * 2;
        float radSquared = radius * radius;
        // differenceSquared is the difference between the radius squared and the radius minus one squared 
        // Represents an "outer shell" of a sphere, used to normalize the gradient between inner shell and outer shell
        float differenceSquared = radSquared - Mathf.Pow(radius - shellThickness, 2);

        //int noise = Mathf.RoundToInt(Random.Range(-noiseRange, noiseRange));

        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                for (int z = -radius; z < radius; z++) {
                    // Creates a sphere by calculating the distance between each point and the center (squared so as not to use an expensive square root function)
                    // distSquared is the distance squared
                    int distSquared = x * x + y * y + z * z;

                    if (distSquared <= Mathf.Pow(radius - shellThickness, 2)) {
                        voxels[x + l/2, y + w/2, z + h/2] = 1;
                    }
                    else if (distSquared <= radSquared) {
                        // Takes the difference between distance and radius (near the edge) and calculates the gradient between them
                        voxels[x + l / 2, y + w / 2, z + h / 2] = (radSquared - distSquared) / differenceSquared;
                    }
                }
            }
        }
        return voxels;
    }
    private float[,,] AsteroidPerlin(int l, int w, int h) {
        diameter = radius * 2;
        float radSquared = radius * radius;
        // differenceSquared is the difference between the radius squared and the radius minus one squared 
        // Represents an "outer shell" of a sphere, used to normalize the gradient between inner shell and outer shell
        float differenceSquared = radSquared - Mathf.Pow(radius - shellThickness, 2);

        //int noise = Mathf.RoundToInt(Random.Range(-noiseRange, noiseRange));

        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                for (int z = -radius; z < radius; z++) {
                    // Creates a sphere by calculating the distance between each point and the center (squared so as not to use an expensive square root function)
                    // distSquared is the distance squared
                    int distSquared = x * x + y * y + z * z;
                    float grad = 1 - (radSquared - distSquared) / differenceSquared;

                    if (distSquared <= Mathf.Pow(radius - shellThickness, 2)) {
                        voxels[x + (l / 2), y + (w / 2), z + (h / 2)] = RandInt(0, 3) + fastNoise.GetNoise(
                            (x + radius) / noiseScale, 
                            (y + radius) / noiseScale, 
                            (z + radius) / noiseScale);
                    }
                    else if (distSquared <= radSquared) {
                        voxels[x + (l / 2), y + (w / 2), z + (h / 2)] = RandInt(0, 3) + fastNoise.GetNoise(
                            (x + radius) / noiseScale,
                            (y + radius) / noiseScale,
                            (z + radius) / noiseScale) * (-grad + 1);
                    }
                }
            }
        }
        return voxels;
    }
    private float[,,] AsteroidHollow(int l, int w, int h) {
        diameter = radius * 2;
        float radSquared = radius * radius;
        // differenceSquared is the difference between the radius squared and the radius minus one squared 
        // Represents an "outer shell" of a sphere, used to normalize the gradient between inner shell and outer shell
        float differenceSquared = radSquared - Mathf.Pow(radius - shellThickness, 2);

        //int noise = Mathf.RoundToInt(Random.Range(-noiseRange, noiseRange));

        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                for (int z = -radius; z < radius; z++) {
                    // Creates a sphere by calculating the distance between each point and the center (squared so as not to use an expensive square root function)
                    // distSquared is the distance squared
                    int distSquared = x * x + y * y + z * z;
                    // Takes the difference between distance and radius (near the edge) and calculates the gradient between them
                    float grad = 1 - (radSquared - distSquared) / differenceSquared;

                    if (distSquared <= Mathf.Pow(radius - shellThickness, 2)) {
                        voxels[x + l / 2, y + w / 2, z + h / 2] = 0;
                    }
                    else if (distSquared <= Mathf.Pow(radius - (shellThickness * 0.5f), 2)) {                   
                        voxels[x + l / 2, y + w / 2, z + h / 2] = grad + 0.4f;//-1 * Mathf.Abs(grad - 0.5f) + 1;
                    }
                    else if (distSquared <= radSquared) {                       
                        voxels[x + l / 2, y + w / 2, z + h / 2] = -(grad - 1) + 0.4f;//-1 * Mathf.Abs(grad - 0.5f) + 1;
                    }
                }
            }
        }
        return voxels;
    }


    private int RandInt(int min, int max) {
        return Mathf.RoundToInt(Random.Range(min, max));
    }
}
