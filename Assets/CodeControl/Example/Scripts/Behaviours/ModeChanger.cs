using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class ModeChanger : MonoBehaviour
    {

        MeshButton[] buttons;

        private void Awake()
        {
            buttons = GetComponentsInChildren<MeshButton>();
            foreach (MeshButton button in buttons)
            {
                button.OnClick += OnButtonClicked;
            }
        }

        private void OnDestroy()
        {
            GameModeMessage message = new GameModeMessage();
            message.Mode = GameMode.None;
            Message.Send<GameModeMessage>(message);
        }

        private void OnButtonClicked(MeshButton clickedButton)
        {
            GameMode newMode;

            if (clickedButton.IsSelected)
            {
                newMode = GameMode.None;
                clickedButton.SetSelected(false);
            }
            else {
                switch (clickedButton.ButtonName)
                {
                    case "target":
                        newMode = GameMode.Target;
                        break;
                    case "destroy":
                        newMode = GameMode.Delete;
                        break;
                    default:
                        newMode = GameMode.Spawn;
                        break;
                }

                foreach (MeshButton button in buttons)
                {
                    button.SetSelected(button == clickedButton);
                }
            }

            GameModeMessage message = new GameModeMessage();
            message.Mode = newMode;
            Message.Send<GameModeMessage>(message);
        }

    }
}