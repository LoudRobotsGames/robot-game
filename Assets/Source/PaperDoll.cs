using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PaperDoll : MonoBehaviour
{
    [SerializeField] private RawImage m_LeftLeg;
    [SerializeField] private RawImage m_RightLeg;
    [SerializeField] private RawImage m_CenterTorso;
    [SerializeField] private RawImage m_LeftTorso;
    [SerializeField] private RawImage m_RightTorso;
    [SerializeField] private RawImage m_LeftArm;
    [SerializeField] private RawImage m_RightArm;
    [SerializeField] private RawImage m_TopWeapon;
    [SerializeField] private RawImage m_Head;

    public Color m_HealthyColor = Color.green;
    public Color m_DamagedColor = Color.red;

    private List<MechSystem> m_SystemList = new List<MechSystem>(9);
    public void UpdateUI(GameObject mech)
    {
        m_SystemList.Clear();
        mech.transform.GetComponentsInChildren<MechSystem>(m_SystemList);

        for (int i = 0; i < m_SystemList.Count; ++i)
        {
            UpdateUIFromSystem(m_SystemList[i]);
        }
    }

    public void UpdateUIFromSystem(MechSystem system)
    {
        RawImage image = null;
        switch (system.m_SystemLocation)
        {
            case MechSystem.SystemLocation.RightLeg:
                image = m_RightLeg;
                break;
            case MechSystem.SystemLocation.LeftLeg:
                image = m_LeftLeg;
                break;
            case MechSystem.SystemLocation.CenterTorso:
                image = m_CenterTorso;
                break;
            case MechSystem.SystemLocation.LeftTorso:
                image = m_LeftTorso;
                break;
            case MechSystem.SystemLocation.RightTorso:
                image = m_RightTorso;
                break;
            case MechSystem.SystemLocation.LeftArm:
                image = m_LeftArm;
                break;
            case MechSystem.SystemLocation.RightArm:
                image = m_RightArm;
                break;
            case MechSystem.SystemLocation.TopWeapon:
                image = m_TopWeapon;
                break;
            case MechSystem.SystemLocation.Head:
                image = m_Head;
                break;
        }

        if (image != null)
        {
            image.CrossFadeColor(Color.Lerp(m_DamagedColor, m_HealthyColor, system.AssessDamage()), .75f, true, false);
        }
    }
}
