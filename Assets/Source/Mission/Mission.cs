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
    public MechBlueprint m_PlayerMech;
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
        //GameObject playerGO = GameObject.Instantiate<GameObject>(m_PlayerPrefab);
        GameObject playerGO = MechHelper.CreateMech(m_PlayerMech);
        playerGO.tag = "Player";

        MechUserControl userControl = playerGO.AddComponent<MechUserControl>();
        MechSystem centerTorso = MechHelper.GetPart(playerGO, MechSystem.SystemLocation.CenterTorso);
        userControl.Torso = playerGO.transform.FindDeepChild(m_PlayerMech.Core.RotatorJoint);
        userControl.TiltPivot = playerGO.transform.FindDeepChild(m_PlayerMech.Core.TiltJoint);
        userControl.m_RotationSpeed = 30;
        userControl.m_MinVerticalAngle = -6f;
        userControl.m_MaxVerticalAngle = 8f;
        userControl.m_MaxHorizontalAngle = 75f;

        // Set up camera and aim point
        Transform aimPoint = new GameObject("AimPoint").GetComponent<Transform>();
        aimPoint.SetParent(centerTorso.transform, false);
        aimPoint.localPosition = m_PlayerMech.Core.CockpitAimPoint;
        Transform camPoint = new GameObject("MainCamera").GetComponent<Transform>();
        camPoint.SetParent(aimPoint, false);
        Camera cockpitCamera = camPoint.gameObject.AddComponent<Camera>();
        cockpitCamera.transform.localPosition = m_PlayerMech.Core.CockpitCameraOffset;
        cockpitCamera.nearClipPlane = 0.5f;
        // Register them
        userControl.CockpitCamera = cockpitCamera;
        userControl.m_AimPoint = aimPoint;

        VisionSensor vision = playerGO.AddComponent<VisionSensor>();
        vision.VisionTagFiler = "Enemy";
        userControl.m_Sensor = vision;

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
