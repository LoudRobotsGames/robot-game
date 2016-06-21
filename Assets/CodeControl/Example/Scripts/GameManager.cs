using UnityEngine;
using System.Collections;
using System.IO;
using CodeControl;

namespace CodeControl.Example
{
    public class GameManager : MonoBehaviour
    {

        private ModelBlobs savedBlobs;
        private GameObject logo;
        private GameObject loadProgression;
        private bool isLoading;

        private void Awake()
        {
            GameObject hud = GameObject.Instantiate(Resources.Load("GameHUD"), Vector3.zero, Quaternion.identity) as GameObject;
            logo = GameObject.Instantiate(Resources.Load("Logo"), Vector3.zero, Quaternion.identity) as GameObject;

            MeshButton[] buttons = hud.GetComponentsInChildren<MeshButton>();
            foreach (MeshButton button in buttons)
            {
                button.OnClick += OnButtonClicked;
            }

            if (Config.OPEN_WEBSITE_ENABLED)
            {
                logo.GetComponent<MeshButton>().OnClick += OnLogoClicked;
            }
            else {
                GameObject.Destroy(logo.GetComponent<MeshButton>());
            }
        }

        private void OnButtonClicked(MeshButton clickedButton)
        {
            if (isLoading) { return; }

            logo.SetActive(false);

            switch (clickedButton.ButtonName)
            {
                case "new":
                    NewGame();
                    break;
                case "load":
                    LoadGame();
                    break;
                case "save":
                    SaveGame();
                    break;
            }
        }

        private void OnLogoClicked(MeshButton clickedButton)
        {
            OpenWebsite();
        }

        private void OpenWebsite()
        {
            Application.OpenURL(Config.WEBSITE_URL);
        }

        private void NewGame()
        {
            Model.DeleteAll();

            GameModel gameModel = new GameModel();
            gameModel.Rockets = new ModelRefs<RocketModel>();

            LevelModel levelModel = new LevelModel();
            levelModel.Turrets = new ModelRefs<TurretModel>();
            gameModel.Level = new ModelRef<LevelModel>(levelModel);

            Controller.Instantiate<GameController>(gameModel);
        }

        private void LoadGame()
        {
            if (Config.SAVE_IN_DIRECTORY_ENABLED)
            {
                if (Directory.Exists(Config.SAVE_DIRECTORY))
                {
                    Model.DeleteAll();
                    isLoading = true;
                    Model.Load(Config.SAVE_DIRECTORY, OnStartLoad, OnLoadProgression, OnLoadDone, OnLoadError);
                }
            }
            else {
                if (savedBlobs != null)
                {
                    Model.DeleteAll();
                    isLoading = true;
                    Model.Load(savedBlobs, OnStartLoad, OnLoadProgression, OnLoadDone, OnLoadError);
                }
            }
        }

        private void OnStartLoad()
        {
            loadProgression = GameObject.Instantiate(Resources.Load("LoadProgression")) as GameObject;
        }

        private void OnLoadProgression(float progress)
        {
            loadProgression.transform.localScale = Vector3.one * MathHelper.EaseOutElastic(progress);
        }

        private void OnLoadDone()
        {
            isLoading = false;
            GameObject.Destroy(loadProgression);
            GameModel gameModel = Model.First<GameModel>();
            if (gameModel != null)
            {
                Controller.Instantiate<GameController>(gameModel);
            }
        }

        private void OnLoadError(string error)
        {
            GameObject.Destroy(loadProgression);
            Debug.LogError(error);
        }

        private void SaveGame()
        {
            if (Config.SAVE_IN_DIRECTORY_ENABLED)
            {
                Model.SaveAll(Config.SAVE_DIRECTORY);
            }
            else {
                savedBlobs = Model.SaveAll();
            }
        }

    }
}