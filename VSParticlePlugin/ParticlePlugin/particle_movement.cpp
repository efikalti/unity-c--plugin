#define EXPORT_API __declspec(dllexport) // Visual Studio needs to annotate exported functions with this
#include <cmath>
#include <random>
#include <iostream>

extern "C"
{
	const EXPORT_API void CentralGravity(float* particle_states,float* center,float dt)
    {
        float* x=&particle_states[0];
        float* v=&particle_states[3];
        
        float gravity[3]; //"gravity" here is just a direction from the particle's position to the specified center
        for(int d=0;d<3;d++){
            gravity[d]=center[d]-x[d];
        }

        float magnitude=std::sqrt(gravity[0]*gravity[0] + gravity[1]*gravity[1] + gravity[2]*gravity[2]);
        float epsilon=1e-10f;

        for(int d=0;d<3;d++){
            gravity[d]/=(magnitude+epsilon);
        }
        
        for(int d=0;d<3;d++){
            v[d]+=gravity[d]*dt;
        }
        for(int d=0;d<3;d++){
            x[d]+=v[d]*dt;
        }
    }

	// Add gravity force to particle
	const EXPORT_API void Gravity(float* particle_states, float* gravity, float dt)
	{
		float* v = &particle_states[3];

		for (int d = 0; d<3; d++) {
			v[d] += gravity[d] * dt;
		}
	}

	// Calculate and return the collision force
	const EXPORT_API void Reflect(float* particle_states, float* wall_normal, float* bounce_force)
	{
		float* v = &particle_states[3];
		float* n = &wall_normal[0];
		float*b = &bounce_force[0];

		float product = 0;
		// Dot product of v and n
		for (int d = 0; d<3; d++) {
			product += (v[d] * n[d]);
		}

		float bounce[3];
		// Calculate without loss
		for (int d = 0; d<3; d++) {
			b[d] = ((product * n[d]) + v[d]);
		}

		float magnitude = std::sqrt(b[0] * b[0] + b[1] * b[1] + b[2] * b[2]);

		for (int d = 0; d<3; d++) {
			b[d] /= (magnitude);
		}

		for (int d = 0; d<3; d++) {
			b[d] *= (-2);
		}

	}

	// Calculate and return the new position of this particle
	const EXPORT_API void Move(float* particle_states, float dt)
	{
		float* x = &particle_states[0];
		float* v = &particle_states[3];

		for (int d = 0; d<3; d++) {
			x[d] += v[d] * dt;
		}
	}
	
	// Calculate and return the distance of these vectors
	const EXPORT_API float Distance(float* a, float* b)
	{

		float distance = std::sqrt( std::pow(a[0] - b[0], 2) + std::pow(a[1] - b[1], 2) + std::pow(a[2] - b[2], 2) );
		return distance;
	}

	// Calculate and return the magnitude of this vector
	const EXPORT_API float Magnitude(float* vector)
	{
		float magnitude = 0;
		for (int d = 0; d < 3; d++) {
			magnitude += vector[d] * vector[d];
		}
		return std::sqrt(magnitude);
	}
	
	// Normalize vector
	const EXPORT_API void Normalize(float* vector)
	{
		float* v = &vector[0];
		float magnitude = Magnitude(v);
		for (int d = 0; d < 3; d++)  v[d] /= (float) magnitude;
	}

	// Limit vector to a max value
	const EXPORT_API void MaxLimit(float* vector, float max_value)
	{
		float* v = &vector[0];
		for (int d = 0; d < 3; d++) {
			if (v[d] < 0) {
				if (v[d] < -max_value) v[d] = -max_value;
			}
			else {
				if (v[d] > max_value) v[d] = max_value;
			}
		}
	}

	// Calculate and return separate force or nullptr if separate force is not applicable
	const EXPORT_API bool Separate(float* particle_states, float* target, float desired_separation)
	{
		float* x = &particle_states[0];
		float away_v[3];

		// Calculate distance of particles
		float distance = Distance(x, target);
		if ((distance > 0) && (distance < desired_separation)) {
			for (int d = 0; d < 3; d++) away_v[d] = 0;

			// Calculate vector pointing away from target
			for (int d = 0; d < 3; d++) away_v[d] = x[d] - target[d];

			// Normalize away vector
			Normalize(away_v);

			for (int d = 0; d < 3; d++) target[d] = away_v[d];
			return true;
		}

		return false;
	}

} // end of export C block
