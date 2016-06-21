using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class CameraRotator : MonoBehaviour
    {

        private const float ROTATE_FALLOF = .9f;
        private const float ROTATE_SPEED = 2.0f;

        private float rotateSpeed;
        private float previousMouseX;
        private bool leftDragEnabled;

        private void Awake()
        {
            leftDragEnabled = true;
            Message.AddListener<GameModeMessage>(OnGameModeChange);
        }

        private void OnDestroy()
        {
            Message.RemoveListener<GameModeMessage>(OnGameModeChange);
        }

        private void OnGameModeChange(GameModeMessage m)
        {
            leftDragEnabled = m.Mode == GameMode.None;
        }

        private void Update()
        {
            HandleInput();

            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            rotateSpeed *= .9f;
        }

        private void HandleInput()
        {
            if ((leftDragEnabled && Input.GetMouseButtonDown(0)) || Input.GetMouseButtonDown(1))
            {
                previousMouseX = Input.mousePosition.x;
            }
            if ((leftDragEnabled && Input.GetMouseButton(0)) || Input.GetMouseButton(1))
            {
                rotateSpeed += ROTATE_SPEED * (Input.mousePosition.x - previousMouseX);
            }
            previousMouseX = Input.mousePosition.x;
        }
    }
}
