using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class BirdParticleMotionScript : MonoBehaviour {
    
    public float r = 0.1f;
    public float maxforce = 0.3f;    // Maximum steering force
    public float maxspeed = 2;    // Maximum speed

    public float radius = 0.5f;


    // Private variables
    private float[] particle_states;
    private float[] acceleration;
    private float width = 20;
    private float height = 20;

    [DllImport("ParticlePlugin")]
    private static extern void Move(float[] particle_states, float dt);
    [DllImport("ParticlePlugin")]
    private static extern bool Separate(float[] particle_states, float[] target, float desiredseparation);
    [DllImport("ParticlePlugin")]
    private static extern float Magnitude(float[] vector);
    [DllImport("ParticlePlugin")]
    private static extern float Distance(float[] a, float[] b);
    [DllImport("ParticlePlugin")]
    private static extern void Normalize(float[] vector);
    [DllImport("ParticlePlugin")]
    private static extern void MaxLimit(float[] vector, float max_value);

    // Use this for initialization
    void Start () {
        // Initialize arrays for particle state
        particle_states = new float[6];
        acceleration = new float[3];

        // Set initial values
        for (int i = 0; i < 3; i++)
        {
            particle_states[i] = transform.position[i];
            particle_states[i + 3] = 0;
            acceleration[i] = 0;
        }
        // Set random starting velocity
        particle_states[3] = Random.Range(-0.1f, 0.1f);
        particle_states[4] = Random.Range(-0.1f, 0.1f);
        
    }
	

	// Update is called once per frame
	void Update ()
    {
        // Update velocity
        for (int i = 0; i < 3; i++)
        {
            particle_states[3 + i] += acceleration[i];

            // Reset acceleration
            acceleration[i] = 0;
        }

        // Move particle
        Move();

        // Wrap particle if it is out of screen bounds
        WrapAroundScreen();
    }

    /*
     * Add force to acceleration 
     */
    void ApplyForce(float[] force)
    {
        for (int i = 0; i < 3; i++) acceleration[i] += force[i];
    }

    /*
     * Accumulate a new acceleration each time based on three rules
     */
    public void flock(List<GameObject> birds)
    {

        float[] sep = Separate(birds);   // Separation
        float[] ali = Align(birds);      // Alignment
        float[] coh = Cohesion(birds);   // Cohesion

        // Arbitrarily weight these forces
        for (int i = 0; i < 3; i++)
        {
            sep[i] *= 1.5f;
            ali[i] *= 1f;
            coh[i] *= 1f;
        }

        // Add the force vectors to acceleration
        ApplyForce(sep);
        ApplyForce(ali);
        ApplyForce(coh);
        
    }

    /* Separation
    *  Method checks for nearby boids and steers away
    */
    float[] Separate(List<GameObject> birds)
    {
        float desiredseparation = 15.0f;
        int count = 0;
        
        float[] target = new float[3];
        float[] steer = new float[3];
        for (int i = 0; i < 3; i++) steer[i] = 0;
        float distance;

        foreach (GameObject other in birds)
        {
            for (int i = 0; i < 3; i++) target[i] = other.transform.position[i];

            distance = Distance(particle_states, target);
            if (Separate(particle_states, target, desiredseparation))
            {
                if (distance > 0)
                {
                    for (int i = 0; i < 3; i++) steer[i] += target[i] / distance;
                    count++;
                }
                else
                {
                    for (int i = 0; i < 3; i++) steer[i] += maxforce;
                }
            }
        }

        // Calculate average steer
        if (count > 0)
        {
            for (int i = 0; i < 3; i++) steer[i] /= count;
        }

        float magnitude = Magnitude(steer);
        if (magnitude > 0)
        {
            // Normalize
            Normalize(steer);
            // Implement Reynolds: Steering = Desired - Velocity
            for (int d = 0; d < 3; d++) steer[d] *= maxspeed;
            for (int d = 0; d < 3; d++) steer[d] -= particle_states[d+3];
            // Limit vector
            MaxLimit(steer, maxforce);
            //Debug.Log(transform.name + " After: " + steer[0] + ", " + steer[1] + ", " + steer[2]);
        }
        return steer;
    }

    /* Alignment
    *  For every nearby boid in the system, calculate the average velocity
    */
    float[] Align(List<GameObject> birds)
    {

        float neighbordist = 15.0f;
        int count = 0;
        float distance;

        float[] target = new float[3];
        float[] sum = new float[3];
        for (int i = 0; i < 3; i++) sum[i] = 0;

        foreach (GameObject other in birds)
        {
            for (int i = 0; i < 3; i++) target[i] = other.transform.position[i];
            // Get distance between particles
            distance = Distance(particle_states, target);

            if ((distance > 0) && (distance < neighbordist) && (distance > radius))
            {
                // Get velocity of target
                target = other.GetComponent<BirdParticleMotionScript>().GetVelocity();
                // Add target velocity to sum
                for (int i = 0; i < 3; i++) sum[i] += target[i];
                count++;
            }
        }
        
        if (count > 0)
        {
            for (int i = 0; i < 3; i++) sum[i] /= (float)count;

            // Implement Reynolds: Steering = Desired - Velocity
            Normalize(sum);
            for (int i = 0; i < 3; i++) sum[i] *= maxspeed;

            for (int i = 0; i < 3; i++) sum[i] -= particle_states[i+3];

            MaxLimit(sum, maxforce);
            
        }
        
        return sum;
    }

    /* Cohesion method
    *  For the average position (i.e. center) of all nearby boids, calculate steering vector towards that position
    */
    float[] Cohesion(List<GameObject> birds)
    {
        float neighbordist = 15.0f;
        float[] target = new float[3];
        float[] sum = new float[3];
        int count = 0;
        float distance;

        for (int i = 0; i < 3; i++) sum[i] = 0;

        foreach (GameObject other in birds)
        {
            for (int i = 0; i < 3; i++) target[i] = other.transform.position[i];
            // Get distance between particles
            distance = Distance(particle_states, target);
            if ((distance > 0) && (distance < neighbordist) && (distance > radius))
            {
                // Add target position to sum
                for (int i = 0; i < 3; i++) sum[i] += target[i];
                count++;
            }
        }
        if (count > 0)
        {
            for (int i = 0; i < 3; i++) sum[i] /= (float)count;
            return Seek(sum);  // Steer towards the position
        }
        return sum;
    }
    
    /*
     * A method that calculates and applies a steering force towards a target
     */
    float[] Seek(float[] target)
    {
        float[] desired = new float[3];
        // A vector pointing from the position to the target
        for (int i = 0; i < 3; i++) desired[i] = target[i] - particle_states[i];

        // Scale to maximum speed
        Normalize(desired);
        for (int i = 0; i < 3; i++) desired[i] *= maxspeed;

        // Steering = Desired minus Velocity
        for (int i = 0; i < 3; i++) desired[i] -= particle_states[i+3];
        MaxLimit(desired, maxforce);
        return desired;
    }

    /*
     * Calculate and apply new position 
     */
    void Move()
    {
        Move(particle_states, Time.deltaTime);
        transform.position = new Vector3(particle_states[0], particle_states[1], particle_states[2]);
    }

    /*
     * Set the width and height for screen wrap
     */
    public void SetWidthHeight(float width, float height)
    {
        this.height = height;
        this.width = width;
    }

    /*
     * Wrap the object if it gets out of camera limits
     */
    void WrapAroundScreen()
    {
        Vector3 pos = transform.position;
        if (transform.position.x < -width - r) pos.x = width - r;
        if (transform.position.y < -height - r) pos.y = height - r;
        if (transform.position.x > width + r) pos.x = -width + r;
        if (transform.position.y > height + r) pos.y = -height + r;
        transform.position = pos;
        UpdatePosition(transform.position);
    }

    /*
     * Update particle state_position array
     */
    void UpdatePosition(Vector3 position)
    {
        for (int i = 0; i < 3; i++) particle_states[i] = position[i];
    }

    /*
     * Returns the velocity of this particle 
     */
    public float[] GetVelocity()
    {
        float[] velocity = new float[3];
        for (int i = 0; i < 3; i++) velocity[i] = particle_states[i + 3];

        return velocity;
    }
}
