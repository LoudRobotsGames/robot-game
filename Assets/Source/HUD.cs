using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour
{
    private static HUD s_Instance;
    public static HUD Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindObjectOfType<HUD>();
            }
            return s_Instance;
        }
    }

    private GameObject m_Player;
    public GameObject Player
    {
        get
        {
            return m_Player;
        }
    }

    private GameObject m_SelectedEnemy;
    public GameObject SelectedEnemy
    {
        get
        {
            return m_SelectedEnemy;
        }
    }

    [SerializeField]
    private PaperDoll m_PlayerPaperDoll;
    [SerializeField]
    private PaperDoll m_EnemyPaperDoll;

    public PaperDoll PlayerHealth
    {
        get
        {
            return m_PlayerPaperDoll;
        }
    }

    public PaperDoll EnemyHealth
    {
        get
        {
            return m_EnemyPaperDoll;
        }
    }

    public void SelectEnemyMech(GameObject enemy)
    {
        if (m_SelectedEnemy != enemy)
        {
            m_EnemyPaperDoll.UpdateUI(enemy);
        }
        m_SelectedEnemy = enemy;
    }

    public void SetPlayerMech(GameObject player)
    {
        m_Player = player;
        m_PlayerPaperDoll.UpdateUI(player);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
