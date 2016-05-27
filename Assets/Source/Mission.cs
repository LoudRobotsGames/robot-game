using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Mission : MonoBehaviour
{
    public static Mission CurrentMission = null;

    public Transform m_PlayerStartPoint;
    public Transform[] m_EnemySpawnPoints;
    public Transform[] m_EnemyPatrolPoints;
    public float m_EnemySpawnInterval = 1f;

    public GameObject m_PlayerPrefab;
    public GameObject m_EnemyPrefab;

    public Transform m_Player;
    public List<MechAIControl> m_EnemyAI = new List<MechAIControl>(32);

    private float m_Timer;

    public void StartMission()
    {
        CurrentMission = this;
    }

    public void FinishMission()
    {
        if (CurrentMission == this)
        {
            CurrentMission = null;
        }
    }

    public void Update()
    {
        // Only update if we are the active mission
        if (CurrentMission != this)
        {
            return;
        }
    }

    public void SpawnPlayer()
    {
        GameObject playerGO = GameObject.Instantiate<GameObject>(m_PlayerPrefab);

        m_Player = playerGO.GetComponent<Transform>();
        m_Player.position = m_PlayerStartPoint.position;
        m_Player.rotation = m_PlayerStartPoint.rotation;
        playerGO.name = "Player";

        HUD.Instance.SetPlayerMech(playerGO);
    }

    public void StartSpawningEnemies()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        int i = 0;
        WaitForSeconds wait = new WaitForSeconds(m_EnemySpawnInterval);
        while (i < m_EnemySpawnPoints.Length)
        {
            SpawnEnemy(m_EnemySpawnPoints[i], m_EnemyPatrolPoints[0].position);
            i++;
            yield return wait;
        }

    }

    private void SpawnEnemy(Transform spawnPoint, Vector3 patrolPoint)
    {
        GameObject enemyGO = GameObject.Instantiate<GameObject>(m_EnemyPrefab);

        Transform enemy = enemyGO.GetComponent<Transform>();
        enemy.position = spawnPoint.position;
        enemy.rotation = spawnPoint.rotation;
        m_EnemyAI.Add(enemy.GetComponent<MechAIControl>());
        enemyGO.name = "Enemy";

        AIBrain ai = enemy.GetComponent<AIBrain>();
        ai.AIData.MoveTarget = patrolPoint;
        ai.SetNextState(MoveToState.StaticState);

        HUD.Instance.SelectEnemyMech(enemyGO);
    }

    public bool IsMissionDone()
    {
        return false;
    }

    #region Event driven callbacks... game logic notifies when things happen so we can evaluate goals
    public void OnPartDestroyed(MechSystem system)
    {
        // If the system destroyed was the CenterTorso, then the whole mech is dead.
        if (system.m_SystemLocation == MechSystem.SystemLocation.CenterTorso)
        {
            OnMechDestroyed(system.m_RootMechObject);
        }
        else if(system.m_SystemLocation == MechSystem.SystemLocation.LeftLeg || system.m_SystemLocation == MechSystem.SystemLocation.RightLeg)
        {
            MechSystem other = null;
            if (system.m_SystemLocation == MechSystem.SystemLocation.LeftLeg)
            {
                other = MechHelper.GetPart(system.m_RootMechObject, MechSystem.SystemLocation.RightLeg);
            }
            else
            {
                other = MechHelper.GetPart(system.m_RootMechObject, MechSystem.SystemLocation.LeftLeg);
            }
            // If both legs are destroyed, then the mech is dead
            if (other.IsDestroyed())
            {
                OnMechDestroyed(system.m_RootMechObject);
            }
        }
    }

    public void OnMechDestroyed(GameObject mech)
    {
        if (HUD.Instance.SelectedEnemy == mech)
        {
            HUD.Instance.ShowEnemyHealth(false);
        }
    }
    #endregion
}
