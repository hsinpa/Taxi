using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Hsinpa.Vehicle.VehicleDataSRP;

namespace Hsinpa.Vehicle
{
    [ExecuteInEditMode]
    public class VehicleView : MonoBehaviour
    {
        [SerializeField]
        Renderer m_render;

        [SerializeField]
        private float suspension_strength;

        [SerializeField]
        private float suspension_damper;

        [SerializeField, Range(0, 90)]
        private float angle_constraint = 30;

        [SerializeField, Range(0, 100)]
        private float forward_strength = 1;

        [SerializeField]
        private VehicleDataSRP vehicleDataSRP;

        private Bounds m_bounds;

        private Rigidbody rigidbody;
        private VehicleStruct vehicleStruct;
        //Local Position  
        private RaycastHit[] m_rayhit;
        private Ray m_ray;

        private const float WHEEL_RADIUS = 0.25f;
        private VehicleCtrl vehicleCtrl;

        void Start()
        {
            vehicleCtrl = GetComponent<VehicleCtrl>();
            rigidbody = GetComponent<Rigidbody>();
            vehicleStruct = new VehicleStruct();
            m_ray = new Ray();
            m_rayhit = new RaycastHit[1];
            CalculateLocalWheelPosition();
        }

        private async void CalculateLocalWheelPosition()
        {
            var original_rotation = this.transform.rotation;
            this.transform.rotation = Quaternion.identity;

            await Task.Delay(100);
            if (m_render != null)
                m_bounds = m_render.bounds;

            float floor = (m_bounds.center.y - m_bounds.extents.y) + (0.01f);

            vehicleStruct.front_right_wheel_local = (new Vector3(m_bounds.extents.x, floor, m_bounds.extents.z));
            vehicleStruct.front_left_wheel_local = (new Vector3(-m_bounds.extents.x, floor, m_bounds.extents.z));
            vehicleStruct.back_right_wheel_local = (new Vector3(m_bounds.extents.x, floor, -m_bounds.extents.z));
            vehicleStruct.back_left_wheel_local = (new Vector3(-m_bounds.extents.x, floor, -m_bounds.extents.z));

            this.transform.rotation = original_rotation;
        }

        private void ProcessSuspension(Vector3 tireWorldVel, RaycastHit rayhit, float suspensionRestDist, Vector3 position, Vector3 direction) {
            float rayDistance = Vector3.Distance(position, rayhit.point);
            float offset = suspensionRestDist - rayDistance;

            float vel = Vector3.Dot(direction, tireWorldVel);
            float force = ((offset * suspension_strength) - (vel * suspension_damper));
            this.rigidbody.AddForceAtPosition(direction * force, position);
        }

        private void ProcessAcceleration(Vector3 carVel, Vector3 position, Vector3 direction, float relative_angle, float forward_strength, float maxVelocity, AnimationCurve powerLookup) {
            if (forward_strength == 0) return;

            Vector3 acceleration_dir = Quaternion.AngleAxis(relative_angle, Vector3.up) * direction;

            float carSpeed = Vector3.Dot(direction, carVel);
            float final_force = powerLookup.Evaluate(Mathf.Clamp(carSpeed, 0, 1));

            //float normalizedSpeed = Mathf.Clamp01( Mathf.Abs(carSpeed) / carSpeed );

            this.rigidbody.AddForceAtPosition(acceleration_dir * (forward_strength * final_force), position);
        }

        private void ProcessSteering(Vector3 tireWorldVel, Vector3 steeringDir, Vector3 position, float maxVelocity, AnimationCurve gripLookup) {
            float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);

            //float gripFactor = 1;
            float gripFactor = gripLookup.Evaluate( Mathf.Clamp( Mathf.Abs(steeringVel) /  maxVelocity, 0, 1) );

            float desiredVelChange = -steeringVel * gripFactor;

            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
            float tireMass = 1;

            this.rigidbody.AddForceAtPosition(steeringDir * tireMass * desiredVelChange, position);
        }

