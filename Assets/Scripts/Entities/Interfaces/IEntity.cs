using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Entity is the base interface that most of the character systems will use.
// Also serves as a way for "Lock On" to work.
public interface IEntity
{
    public Vector3 position { get; }
    public float speed { get; }
    public Vector3 velocity { get; }
    public float currentSpeed { get; }
    public Vector3 heading { get; }

    // Bounds of the entity. Used by the player cam to test if the entity is on screen and should be locked on.
    Bounds GetAABB();

    // Mostly used for the player's HUD lock on reticle. The location of where the reticle should be positioned.
    // Could possibly also be the location that visual projectiles should travel to.
    Transform GetCameraLookTransform();

    // Radius of the entity for various range detection.
    // Will possibly be changed to a collider of some sort later, for more specfic detection.
    float GetTargetRadius();

    // Used for attack targeting.
    int GetTeam();
}
