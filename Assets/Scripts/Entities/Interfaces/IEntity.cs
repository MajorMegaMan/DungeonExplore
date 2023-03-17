using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Entity is the base interface that most of the character systems will use.
// Also serves as a way for "Lock On" to work.
public interface IEntity
{
    public string entityName { get; }
    public Vector3 position { get; }
    public float speed { get; }
    public Vector3 velocity { get; }
    public float currentSpeed { get; }
    public Vector3 heading { get; }

    public EntityStats entityStats { get; }

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

    void ReceiveHit(IEntity attacker);

    #region UnityValidation
    // These Validation functions should be used when setting the entity objects within unity.
    // They simply ensure that the Entity being targeted is inheriting a unity object.

    // Tries to Find a Component that inherits from IEntity.
    static IEntity ValidateGameObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return null;
        }

        var entity = gameObject.GetComponent<IEntity>();
        if (entity != null)
        {
            return entity;
        }
        else
        {
            Debug.LogError("GameObject does not contain Component interface of IEntity. " + gameObject.name);
            return null;
        }
    }

    // Tests if IEntity is also a Component Type.
    static MonoBehaviour ValidateEntityAsMonobehaviour(IEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        var component = (entity as MonoBehaviour);
        if (component == null)
        {
            Debug.LogError("IEntity does not inherit from Monobehaviour. " + entity.entityName);
            return null;
        }

        return component;
    }
    #endregion // UnityValidation
}
