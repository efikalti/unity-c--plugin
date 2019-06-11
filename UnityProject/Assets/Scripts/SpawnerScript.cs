using UnityEngine;
using System.Collections;

public class SpawnerScript : MonoBehaviour {
	public float spawnTime = 3f;		// The amount of time between each spawn.
	public float spawnDelay = 0f;		// The amount of time before spawning starts.
    public int max_particles = 50;
    public bool particleGravity = true;
    public string prefab_name = "Prefabs/particle_prefab";

    public float r = 1.0f;

    private GameObject [] object_prefabs;		// Array of prefabs.
    private int counter;

	// Use this for initialization
	void Start () {
		object_prefabs = new GameObject[1];
        object_prefabs[0] = Resources.Load<GameObject>(prefab_name);
        counter = 0;

        InvokeRepeating("SpawnRandom", spawnDelay, spawnTime);
	}
    
    /**
     * Spawn particles at a random position near the spawner
     **/
    void SpawnRandom()
	{
        if (counter >= max_particles) { return; }

		for(int i=0; i<1; i++){
            // Add random r offset to x, y, z coordinates for position
			float x_perturb = Random.Range(-r, r);
			float y_perturb = Random.Range(-r, r);
            float z_perturb = 0;
            // Calculate position vector
			Vector3 pos = new Vector3(transform.position.x+x_perturb, transform.position.y+y_perturb, transform.position.z+z_perturb);
            // Instantiate object
			GameObject obg = Instantiate(object_prefabs[0], pos, transform.rotation);
            obg.GetComponent<ParticleMotionScript>().gravity = particleGravity;
            counter++;
        }
	}

    /**
     * Spawn particles from the origin of the spawner 
     **/
    void SpawnFromSelf()
    {
        if (counter >= max_particles) { return; }

        // Instantiate object at the spawner's position
        Instantiate(object_prefabs[0], transform.position, transform.rotation);

        counter++;
    }
}