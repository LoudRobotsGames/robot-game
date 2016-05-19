using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public interface ISensor
{
    void GetContacts(ref List<Transform> contacts);
}

