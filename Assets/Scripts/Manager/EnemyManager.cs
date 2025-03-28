using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private Coroutine waveRoutine;

    [SerializeField]
    private List<GameObject> enemyPrefabs; // 생성할 적 프리팹 리스트
    private Dictionary<string, GameObject> enemyPrefabsDic;

    [SerializeField]
    private List<Rect> spawnAreas; // 적을 생성할 영역 리스트

    [SerializeField]
    private Color gizmoColor = new Color(1, 0, 0, 0.3f); // 기즈모 색상

    private List<EnemyController> activeEnemies = new List<EnemyController>(); // 현재 활성화된 적들

    private bool enemySpawnComplite;

    [SerializeField] private float timeBetweenSpawns = 0.2f;
    [SerializeField] private float timeBetweenWaves = 1f;

    GameManager gameManager;

    [SerializeField] private List<GameObject> itemPrefabs;
    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        enemyPrefabsDic = new Dictionary<string, GameObject>();
        foreach (GameObject perfab in enemyPrefabs)
        {
            enemyPrefabsDic[perfab.name] = perfab;
        }
    }
    public void StartWave(int waveCount)
    {
        if(waveCount < 0)
        {
            gameManager.EndOfWave();
            return;
        }
        if (waveRoutine != null)
            StopCoroutine(waveRoutine);
        waveRoutine = StartCoroutine(SpawnWave(waveCount));
    }

    public void StopWave()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnWave(int waveCount) // 코루틴
    {
        enemySpawnComplite = false;
        yield return new WaitForSeconds(timeBetweenWaves);
        for (int i = 0; i < waveCount; i++)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
            SpawnRandomEnemy();
        }

        enemySpawnComplite = true;
    }

    private void SpawnRandomEnemy(string prefabName = null)
    {
        if (enemyPrefabs.Count == 0 || spawnAreas.Count == 0)
        {
            Debug.LogWarning("Enemy Prefabs 또는 Spawn Areas가 설정되지 않았습니다.");
            return;
        }

        // 랜덤한 적 프리팹 선택
        GameObject randomPrefab;
        if (prefabName == null)
        {
             randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        }

        else
        {
            randomPrefab = enemyPrefabsDic[prefabName]; 
        }

        // 랜덤한 영역 선택
        Rect randomArea = spawnAreas[Random.Range(0, spawnAreas.Count)];

        // Rect 영역 내부의 랜덤 위치 계산
        Vector2 randomPosition = new Vector2(
            Random.Range(randomArea.xMin, randomArea.xMax),
            Random.Range(randomArea.yMin, randomArea.yMax)
        );

        // 적 생성 및 리스트에 추가
        GameObject spawnedEnemy = Instantiate(randomPrefab, new Vector3(randomPosition.x, randomPosition.y), Quaternion.identity);
        EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
        enemyController.Init(this, gameManager.player.transform);
        activeEnemies.Add(enemyController);
    }

    // 기즈모를 그려 영역을 시각화 (선택된 경우에만 표시)
    private void OnDrawGizmosSelected()
    {
        if (spawnAreas == null) return;

        Gizmos.color = gizmoColor;
        foreach (var area in spawnAreas)
        {
            Vector3 center = new Vector3(area.x + area.width / 2, area.y + area.height / 2);
            Vector3 size = new Vector3(area.width, area.height);
            Gizmos.DrawCube(center, size);
        }
    }

    public void RemoveEnemyOnDeath(EnemyController enemy) // 에너미가 죽었을때 호출
    {
        activeEnemies.Remove(enemy);
        CreateRandomItem(enemy.transform.position); // 에너미가 죽으면 드롭 형태
        if (enemySpawnComplite && activeEnemies.Count == 0)
            gameManager.EndOfWave();

        
    }

    public void CreateRandomItem(Vector3 position)
    {
        GameObject item = Instantiate(itemPrefabs[Random.Range(0, itemPrefabs.Count)],position, Quaternion.identity);  
    }


    public void StartStage(StageInstance stageInstance)
    {
        if (waveRoutine != null)
            StopCoroutine(waveRoutine);

        waveRoutine = StartCoroutine(SpawnStart(stageInstance));
    }

    private IEnumerator SpawnStart(StageInstance stageInstance)
    {
        enemySpawnComplite = false;
        yield return new WaitForSeconds(timeBetweenWaves);

        WaveData waveData = stageInstance.currentStageInfo.waves[stageInstance.currentWave];
        for (int i = 0; i < waveData.monsters.Length; i++)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);

            MonsterSpawnData monsterSpawnData = waveData.monsters[i];
            for (int j = 0; j < monsterSpawnData.spawnCount; j++)
            {
                SpawnRandomEnemy(monsterSpawnData.monsterType);
            }
        }

        if (waveData.hasBoss)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);

            gameManager.MainCameraShake();
            SpawnRandomEnemy(waveData.bossType);
        }

        enemySpawnComplite = true;
    }
}
