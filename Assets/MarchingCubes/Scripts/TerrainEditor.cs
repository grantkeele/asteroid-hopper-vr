using UnityEngine;

public class TerrainEditor : MonoBehaviour
{
    private bool addTerrain = true;
    [SerializeField] private float force = 2f;
    [SerializeField] private float range = 2f;

    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float minDistance = 0.5f;

    [SerializeField] private AnimationCurve forceOverDistance = AnimationCurve.Constant(0, 1, 1);

    [SerializeField] private World world;

    [SerializeField] private GameObject rightHandAnchor;
    [SerializeField] private GameObject leftHandAnchor;
    GameObject primaryHand;
    GameObject secondaryHand;

    Chunk[] _initChunks;

    private void Start()
    {
        primaryHand = leftHandAnchor;
        secondaryHand = rightHandAnchor;

        _initChunks = new Chunk[8];
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        TryEditTerrain();
    }

    private void TryEditTerrain()
    {
        if (force <= 0 || range <= 0)
        {
            return;
        }

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.5f)
        {
            RaycastToTerrain(primaryHand.transform, !addTerrain);
        }
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f)
        {
            RaycastToTerrain(secondaryHand.transform, addTerrain);
        }
    }

    private void RaycastToTerrain(Transform handVector, bool addTerrain)
    {
        Vector3 startP = handVector.position;
        Vector3 destP = startP + handVector.forward;
        Vector3 direction = destP - startP;
        Vector3 newStartP = startP + (direction * minDistance);

        Ray ray = new Ray(newStartP, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Vector3 hitPoint = hit.point;

            if (addTerrain)
            {
                Collider[] hits = Physics.OverlapSphere(hitPoint, range / 2f * 0.8f, 8);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (!hits[i].CompareTag("Terrain"))
                    {
                        return;
                    }
                }
            }

            Vector3 localHitPoint = new Vector3(hitPoint.x / world.polySize, hitPoint.y / world.polySize, hitPoint.z / world.polySize);
            EditTerrain(localHitPoint, addTerrain, force, range);
        }
    }

    private void EditTerrain(Vector3 point, bool addTerrain, float force, float range)
    {
        int buildModifier = addTerrain ? 1 : -1;

        int hitX = (point.x).Round();
        int hitY = (point.y).Round();
        int hitZ = (point.z).Round();

        int intRange = range.Ceil();

        for (int x = -intRange; x <= intRange; x++)
        {
            for (int y = -intRange; y <= intRange; y++)
            {
                for (int z = -intRange; z <= intRange; z++)
                {
                    int offsetX = hitX - x;
                    int offsetY = hitY - y;
                    int offsetZ = hitZ - z;

                    if (!world.IsVoxelPointInsideWorld(offsetX, offsetY, offsetZ))
                        continue;

                    float distance = Utils.Distance(offsetX, offsetY, offsetZ, point);
                    if (!(distance <= range)) continue;
                    
                    float modificationAmount = force / distance * forceOverDistance.Evaluate(1 - distance.Map(0, force, 0, 1)) * buildModifier;

                    float oldDensity = world.GetDensity(offsetX, offsetY, offsetZ);
                    float newDensity = oldDensity + modificationAmount;

                    newDensity = newDensity.Clamp01();

                    world.SetDensity(newDensity, offsetX, offsetY, offsetZ, true, _initChunks);

                    float oldPointData = world.asteroidData[offsetX, offsetY, offsetZ];

                    if (newDensity < 0.5) {
                        world.asteroidData[offsetX, offsetY, offsetZ] = newDensity;
                    }
                    else {
                        world.asteroidData[offsetX, offsetY, offsetZ] = Mathf.Floor(oldPointData) + newDensity;
                    }               
                }
            }
        }
    }
}

