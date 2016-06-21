using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeControl;

namespace CodeControl.Example
{
    public static class TurretColors
    {

        public static Color GetRandom()
        {
            List<Color> colors = new List<Color>();
            colors.Add(new Color(176 / 255.0f, 23 / 255.0f, 31 / 255.0f));
            colors.Add(new Color(155 / 255.0f, 48 / 255.0f, 255 / 255.0f));
            colors.Add(new Color(72 / 255.0f, 118 / 255.0f, 255 / 255.0f));
            colors.Add(new Color(30 / 255.0f, 144 / 255.0f, 255 / 255.0f));
            colors.Add(new Color(51 / 255.0f, 161 / 255.0f, 201 / 255.0f));
            colors.Add(new Color(69 / 255.0f, 139 / 255.0f, 0 / 255.0f));
            colors.Add(new Color(105 / 255.0f, 139 / 255.0f, 34 / 255.0f));
            colors.Add(new Color(238 / 255.0f, 238 / 255.0f, 0 / 255.0f));
            colors.Add(new Color(255 / 255.0f, 215 / 255.0f, 0 / 255.0f));
            colors.Add(new Color(255 / 255.0f, 165 / 255.0f, 0 / 255.0f));
            colors.Add(new Color(139 / 255.0f, 90 / 255.0f, 0 / 255.0f));
            colors.Add(new Color(255 / 255.0f, 153 / 255.0f, 18 / 255.0f));
            colors.Add(new Color(255 / 255.0f, 69 / 255.0f, 0 / 255.0f));
            colors.Add(new Color(205 / 255.0f, 201 / 255.0f, 201 / 255.0f));
            colors.Add(new Color(142 / 255.0f, 142 / 255.0f, 142 / 255.0f));
            colors.Add(new Color(244 / 255.0f, 244 / 255.0f, 244 / 255.0f));

            List<TurretModel> turretModels = Model.GetAll<TurretModel>();
            foreach (TurretModel turretModel in turretModels)
            {
                colors.Remove(turretModel.Color);
            }

            if (colors.Count == 0)
            {
                return new Color(Random.value, Random.value, Random.value);
            }
            else {
                return colors[Random.Range(0, colors.Count)];
            }
        }

    }
}
