using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockParticleScript : MonoBehaviour {
    public int max_birds = 10;

    private GameObject bird_prefab;
    private List<GameObject> birds;

    // Use this for initialization
    void Start ()
    {
        bird_prefab = Resources.Load<GameObject>("Prefabs/bird_particle");
        birds = new List<GameObject>();
        SpawnBirds();
    }
	
	// Update is called once per frame
	void Update () {
        UpdateBirds();

    }

    void UpdateBirds()
    {
        foreach (GameObject bird in birds)
        {
            bird.GetComponent<BirdParticleMotionScript>().flock(birds);
        }
    }

    void SpawnBirds()
    {
        GameObject bird;


        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;
        for (int i=0; i<max_birds; i++)
        {
            float r = 1.0f;
            // Add random r offset to x, y, z coordinates for position
            float x_perturb = Random.Range(-r, r);
            float y_perturb = Random.Range(-r, r);
            float z_perturb = 0;
            // Calculate position vector
            Vector3 pos = new Vector3(transform.position.x + x_perturb, transform.position.y + y_perturb, transform.position.z + z_perturb);
            // Instantiate object
            bird = Instantiate(bird_prefab, pos, transform.rotation);
            bird.AddComponent<BirdParticleMotionScript>();
            bird.GetComponent<BirdParticleMotionScript>().SetWidthHeight(horzExtent, vertExtent);
            bird.name = "bird_particle_" + (i+1);
            birds.Add(bird);
        }
    }

}
