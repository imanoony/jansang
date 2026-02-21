using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TutorialComponent", menuName = "Scriptable Objects/TutorialComponent")]
public class TutorialComponent : ScriptableObject
{
    public int specialConditionForNextTutorial;
    public bool isHighlighting;
    public Vector3 highlightPosition;
    public int highlightTransformId;

    public float highlightPixel = 10;

    public string actionmapname;
    public string actionname;

    public InputActionPhase actionPhase;

    public Vector2 tutorialTextPos;
    public string tutorialText;

    public int nextCameraClamperId;
    public int wallToBeGone;
    
    public Vector2 tutorialImagePos;
    public bool isUsingImage;
    public Sprite tutorialImage;
}
