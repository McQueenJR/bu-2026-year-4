using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CampRoom
{
    [Header("Room")]
    public string roomCode;        
    public string phoneNumber;     

    [Header("Residents")]
    public List<CampResident> residents = new List<CampResident>();
}