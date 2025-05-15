using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class EnemySpawnLogic
    {
        public bool isActive;
        [SerializeField] private GameObject enemyPrefab;

        [SerializeField] private Vector2 spawnTimerRange;
        private float _currentSpawnTimer;
        private float _currentSleepSpawnTimer;

        [SerializeField] private int enemyPoolSize;
        private readonly List<GameObject> _enemyPool = new();

        [SerializeField] private Collider2D spawnArea;
        [SerializeField] private Vector2 spawnMargin;

        internal void SetTimer()
        {
            _currentSpawnTimer = Random.Range(spawnTimerRange.x, spawnTimerRange.y);
        }

        internal void UpdateTimer()
        {
            if (_currentSleepSpawnTimer > 0)
                _currentSleepSpawnTimer.UpdateTimer();
            
            _currentSpawnTimer.UpdateTimer();

            if (_currentSpawnTimer <= 0)
            {
                if (_enemyPool.Count < enemyPoolSize)
                {
                    // create new enemy instance
                    SpawnEnemy();
                }
                else if (_enemyPool.Any(x => !x.activeSelf))
                {
                    // recycle enemy instance
                    SpawnEnemy( _enemyPool.Find(x => !x.activeSelf) );
                }
                else if (_currentSleepSpawnTimer <= 0)
                {
                    _currentSleepSpawnTimer = 1;
                }
            }
        }
        
        private void SpawnEnemy()
        {
            // can't spawn enemy, spawn point inside camera area
            if (!CalculateSpawnPosition(out var spawnPoint))
            {
                Debug.DrawRay(spawnPoint, Vector2.up, Color.red, 5);
                _currentSleepSpawnTimer = 1;
                return;
            }
            Debug.DrawRay(spawnPoint, Vector2.up, Color.green, 5);

            var enemyInstance = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
            _enemyPool.Add(enemyInstance);
            
            _currentSpawnTimer = Random.Range(spawnTimerRange.x, spawnTimerRange.y);
        }

        private void SpawnEnemy(GameObject instance)
        {
            // can't spawn enemy, spawn point inside camera area
            if (!CalculateSpawnPosition(out var spawnPoint))
            {
                _currentSleepSpawnTimer = 1;
                return;
            }
            
            instance.transform.position = spawnPoint;
            instance.SetActive(true);
            
            _currentSpawnTimer = Random.Range(spawnTimerRange.x, spawnTimerRange.y);
        }

        private bool CalculateSpawnPosition(out Vector2 spawnPoint)
        {
            var cameraBounds = GameLoopManager.Instance.CamRef.GetCameraBounds();
            var playerLookingRight = GameLoopManager.Instance.PlayerRef.IsFacingRight;
            
            var offCameraPoint = cameraBounds.center;
            Debug.DrawLine(offCameraPoint - Vector2.up*0.5f, offCameraPoint + Vector2.up*0.5f, Color.blue, 5);
            Debug.DrawLine(offCameraPoint - Vector2.right*0.5f, offCameraPoint + Vector2.right*0.5f, Color.blue, 5);
            
            offCameraPoint.x = (playerLookingRight) 
                ? cameraBounds.xMax
                : cameraBounds.xMin;
            
            offCameraPoint += (playerLookingRight) ? spawnMargin : -spawnMargin;
            
            spawnPoint = spawnArea.ClosestPoint(offCameraPoint);
            
            // returns if spawnpoint is usable or not
            return !cameraBounds.Contains(spawnPoint);
        }

        public void Reset()
        {
            foreach (var enemy in _enemyPool)
            {
                enemy.SetActive(false);
            }
        }
    }

    [SerializeField] private List<EnemySpawnLogic> spawnLogicList;

    private void OnEnable()
    {
        foreach (var spawnLogic in spawnLogicList)
        {
            spawnLogic.SetTimer();
        }

        GameLoopManager.OnPlayerDeadReset += OnPlayerDeadReset;
    }

    private void OnDisable()
    {
        GameLoopManager.OnPlayerDeadReset -= OnPlayerDeadReset;
    }

    private void Update()
    {
        foreach (var spawnLogic in spawnLogicList)
        {
            if (spawnLogic.isActive)
                spawnLogic.UpdateTimer();
        }
    }

    public void ToggleEnemySpawnLogic(int index)
    {
        Assert.IsTrue(index.IsInRange(0, spawnLogicList.Count), "Index is out of range.");
        
        spawnLogicList[index].isActive = !spawnLogicList[index].isActive;
        Debug.Log($"Enemy spawn logic {index} set to {spawnLogicList[index].isActive}");
    }

    private void OnPlayerDeadReset()
    {
        foreach (var spawnLogic in spawnLogicList)
        {
            spawnLogic.Reset();
        }
    }
}
