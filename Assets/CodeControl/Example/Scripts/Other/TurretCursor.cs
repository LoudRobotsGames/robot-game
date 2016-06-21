using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace CodeControl.Example
{
    public class TurretCursor : MonoBehaviour
    {

        [Serializable]
        private class CursorVisual
        {
            [SerializeField]
            public TurretCursorType Type;
            [SerializeField]
            public GameObject Visual;
        }

        [SerializeField]
        private List<CursorVisual> cursorVisuals = new List<CursorVisual>();

        public void SetType(TurretCursorType type)
        {
            cursorVisuals.ForEach(x => x.Visual.SetActive(x.Type == type));
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public bool HoverGrid()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f) && hit.collider.tag == "Level")
            {
                transform.position = Grid.GetPositionOnGrid(hit.point);
                return true;
            }
            else {
                return false;
            }
        }

    }
}
