using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnData
    {
        public GameObject enemyPrefab;  // 생성할 적 프리팹
        public Transform spawnPoint;    // 생성할 적 위치
    }

    [Header("스폰 설정")]
    [SerializeField] private SpawnData[] spawnList;     // 생성할 위치 여러개 배열

    private void Start()
    {
        // 게임 시작 시 모든 스폰 포인트에 적 생성
        SpawnAllEnemies();
    }

    private void SpawnAllEnemies()
    {
        foreach(SpawnData data in spawnList)
        {
            if(data.enemyPrefab != null && data.spawnPoint != null)
            {
                Instantiate(data.enemyPrefab, data.spawnPoint.position, data.spawnPoint.rotation);
            }
        }

        Debug.Log("적 " + spawnList.Length + "마리 스폰");
    }
}
