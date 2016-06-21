using UnityEngine;
using System.Collections;

namespace CodeControl.Example
{
    public class Colorer : MonoBehaviour
    {

        public void SetColor(Color color)
        {
            GetComponent<Renderer>().material.SetColor("_BaseColor", color);
        }

    }
}
