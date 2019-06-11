using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMagnetScript : MonoBehaviour {

    public float power = 1;
    private Vector3 powerVector;

	// Use this for initialization
	void Start () {
        powerVector = power * transform.position.normalized;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {

        ParticleMotionScript particle = other.gameObject.GetComponent<ParticleMotionScript>();
        
        if (particle != null)
        {
            // Add the new force with dampen true
            particle.AddForce(powerVector, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ParticleMotionScript particle = other.gameObject.GetComponent<ParticleMotionScript>();

        if (particle != null)
        {
            // Remove magnetic force since the object is out of field range
            particle.RemoveForce(powerVector, false);
        }
    }
}
