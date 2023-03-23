using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Vehicle
{
    [CreateAssetMenu(fileName = "VehicleDataSRP", menuName = "SRP/VehicleDataSRP", order = 1)]
    public class VehicleDataSRP : ScriptableObject
    {
        public WheelConfig front_wheels;
        public WheelConfig back_wheels;

        public float friction= 0.01f;
        public float max_horse_power = 20;
        public float max_steering_force = 3;

        [System.Serializable]
        public struct WheelConfig {
            public AnimationCurve horse_power;

            public AnimationCurve grip_factor;
        }
    }
}