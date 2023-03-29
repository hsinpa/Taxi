using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField]
    private Camera m_follow_camera;

    [SerializeField]
    private Transform m_car_transform;

    [SerializeField]
    private Vector3 m_rest_camera_offset;

    [SerializeField]
    private Vector3 m_max_camera_offset;

    Vector3 m_lerp_position;
    Quaternion m_lerp_rotation;

    private void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (m_follow_camera != null && m_car_transform != null)
            UpdateCameraPosition();
    }

    void UpdateCameraPosition() {
        Vector3 car_position = transform.position;

        car_position -= (transform.forward * 4);
        car_position.y += 2.0f;

        m_lerp_position = Vector3.Lerp(m_lerp_position, car_position, 0.08f);

        m_follow_camera.transform.position = m_lerp_position;


        var look_at_position = m_lerp_position;
        look_at_position.y-= 1;
        Vector3 direction = (transform.position - look_at_position).normalized;

        m_lerp_rotation = Quaternion.Lerp(m_lerp_rotation, Quaternion.LookRotation(direction, Vector3.up), 0.15f);

        m_follow_camera.transform.rotation = m_lerp_rotation;
    }
}
