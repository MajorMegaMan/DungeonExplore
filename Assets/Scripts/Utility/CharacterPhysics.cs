using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBB
{
    public static class CharacterPhysics
    {
        // returns the magnitude of velocity. ie. speed
        // heading will be the normalised velocity
        public static float FindSpeedHeading(Vector3 velocity, ref Vector3 heading)
        {
            float speed = velocity.magnitude;
            if (speed > 0)
            {
                heading = velocity / speed;
            }
            return speed;
        }


        // Very simple drag to stop velocity
        public static Vector3 AddDrag(Vector3 normalisedVelocity, float currentSpeed, float dragStrength, float deltaTime)
        {
            if (currentSpeed == 0.0f)
                return Vector3.zero;

            float dragDelta = dragStrength * deltaTime;
            if (currentSpeed - dragDelta < 0.0f)
            {
                dragDelta += currentSpeed - dragDelta;
            }
            Vector3 dragResult = normalisedVelocity * -dragDelta;
            return dragResult;
        }

        // returns the force delta that will result in acceleration being limited.
        // Useful for always accelerating conditions. such as a character.
        public static Vector3 LimitAcceleration(Vector3 velocity, Vector3 direction, float inputMagnitude, float accelerationStrength, float deltaTime, float maxSpeed, float absoluteMaxSpeed, float limitPullStrength = 1.0f, float overSpeedScaleStrength = 1.0f)
        {
            Vector3 nextVelocity = velocity + (direction * accelerationStrength * deltaTime);
            float nextSpeed = nextVelocity.magnitude;

            // Find the difference in speed from the desired max Speed to the nextSpeed
            //Vector3 targetVelocity = accelerationDirection * maxSpeed;
            float targetMagnitude = inputMagnitude * maxSpeed;

            float toTargetMagnitude = nextSpeed - targetMagnitude;

            // If the force is trying to go faster than the max speed.
            if (toTargetMagnitude > 0.0f)
            {
                // Trying to go faster than the limit
                // return negative direction * strength of decceleration
                return Limit(deltaTime, accelerationStrength, toTargetMagnitude, nextVelocity, nextSpeed, maxSpeed, absoluteMaxSpeed, limitPullStrength, overSpeedScaleStrength);
            }

            return Vector3.zero;
        }

        // Does not require input magnitude consideration. useful for on/off situations.
        public static Vector3 LimitVehicleAcceleration(Vector3 velocity, Vector3 forceDelta, float accelerationForce, float deltaTime, float maxSpeed, float absoluteMaxSpeed, float limitPullStrength = 1.0f, float overSpeedScaleStrength = 1.0f)
        {
            Vector3 nextVelocity = velocity + forceDelta;
            float nextSpeed = nextVelocity.magnitude;

            // Find the difference in speed from the desired max Speed to the nextSpeed
            float toTargetMagnitude = nextSpeed - maxSpeed;

            // If the force is trying to go faster than the max speed.
            if (toTargetMagnitude > 0.0f)
            {
                // Trying to go faster than the limit
                return Limit(deltaTime, accelerationForce, toTargetMagnitude, nextVelocity, nextSpeed, maxSpeed, absoluteMaxSpeed, limitPullStrength, overSpeedScaleStrength);
            }

            return Vector3.zero;
        }

        static Vector3 Limit(float deltaTime, float accelerationStrength, float toTargetMagnitude, Vector3 nextVelocity, float nextSpeed, float maxSpeed, float absoluteMaxSpeed, float limitPullStrength, float overSpeedScaleStrength)
        {
            // Trying to go faster than the limit

            // Find the velocity the body wants to be limited to.
            Vector3 targetVelDir = nextVelocity / nextSpeed;
            //Vector3 targetVelocity = targetVelDir * m_maxSpeed;

            // range of 0.0f to 1.0f based on how far current speed is past m_moveSpeed with m_absoluteSpeed as a max
            float overSpeedScale = (nextSpeed - maxSpeed) / (absoluteMaxSpeed - maxSpeed);

            float speedScale = Mathf.Max(overSpeedScale, 0.0f);

            float deccelerationFactor = Mathf.Min((1.0f + (speedScale * overSpeedScaleStrength)) * limitPullStrength * accelerationStrength * deltaTime, toTargetMagnitude);

            // Find if the force is adding past the absolute limit.
            float absoluteFactor = Mathf.Max(0.0f, (nextSpeed - absoluteMaxSpeed));
            deccelerationFactor = Mathf.Max(deccelerationFactor, absoluteFactor);

            // return negative direction * strength of decceleration
            return -targetVelDir * deccelerationFactor;
        }

        public static float CalculateJumpForce(float jumpHeight, float gravity)
        {
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            float verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            return verticalVelocity;
        }
    }
}
