using UnityEngine;

namespace MissileBehaviours.Misc
{
    /// <summary>
    /// A very simple flying camera, very similar to the Unity scene camera.
    /// </summary>
    public class FlyingCamera : MonoBehaviour
    {
        [Tooltip("The sensitivity of the mouse input.")]
        public float sensitivity = 200;
        [Tooltip("How fast, in units per second, the camera moves.")]
        public float speed = 60;
        [Tooltip("How much faster the camera moves when the left shift key is pressed.")]
        public float shiftMultiplier = 3;

        float rotationY;
        float rotationX;
        float currentSpeed;

        void Start()
        {
            // Make sure the cursor is locked and hidden.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void FixedUpdate()
        {
            // Make sure the cursor is locked and hidden again after returning to the window.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // If the left shift key is pressed, we will move faster.
            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * shiftMultiplier : speed;

            // Get the mouse input.
            rotationY += Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity;
            rotationX += -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity;

            // Clamp the vertical rotation to avoid all the problems that can occur otherwise.
            rotationX = Mathf.Clamp(rotationX, -90, 90);

            // Rotate depending on the input we got earlier.
            transform.localEulerAngles = new Vector3(rotationX, rotationY, transform.localEulerAngles.z);

            // Move depending on input.
            transform.position += Input.GetAxisRaw("Horizontal") * Time.deltaTime * currentSpeed * transform.right;
            transform.position += Input.GetAxisRaw("Vertical") * Time.deltaTime * currentSpeed * transform.forward;
        }
    }
}
