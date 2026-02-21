using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    
    public int currentTutorialComponentId;

    public CameraFollow2D tutorialCamera;
    
    private int timeStopId;
    public Transform[] tutorialTransforms;
    public Collider2D[] cameraClampers;
    public GameObject[] wallsToNextSection;
    public bool isTutorialActive;

    private SpotlightHighlighter spotlightHighlighter;
    private bool canGotoNext;

    public TMP_Text tutorialText;

    public int currentSpecialConditionId;

    public TutorialComponent currentTc;

    public bool isSection2Cleared;

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
            spotlightHighlighter.HighlightWorld(tutorialTransforms[tc.highlightTransformId], tc.highlightPixel);
        }
        
        tutorialText.gameObject.SetActive(true);
        tutorialText.rectTransform.anchoredPosition = tc.tutorialTextPos;
        tutorialText.text = tc.tutorialText;

        canGotoNext = false;

        InputAction action = InputSystem.actions.FindActionMap(tc.actionmapname).FindAction(tc.actionname);
        if (tc.specialConditionForNextTutorial == 0)
        {

            if (tc.actionPhase != InputActionPhase.Canceled)
            {
                InputSystem.actions.FindActionMap("Player").Disable();
                action.Enable();
            }

            if (tc.actionPhase == InputActionPhase.Started) action.started += OnTutorialNext;
            if (tc.actionPhase == InputActionPhase.Performed) action.canceled += OnTutorialNext;
            if (tc.actionPhase == InputActionPhase.Canceled) action.canceled += OnTutorialNext;
        }

        int timestopid = GameManager.Instance.TimeManager.EnterBulletTime(0);
        currentTc = tc;
        
        await UniTask.WaitUntil(() => canGotoNext, cancellationToken:this.GetCancellationTokenOnDestroy());
        
        GameManager.Instance.TimeManager.ExitBulletTime(timestopid);

        if (tc.specialConditionForNextTutorial == 0)
        {
            if (tc.actionPhase == InputActionPhase.Started) action.started -= OnTutorialNext;
            if (tc.actionPhase == InputActionPhase.Performed) action.canceled -= OnTutorialNext;
            if (tc.actionPhase == InputActionPhase.Canceled) action.canceled -= OnTutorialNext;
        }

        if (tc.actionPhase != InputActionPhase.Canceled) InputSystem.actions.FindActionMap("Player").Enable();
        
        tutorialText.gameObject.SetActive(false);
        
        spotlightHighlighter.Hide();

        if (tc.nextCameraClamperId != 0) tutorialCamera.boundsCollider = cameraClampers[tc.nextCameraClamperId];
        if (tc.wallToBeGone != 0) wallsToNextSection[tc.wallToBeGone].SetActive(false);
        
        isTutorialActive = false;
    }

    public void OnTutorialNext(InputAction.CallbackContext ctx)
    {
        Debug.Log(currentTc.name);
        canGotoNext = true;
    }

    public void PlayerActualliySwap()
    {
        if (currentTc.specialConditionForNextTutorial == 1) canGotoNext = true;
    }

    public int attackTutorialClamper;
    public int attackTutorialWall;
    
    public void FirstBattleTutorialDone()
    {
        tutorialCamera.boundsCollider = cameraClampers[attackTutorialClamper];
        wallsToNextSection[attackTutorialWall].SetActive(false);
    }

    public int section2Monsters = 4;
    private int section2MonsterDead = 0;
    public int section2Clamper;
    public int section2Wall;

    public void KillSection2Monsters()
    {
        section2MonsterDead++;

        if (section2MonsterDead >= section2Monsters)
        {
            tutorialCamera.boundsCollider = cameraClampers[section2Clamper];
            wallsToNextSection[section2Wall].SetActive(false);
        }
    }
}
