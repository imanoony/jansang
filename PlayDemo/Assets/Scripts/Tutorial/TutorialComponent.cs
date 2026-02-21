using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "TutorialComponent", menuName = "Scriptable Objects/TutorialComponent")]
public class TutorialComponent : ScriptableObject
{
    public bool isHighlighting;
    public Vector3 highlightPosition;
    public int highlightTransformId;

    public string actionmapname;
    public string actionname;

    public Vector2 tutorialTextPos;
    public string tutorialText;
    
    public Vector2 tutorialImagePos;
    public bool isUsingImage;
    public Sprite tutorialImage;
}
