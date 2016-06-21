using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class SpawnMode : AbstractMode
    {

        protected override void Awake()
        {
            base.Awake();
            turretCursor.SetType(TurretCursorType.Create);
        }

        protected override GameMode GetGameMode()
        {
            return GameMode.Spawn;
        }

        private void Update()
        {
            if (turretCursor.HoverGrid())
            {
                turretCursor.Show();
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceTurretMessage message = new PlaceTurretMessage();
                    message.Position = turretCursor.transform.position;
                    Message.Send<PlaceTurretMessage>(message);
                }
            }
            else {
                turretCursor.Hide();
            }
        }

        private void OnGameModeChange(GameModeMessage m)
        {
            enabled = m.Mode == GameMode.Spawn;
        }

    }
}