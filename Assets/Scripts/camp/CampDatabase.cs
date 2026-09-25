using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName="CampDatabase", menuName="Temple/Camp Database")]
public class CampDatabase : ScriptableObject
{
    public List<CampRoom> rooms = new List<CampRoom>();
}