using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class TargetMode : AbstractMode
    {

        private TurretModel from;
        private TurretModel hovered;

        protected override void OnEnable()
        {
            base.OnEnable();
            Message.AddListener<TurretMessage>("click", OnTurretClicked);
            Message.AddListener<TurretMessage>("mouse_over", OnTurretMouseOver);
            Message.AddListener<TurretMessage>("mouse_exit", OnTurretMouseOut);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            from = null;
            Message.RemoveListener<TurretMessage>("click", OnTurretClicked);
            Message.RemoveListener<TurretMessage>("mouse_over", OnTurretMouseOver);
            Message.RemoveListener<TurretMessage>("mouse_exit", OnTurretMouseOut);
        }

        protected override GameMode GetGameMode()
        {
            return GameMode.Target;
        }

        private void Update()
        {
            if (hovered != null)
            {
                turretCursor.Show();
                return;
            }

            if (!turretCursor.HoverGrid())
            {
                turretCursor.Hide();
            }
            else {
                turretCursor.Show();
                if (from == null)
                {
                    turretCursor.SetType(TurretCursorType.Select);
                }
                else {
                    turretCursor.SetType(TurretCursorType.Target);
                }
            }
        }

        private void OnTurretClicked(TurretMessage m)
        {
            if (from == null)
            {
                from = m.TurretModel;
                turretCursor.transform.position = from.Position;
                turretCursor.SetType(TurretCursorType.Stop);
                return;
            }

            turretCursor.SetType(TurretCursorType.SelectHover);

            if (from == m.TurretModel)
            {
                m.TurretModel.TargetTurret.Model = null;
                m.TurretModel.NotifyChange();
                from = null;
                return;
            }

            from.TargetTurret.Model = m.TurretModel;
            from.NotifyChange();

            from = null;
        }

        private void OnTurretMouseOver(TurretMessage m)
        {
            hovered = m.TurretModel;
            turretCursor.transform.position = hovered.Position;
            if (m.TurretModel == from)
            {
                turretCursor.SetType(TurretCursorType.Stop);
            }
            else if (from == null)
            {
                turretCursor.SetType(TurretCursorType.SelectHover);
            }
            else {
                turretCursor.SetType(TurretCursorType.TargetHover);
            }
        }

        private void OnTurretMouseOut(TurretMessage m)
        {
            if (m.TurretModel == hovered)
            {
                hovered = null;
            }
        }

    }
}