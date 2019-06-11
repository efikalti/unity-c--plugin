using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class ParticleMotionScript : MonoBehaviour {

    // Public variables
    public bool gravity = true;
    public float gravityForce = 1;
    public float dampenValue = 0.01f;

    // Private variables
    private float[] particle_states;
    private IList forces;
    private float dt;


    // Plugin API methods
    [DllImport("ParticlePlugin")]
	private static extern void Gravity(float[] particle_states,float[] center, float dt);
    [DllImport("ParticlePlugin")]
    private static extern void Reflect(float[] particle_states, float[] wall_normal, float[] bounce_force);
    [DllImport("ParticlePlugin")]
    private static extern void Move(float[] particle_states, float dt);


    // Initialize particle values
    void Start () {
        // Initialize arrays for particle state
		particle_states = new float[6];

        // Set initial values
        for (int d = 0; d < 3; d++)
        {
            particle_states[d] = transform.position[d];
            particle_states[d + 3] = 0;
        }

        // Initialize list for forces
        forces = new ArrayList();

        // Initiate gravity if enabled
        if(gravity)
        {
            Gravity();
        }
    }


    // Called on fixed intervals
    void FixedUpdate ()
    {

        dt = Time.deltaTime + 0.1f;
        Move();
        SumForces();
    }


    /**
     * Sets a target position for the particle to move to that is lower than the original position
     **/
    void Gravity()
    {
        // Call gravity api to calculate the new state of the particle
        //Gravity(particle_states, gravity, 1.0f);
        AddForce(new Vector3(0, -gravityForce, 0), false);
    }


    /*
     * Api call to the Move function to calculate the new position
     * Update position of particle
     */
    void Move()
    {
        Move(particle_states, dt);
        transform.position = new Vector3(particle_states[0], particle_states[1], particle_states[2]);
    }


    /**
     * Detects collision with other objects
     * Calls Reflect api call with the object's normal to get the resulting collision velocity
     **/
    private void OnCollisionEnter(Collision collision)
    {
        // Get normal from collision surface
        float[] normal = new float[3];
        normal[0] = collision.contacts[0].normal.x;
        normal[1] = collision.contacts[0].normal.y;
        normal[2] = collision.contacts[0].normal.z;

        // Call reflect to calculate the resulting force
        float[] bounce_force = new float[3];
        Reflect(particle_states, normal, bounce_force);

        // Add the new force with dampen true
        AddForce(new Vector3(bounce_force[0], bounce_force[1], bounce_force[2]), true);
    }


    /*
     * Adds a new force to be applied to this particle
     * If dampen is true, the force will dampen over time by 
     * the minimizeInterval value every update
     */
    public void AddForce(Vector3 force, bool dampen)
    {
        Force newForce = gameObject.AddComponent<Force>();
        newForce.Set(force.x, force.y, force.z, dampenValue * force.magnitude, dampen);
        forces.Add(newForce);
    }

    /*
    * Apply this force one on the particle without saving it
    */
    public void ApplyForceOnce(Vector3 force)
    {
        // Set force
        for (int i = 0; i < 3; i++) particle_states[i + 3] = force[i];
        // Update position
        Move();
    }

    /*
     * Remove the specified force from the list of active forces
     */
    public void RemoveForce(Vector3 force, bool dampen)
    {
        int index = FindForce(force, dampen);
        if (index > 0)
        {
            Force f = (Force)forces[index];
            forces.RemoveAt(index);
            Destroy(f);
        }
    }


    public int FindForce(Vector3 force, bool dampen)
    {
        int index = 0;

        foreach(Force f in forces)
        {
            if (f.Values[0] == force[0] && f.Values[1] == force[1] && f.Values[2] == force[2] && f.Dampen == dampen)
            {
                return index;
            }
            index++;
        }

        return -1;
    }


    /*
     * Sum forces applied to this object
     */
    public void SumForces()
    {
        for (int i = 0; i < 3; i++) particle_states[i+3] = 0;
        // Sum forces
        for (int index=0; index < forces.Count; index++)
        {
            // Check if force is zero
            Force force = (Force) forces[index];
            if ( force.CheckState() )
            {
                // remove zero force
                forces.RemoveAt(index);
                Destroy(force);
            }
            else
            {
                // Update state velocity with the sum of all forces
                for (int i = 0; i < 3; i++) particle_states[i + 3] += force.Values[i];
            }
        }
    }

    
}
