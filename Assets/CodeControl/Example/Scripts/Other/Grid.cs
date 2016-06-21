using UnityEngine;
using System.Collections;

namespace CodeControl.Example
{
    public static class Grid
    {

        private const float GRID_SIZE = .5f;

        public static Vector3 GetPositionOnGrid(Vector3 position)
        {
            return new Vector3(Mathf.Round(position.x * (1.0f / GRID_SIZE)) / (1.0f / GRID_SIZE),
                               0.0f,
                               Mathf.Round(position.z * (1.0f / GRID_SIZE)) / (1.0f / GRID_SIZE));
        }

    }

}