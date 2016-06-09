using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Init,
        Loading,
        StartMission,
        Playing,
        Pause,
        Lose,
        Win,

        COUNT
    }

    public Camera m_LoadingCamera;
    public Text m_LoadingText;
    public Image m_Crosshair;
    public float m_Timer = 0f;

    public Mission m_Mission;
    public GameState m_CurrentState = GameState.Init;

    // Use this for initialization
    void Start()
    {
        m_CurrentState = GameState.Loading;
        m_Crosshair.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        m_Timer += Time.deltaTime;
        switch(m_CurrentState)
        {
            case GameState.Loading:
                HUD.Instance.SelectEnemyMech(null);
                m_LoadingText.text = "Loading Game";
                if( m_Timer > 3f )
                {
                    m_LoadingText.text = "";
                    SetNextState(GameState.StartMission);
                }
                break;
            case GameState.StartMission:
                m_Mission.SpawnPlayer();
                m_Crosshair.enabled = true;
                m_Crosshair.CrossFadeAlpha(1, 1f, false);
                m_LoadingCamera.enabled = false;
                SetNextState(GameState.Playing);
                break;

            case GameState.Playing:
                if( m_Mission.IsMissionDone() )
                {
                    SetNextState(GameState.Win);
                }
                break;
        }
    }

    public void SetNextState(GameState nextState)
    {
        Debug.Log("Entering state: " + nextState.ToString());
        m_CurrentState = nextState;
        m_Timer = 0f;
    }
}
