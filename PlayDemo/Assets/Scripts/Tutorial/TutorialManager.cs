using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    
    public int currentTutorialComponentId;
    
    private int timeStopId;
    public Transform[] tutorialTransforms;
    private bool isTutorialActive;

    private SpotlightHighlighter spotlightHighlighter;
    private bool canGotoNext;

    public TMP_Text tutorialText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    private void Start()
    {
        spotlightHighlighter = GetComponent<SpotlightHighlighter>();
    }

    public async UniTask ShowTutorial(TutorialComponent tc)
    {
        isTutorialActive = true;
        
        if (tc.isHighlighting)
        {
            spotlightHighlighter.HighlightWorld(tutorialTransforms[tc.highlightTransformId], 1);
        }
        
        tutorialText.gameObject.SetActive(true);
        tutorialText.rectTransform.anchoredPosition = tc.tutorialTextPos;
        tutorialText.text = tc.tutorialText;

        canGotoNext = false;
        
        InputAction action = InputSystem.actions.FindActionMap(tc.actionmapname).FindAction(tc.actionname);
        
        action.started += OnTutorialNext;
        InputSystem.actions.FindActionMap("Player").Disable();
        action.Enable();
        
        int timestopid = GameManager.Instance.TimeManager.EnterBulletTime(0);
        
        await UniTask.WaitUntil(() => canGotoNext, cancellationToken:this.GetCancellationTokenOnDestroy());
        
        GameManager.Instance.TimeManager.ExitBulletTime(timestopid);
        action.started -= OnTutorialNext;
        InputSystem.actions.FindActionMap("Player").Enable();
        
        tutorialText.gameObject.SetActive(false);
        
        spotlightHighlighter.Hide();
        isTutorialActive = false;
    }

    public void OnTutorialNext(InputAction.CallbackContext ctx)
    {
        canGotoNext = true;
    }
}
