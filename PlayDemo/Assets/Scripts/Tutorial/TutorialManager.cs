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
    public RectTransform[] tutorialUiElements;
    public Collider2D[] cameraClampers;
    public GameObject[] wallsToNextSection;
    public GameObject[] tutorialTriggers;
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
    

    public Transform playerTransform;
    
    private void Start()
    {
        spotlightHighlighter = GetComponent<SpotlightHighlighter>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        StartTutorial().Forget();
    }

    public GameObject startTutorialTrigger;

    private async UniTask StartTutorial()
    {
        playerTransform.GetComponent<PlayerMovement2D>().disable = true;
        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken:this.GetCancellationTokenOnDestroy());
        playerTransform.GetComponent<PlayerMovement2D>().disable = false;
        startTutorialTrigger.SetActive(true);
    }
    

    public async UniTask ShowTutorial(TutorialComponent tc)
    {
        isTutorialActive = true;
        
        if (tc.isHighlighting )
        {
            if (tc.worldObjectHighlight) spotlightHighlighter.HighlightWorld(tutorialTransforms[tc.highlightTransformId], tc.highlightPixel);
            else  spotlightHighlighter.HighlightUI(tutorialUiElements[tc.highlightTransformId], tc.highlightPixel);
        }
        
        tutorialText.gameObject.SetActive(true);
        tutorialText.rectTransform.anchoredPosition = tc.tutorialTextPos;
        tutorialText.text = tc.tutorialText;

        canGotoNext = false;
        
        if (tc.playerDisabled) GameObject.FindWithTag("Player").GetComponent<PlayerMovement2D>().disable = false;

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
        if (tc.nextTutorialTriggerId != 0) tutorialTriggers[tc.nextTutorialTriggerId].SetActive(true);

        if (tc.thenFocusPlayer) tutorialCamera.SetTargetRoot(playerTransform);
        
        isTutorialActive = false;
    }

    public void OnTutorialNext(InputAction.CallbackContext ctx)
    {
        Debug.Log(currentTc.name);
        canGotoNext = true;
    }


    [Header("Section 1 - Swap tutorial")]
    private bool swapGood = false;
    public TutorialComponent swapGoodComponent;
    public int swappedMonsterDieHighlightIndex = 4;
    public void PlayerActualliySwap(Transform target)
    {
        if (swapGood) return;
        if (currentTc.specialConditionForNextTutorial == 1)
        {
            tutorialCamera.targetRoot = target;
            target.GetComponent<EnemyBase>().onDeath.AddListener(SwappedMonsterDie);
            canGotoNext = true;
            swapGood = true;
            tutorialTransforms[swappedMonsterDieHighlightIndex] = target;
        }
    }

    public void SwappedMonsterDie()
    {
        GameObject tf = new GameObject("TMP");
        tf.transform.position = tutorialTransforms[swappedMonsterDieHighlightIndex].position;
        tutorialTransforms[swappedMonsterDieHighlightIndex] = tf.transform;
        tutorialCamera.SetTargetRoot(tf.transform);
        ShowTutorial(swapGoodComponent).Forget();
    }

    public int attackTutorialClamper;
    public int attackTutorialWall;
    
    public void FirstBattleTutorialDone()
    {
        tutorialCamera.boundsCollider = cameraClampers[attackTutorialClamper];
        wallsToNextSection[attackTutorialWall].SetActive(false);
    }

    [Header("Section 2")]
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

    [Header("Section 3")]
    public int section3Monsters = 8;
    private int section3MonsterDead = 0;
    public int section3Clamper;
    public int section3Wall;

    public void KillSection3Monsters()
    {
        section3MonsterDead++;

        if (section3MonsterDead >= section3Monsters)
        {
            tutorialCamera.boundsCollider = cameraClampers[section3Clamper];
            wallsToNextSection[section3Wall].SetActive(false);
        }
    }

    private bool firstSwapHitMonster;

}
