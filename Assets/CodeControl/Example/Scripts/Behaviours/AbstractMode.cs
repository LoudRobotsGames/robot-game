using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public abstract class AbstractMode : MonoBehaviour
    {

        protected TurretCursor turretCursor;

        protected virtual void Awake()
        {
            turretCursor = (GameObject.Instantiate(Resources.Load("TurretCursor")) as GameObject).GetComponent<TurretCursor>();
            turretCursor.transform.SetParent(transform);
            turretCursor.Hide();

            Message.AddListener<GameModeMessage>(OnGameModeChange);
            enabled = false;
        }

        protected virtual void OnDestroy()
        {
            Message.RemoveListener<GameModeMessage>(OnGameModeChange);
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable()
        {
            turretCursor.Hide();
        }

        protected abstract GameMode GetGameMode();

        private void OnGameModeChange(GameModeMessage m)
        {
            enabled = m.Mode == GetGameMode();
        }

    }
}