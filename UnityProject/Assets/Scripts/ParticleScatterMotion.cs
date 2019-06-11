using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class ParticleScatterMotion : MonoBehaviour {

    public float power = 5f;
    private List<Transform> particles;

    // Use this for initialization
    void Start () {
        particles = new List<Transform>();
        foreach(Transform child in transform)
        {
            particles.Add(child);
        }
	}
	
	// Update is called once per frame
	void Update () {
        Scatter();
	}

    private void Scatter()
    {
        float distance;
        Vector3 forceA;
        Vector3 forceB;
        foreach (Transform particleA in transform)
        {
            foreach (Transform particleB in transform)
            {
                if (particleA != particleB)
                {
                    distance = Vector3.Distance(particleA.position, particleB.position);
                    forceB = -1 * particleB.position.normalized * power / distance;
                    forceA = -1 * particleA.position.normalized * power / distance;
                    particleA.GetComponent<ParticleMotionScript>().ApplyForceOnce(forceB);
                    particleB.GetComponent<ParticleMotionScript>().ApplyForceOnce(forceA);
                }
            }
        }
    }
}
