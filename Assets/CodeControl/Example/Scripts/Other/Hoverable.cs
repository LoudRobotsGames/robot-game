using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CodeControl.Example
{
    public class Hoverable : MonoBehaviour
    {

        public Action OnMouseOver;
        public Action OnMouseExit;

        public bool IsMouseOver { get; private set; }

        private static List<Hoverable> instances = new List<Hoverable>();
        private static bool updated;

        private Collider[] colliders;

        private void Awake()
        {
            colliders = GetComponentsInChildren<Collider>();
            instances.Add(this);
        }

        private void OnDestroy()
        {
            instances.Remove(this);
        }

        private void OnDisable()
        {
            IsMouseOver = false;
        }

        private void SetMouseOver(bool isMouseOver)
        {
            if (IsMouseOver && !isMouseOver && OnMouseExit != null)
            {
                OnMouseExit();
            }
            else if (!IsMouseOver && isMouseOver && OnMouseOver != null)
            {
                OnMouseOver();
            }
            IsMouseOver = isMouseOver;
        }

        private void Update()
        {
            if (this != instances[0]) { return; }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                foreach (Hoverable hoverable in instances)
                {
                    bool isHovered = false;
                    foreach (Collider collider in hoverable.colliders)
                    {
                        if (hit.collider == collider)
                        {
                            isHovered = true;
                            break;
                        }
                    }
                    hoverable.SetMouseOver(isHovered);
                }
            }
            else {
                instances.ForEach(x => x.SetMouseOver(false));
            }
        }

    }
}