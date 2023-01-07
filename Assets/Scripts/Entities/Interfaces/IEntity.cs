using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    public float speed { get; }
    public Vector3 velocity { get; }
    public float currentSpeed { get; }
    public Vector3 heading { get; }
}
