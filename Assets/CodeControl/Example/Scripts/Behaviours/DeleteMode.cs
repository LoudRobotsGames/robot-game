using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class DeleteMode : AbstractMode
    {

        private TurretModel hovered;
        private bool deletedThisFrame;

        protected override void OnEnable()
        {
            base.OnEnable();

            turretCursor.SetType(TurretCursorType.Delete);

            Message.AddListener<TurretMessage>("click", OnTurretClicked);
            Message.AddListener<TurretMessage>("mouse_over", OnTurretMouseOver);
            Message.AddListener<TurretMessage>("mouse_exit", OnTurretMouseOut);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Message.RemoveListener<TurretMessage>("click", OnTurretClicked);
            Message.RemoveListener<TurretMessage>("mouse_over", OnTurretMouseOver);
            Message.RemoveListener<TurretMessage>("mouse_exit", OnTurretMouseOut);
        }

        protected override GameMode GetGameMode()
        {
            return GameMode.Delete;
        }

        private void Update()
        {
            if (turretCursor.HoverGrid() || hovered != null)
            {
                turretCursor.Show();
            }
            else {
                turretCursor.Hide();
            }

            deletedThisFrame = false;
        }

        private void OnTurretClicked(TurretMessage m)
        {
            if (deletedThisFrame) { return; } // Prevent click-trough
            m.TurretModel.Delete();
            hovered = null;
            turretCursor.SetType(TurretCursorType.Delete);
            deletedThisFrame = true;
        }

        private void OnTurretMouseOver(TurretMessage m)
        {
            hovered = m.TurretModel;
            turretCursor.SetType(TurretCursorType.DeleteHover);
            turretCursor.transform.position = m.TurretModel.Position;
        }

        private void OnTurretMouseOut(TurretMessage m)
        {
            hovered = null;
            turretCursor.SetType(TurretCursorType.Delete);
        }

    }
}