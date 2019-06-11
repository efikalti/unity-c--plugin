using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Force : MonoBehaviour {

    public float[] values;
    private bool dampen;
    private float dampenValue;

    public float[] Values
    {
        get
        {
            return values;
        }

        set
        {
            values = value;
        }
    }

    public bool Dampen
    {
        get
        {
            return dampen;
        }

        set
        {
            dampen = value;
        }
    }

    public float DampenValue
    {
        get
        {
            return dampenValue;
        }

        set
        {
            dampenValue = value;
        }
    }
    
    
    /*
     * Use this method to initialize the values for this force
     * if dampen is true the force will gradually decrease until it becomes zero
     */
    public void Set(float x, float y, float z, float dampenValue, bool dampen)
    {
        Values = new float[3];
        Values[0] = x;
        Values[1] = y;
        Values[2] = z;
        this.DampenValue = dampenValue;
        this.Dampen = dampen;
    }


    // Use this for initialization
    void Start () {
		
	}
	

	// Update is called once per frame
	void Update()
    {
        if (Dampen)
        {
            DampenForce();
        }
	}


    /*
     * Gradually decrease force values by DampenValue each time
     */
    private void DampenForce()
    {
        for (int i=0; i<3; i++)
        {
            if (Values[i] > 0)
            {
                Values[i] = Math.Max(Values[i] - DampenValue, 0);
            }
            else if (Values[i] < 0)
            {
                Values[i] = Math.Min(Values[i] + DampenValue, 0);
            }
        }
    }


    /*
     * Return true if the force is zero 
     */
    public bool CheckState()
    {
        return (Values[0] == 0 && Values[1] == 0 && Values[2] == 0);
    }
}
