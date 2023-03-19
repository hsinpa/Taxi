using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VehicleStruct {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 acceleration;
    public Vector3 velocity;

    public Vector3 front_left_wheel_local;
    public Vector3 front_right_wheel_local;
    public Vector3 back_left_wheel_local;
    public Vector3 back_right_wheel_local;

    public Vector3 front_left_wheel_world;
    public Vector3 front_right_wheel_world;
    public Vector3 back_left_wheel_world;
    public Vector3 back_right_wheel_world;
}

public enum WheelType { Front, Back}

public struct VehicleInput {
    public Vector2 axis;
    public float brake;

    public float up;
    public float down;
    public float left;
    public float right;
}
