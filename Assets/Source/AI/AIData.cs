using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AIData
{
    public Vector3 MoveTarget = Vector3.zero;
    public List<Transform> ContactList = new List<Transform>(32);
    public MechAIControl AICharacter;
    public ISensor Sensor;

    public Transform CurrentThreat;
    
    public void UpdateCurrentThreat()
    {
        if (ContactList.Count > 0)
        {
            if (CurrentThreat == null || CurrentThreat == ContactList[0])
            {
                // Acquired a new threat to target
                CurrentThreat = ContactList[0];
            }
            else
            {
                // Re-evaluate if the current threat is the best one
            }
        }
        else if (ContactList.Count == 0 && CurrentThreat != null)
        {
            // Lost the old threat
            CurrentThreat = null;
        }
    }

    public void UpdateContactList()
    {
        Sensor.GetContacts(ref ContactList);
    }
}
