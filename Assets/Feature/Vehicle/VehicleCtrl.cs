using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Hsinpa.Vehicle
{
    public class VehicleCtrl : MonoBehaviour
    {
        private VehicleInput m_vehicleInput;
        public VehicleInput vehicleInput => m_vehicleInput;

        private GameplayAction inputActions;

        void Start()
        {
            inputActions = new GameplayAction();
            inputActions.Enable();

            inputActions.Control.Up.performed += OnDirectionKeyEvent;
            inputActions.Control.Up.canceled += OnDirectionKeyEvent;

            inputActions.Control.Down.performed += OnDirectionKeyEvent;
            inputActions.Control.Down.canceled += OnDirectionKeyEvent;

            inputActions.Control.Left.performed += OnDirectionKeyEvent;
            inputActions.Control.Left.canceled += OnDirectionKeyEvent;

            inputActions.Control.Right.performed += OnDirectionKeyEvent;
            inputActions.Control.Right.canceled += OnDirectionKeyEvent;

            inputActions.Control.Brake.performed += OnDirectionKeyEvent;
            inputActions.Control.Brake.canceled += OnDirectionKeyEvent;
        }

        private void OnDirectionKeyEvent(CallbackContext callbackContext) {
            float value = callbackContext.ReadValue<float>();

            if (callbackContext.action == inputActions.Control.Up) {
                m_vehicleInput.up = value;
            }

            if (callbackContext.action == inputActions.Control.Down)
            {
                m_vehicleInput.down = value;
            }

            if (callbackContext.action == inputActions.Control.Right)
            {
                m_vehicleInput.right = value;
            }

            if (callbackContext.action == inputActions.Control.Left)
            {
                m_vehicleInput.left = value;
            }

            if (callbackContext.action == inputActions.Control.Brake)
            {
                m_vehicleInput.brake = value;
            }

            m_vehicleInput.axis.y = m_vehicleInput.up - m_vehicleInput.down;
            m_vehicleInput.axis.x = m_vehicleInput.right - m_vehicleInput.left;
        }

    }
}