        private void ProcessWheel(Vector3 virtual_position, Vector3 up_direction, WheelType wheelType, VehicleDataSRP.WheelConfig wheelConfig)
        {
            m_ray.origin = virtual_position;
            m_ray.direction = -up_direction;

            int hit = Physics.RaycastNonAlloc(m_ray, m_rayhit, WHEEL_RADIUS);
            if (hit <= 0) return;

            float rotate_angle = Mathf.Lerp(-angle_constraint, angle_constraint, vehicleCtrl.vehicleInput.axis.x + 1 * 0.5f);
            float scale_forward = Mathf.Lerp(-forward_strength, forward_strength, vehicleCtrl.vehicleInput.axis.y + 1 * 0.5f);
            Vector3 tireWorldVel = rigidbody.GetPointVelocity(virtual_position);

            Vector3 acceleration_dir = this.transform.forward;

            if (wheelType == WheelType.Front) {
                acceleration_dir = Vector3.Normalize(Quaternion.AngleAxis(rotate_angle, transform.up) * this.transform.forward);
            }
            else
                rotate_angle = 0;

            Vector3 side_dir = Vector3.Cross(transform.up, acceleration_dir);
            //side_dir.z = side_dir.z * -1;

            ProcessSuspension(tireWorldVel, m_rayhit[0], WHEEL_RADIUS, virtual_position, up_direction);

            ProcessSteering(tireWorldVel, side_dir, virtual_position, vehicleDataSRP.max_steering_force, wheelConfig.grip_factor);

            ProcessAcceleration(this.rigidbody.linearVelocity, virtual_position, this.transform.forward, relative_angle: rotate_angle, 
                                forward_strength: scale_forward * 3, maxVelocity: vehicleDataSRP.max_horse_power, powerLookup: wheelConfig.horse_power);
        }

        private VehicleStruct UpdateStructState(ref VehicleStruct vehicleStruct) {
            Matrix4x4 m = Matrix4x4.Rotate(transform.localRotation);
            Vector3 current_center_position = transform.position;

            vehicleStruct.front_right_wheel_world = current_center_position + m.MultiplyPoint3x4(vehicleStruct.front_right_wheel_local);
            vehicleStruct.front_left_wheel_world = current_center_position + m.MultiplyPoint3x4(vehicleStruct.front_left_wheel_local);

            vehicleStruct.back_right_wheel_world = current_center_position + m.MultiplyPoint3x4(vehicleStruct.back_right_wheel_local);
            vehicleStruct.back_left_wheel_world = current_center_position + m.MultiplyPoint3x4(vehicleStruct.back_left_wheel_local);

            return vehicleStruct;
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying) return;

            UpdateStructState(ref this.vehicleStruct);
            Vector3 direction_up = transform.up;

            if (vehicleCtrl.vehicleInput.axis.sqrMagnitude < 0.1f) {
                Vector3 newVelocity = rigidbody.linearVelocity - (rigidbody.linearVelocity * vehicleDataSRP.friction);

                if (newVelocity.magnitude < 0.05f)
                    newVelocity = newVelocity * 0;

                rigidbody.linearVelocity = newVelocity;
            }

            ProcessWheel(vehicleStruct.front_right_wheel_world, direction_up, WheelType.Front, vehicleDataSRP.front_wheels);
            ProcessWheel(vehicleStruct.front_left_wheel_world, direction_up, WheelType.Front, vehicleDataSRP.front_wheels);

            ProcessWheel(vehicleStruct.back_right_wheel_world, direction_up, WheelType.Back, vehicleDataSRP.back_wheels);
            ProcessWheel(vehicleStruct.back_left_wheel_world, direction_up, WheelType.Back, vehicleDataSRP.back_wheels);
        }

        private void OnDrawGizmos()
        {
            //Show front vehicleStruct
            if (!Application.isPlaying) this.vehicleStruct = UpdateStructState(ref this.vehicleStruct);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(this.vehicleStruct.front_right_wheel_world, WHEEL_RADIUS);
            Gizmos.DrawSphere(this.vehicleStruct.front_left_wheel_world, WHEEL_RADIUS);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(this.vehicleStruct.back_right_wheel_world, WHEEL_RADIUS);
            Gizmos.DrawSphere(this.vehicleStruct.back_left_wheel_world, WHEEL_RADIUS);
        }
    }
}