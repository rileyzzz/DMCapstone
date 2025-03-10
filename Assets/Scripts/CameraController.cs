using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private InputAction m_CameraAction;
    private InputAction m_RotateLeft;
    private InputAction m_RotateRight;

    private Vector3 m_CameraVelocity;
    private Vector3 m_CameraVelocityVel;

    private int m_Rotation = 0;
    private float m_flRotation = 0.0f;
    private float m_flRotationVel = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_CameraAction = InputSystem.actions.FindAction("Player/Move");
        m_RotateLeft = InputSystem.actions.FindAction("Player/Previous");
        m_RotateRight = InputSystem.actions.FindAction("Player/Next");
    }

    // Update is called once per frame
    void Update()
    {
        var move = m_CameraAction.ReadValue<Vector2>();

        Vector3 moveDir = (transform.rotation * new Vector3(move.x, 0, move.y)) * 50.0f;
        m_CameraVelocity = Vector3.SmoothDamp(m_CameraVelocity, moveDir, ref m_CameraVelocityVel, 0.1f, 100.0f, Time.deltaTime);

        transform.position = transform.position + m_CameraVelocity * Time.deltaTime;

        if (m_RotateLeft.WasPressedThisFrame()) m_Rotation++;
        if (m_RotateRight.WasPressedThisFrame()) m_Rotation--;

        float targetRotation = m_Rotation * 45.0f;
        m_flRotation = Mathf.SmoothDamp(m_flRotation, targetRotation, ref m_flRotationVel, 0.1f, 360.0f, Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, m_flRotation, 0);
    }
}
