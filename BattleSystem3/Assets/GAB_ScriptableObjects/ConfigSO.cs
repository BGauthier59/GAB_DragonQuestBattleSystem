using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Config", menuName = "Config")]
public class ConfigSO : ScriptableObject
{
    public EntitySO[] monsters;
}
