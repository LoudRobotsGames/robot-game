/// <copyright file="IWindowRenderer.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;

namespace CodeControl.Editor {

    public interface IWindowRenderer {
        Rect BoundingBox { get; }
        string Title { get; }

        void Deinit();
        void Update();
        void ShowContextMenu();
        void Render();
        void RenderMiniMap();
    }

}