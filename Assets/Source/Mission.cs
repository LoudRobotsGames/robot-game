using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Mission : MonoBehaviour
{
    public Transform m_PlayerStartPoint;
    public Transform[] m_EnemySpawnPoints;
    public float m_EnemySpawnInterval = 1f;

    public GameObject m_PlayerPrefab;
    public GameObject m_EnemyPrefab;

    public Transform m_Player;
    public List<MechAIControl> m_EnemyAI = new List<MechAIControl>(32);

    private float m_Timer;


    public void Update()
    {
        for (int i = 0; i < m_EnemyAI.Count; ++i)
        {
            if (m_EnemyAI[i] != null)
            {
                m_EnemyAI[i].SetTarget(m_Player);
            }
        }
    }

    public void SpawnPlayer()
    {
        GameObject playerGO = GameObject.Instantiate<GameObject>(m_PlayerPrefab);

        m_Player = playerGO.GetComponent<Transform>();
        m_Player.position = m_PlayerStartPoint.position;
        m_Player.rotation = m_PlayerStartPoint.rotation;
        playerGO.name = "Player";

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
            SpawnEnemy(m_EnemySpawnPoints[i]);
            i++;
            yield return wait;
        }
    }

    private void SpawnEnemy(Transform spawnPoint)
    {
        GameObject enemyGO = GameObject.Instantiate<GameObject>(m_EnemyPrefab);

        Transform enemy = enemyGO.GetComponent<Transform>();
        enemy.position = spawnPoint.position;
        enemy.rotation = spawnPoint.rotation;
        m_EnemyAI.Add(enemy.GetComponent<MechAIControl>());
        enemyGO.name = "Enemy";
    }

    public bool IsMissionDone()
    {
        return false;
    }

}
