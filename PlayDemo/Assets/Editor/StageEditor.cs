using UnityEditor;
using UnityEngine;

public class StageEditor : EditorWindow
{
    public Grid grid;
    public Transform player;
    public Transform enemy;
    public StageData stage;
    public StagePlacer stagePlacer;

    [MenuItem("Tools/Stage Editor")]
    public static void Open()
    {
        StageEditor window = GetWindow<StageEditor>("Stage Editor");
        window.minSize = new Vector2(400, 300);
        window.maxSize = new Vector2(800, 600);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Stage Editor", EditorStyles.boldLabel);

        grid = (Grid)EditorGUILayout.ObjectField("Grid", grid, typeof(Grid), true);
        player = (Transform)EditorGUILayout.ObjectField("Player", player, typeof(Transform), true);
        enemy = (Transform)EditorGUILayout.ObjectField("Enemy", enemy, typeof(Transform), true);
        stagePlacer = (StagePlacer)EditorGUILayout.ObjectField("Stage Placer", stagePlacer, typeof(StagePlacer), true);
        
        GUILayout.Space(10);
        GUILayout.Label("Stage", EditorStyles.boldLabel);

        stage = (StageData)EditorGUILayout.ObjectField("StageData", stage, typeof(StageData), false);

        GUILayout.Space(10);


        if (GUILayout.Button("Save"))
        {
            if (!CheckInstances()) return;

            if (grid != null && stage != null)
                StageSerializer.Save(grid, player, enemy, stage); 
        }

        if (GUILayout.Button("Load"))
        {
            if (!CheckInstances()) return;

            if (grid != null && stage != null)
                StageSerializer.Load(
                    stage, grid, player, enemy,
                    stagePlacer.tiles,
                    stagePlacer.playerPrefab,
                    stagePlacer.enemyPrefab
                );
        }
    }

    private bool CheckInstances()
    {
        if (player == null){
            player = GameObject.Find("PlayerRoot")?.transform;
            if (player == null)
                Debug.LogError("PlayerRoot not found in scene.");
                return false;
        }
        if (enemy == null)
        {
            enemy = GameObject.Find("EnemyRoot")?.transform;
            if (enemy == null)
                Debug.LogError("EnemyRoot not found in scene.");
                return false;
        }
        if (grid == null){
            grid = GameObject.Find("Grid")?.GetComponent<Grid>();
            if (grid == null)
                Debug.LogError("Grid not found in scene.");
                return false;
        }
        if (stagePlacer == null){
            stagePlacer = GameObject.Find("Grid")?.GetComponent<StagePlacer>();
            if (stagePlacer == null)
                Debug.LogError("StagePlacer not found in scene.");
                return false;
        }
        if (stage == null){
            Debug.LogError("StageData is not assigned.");
            return false;
        }
        return true;
    }
}

