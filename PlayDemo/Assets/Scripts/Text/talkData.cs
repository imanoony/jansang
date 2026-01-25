using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "talk/talk_Data")]
public class talkData : ScriptableObject
{
    [TextArea]
    public List<string> randomLines;
    public float remainingTime;
}