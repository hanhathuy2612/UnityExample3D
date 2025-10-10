using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        public class SpawnWaveType
        {
            public int count = 5;
            public float spawnRate = 1f; // seconds between spawns
            public GameObject enemyPrefab;
        }

        public SpawnWaveType[] waves;
        public Transform[] spawnPoints;
        public int poolSizePerPrefab = 10;

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

        private void Start()
        {
            // create pools
            foreach (var w in waves)
            {
                if (w.enemyPrefab == null) continue;
                if (!_pools.ContainsKey(w.enemyPrefab))
                {
                    var q = new Queue<GameObject>();
                    for (var i = 0; i < poolSizePerPrefab; i++)
                    {
                        var newEnemy = Instantiate(w.enemyPrefab);
                        newEnemy.SetActive(false);
                        q.Enqueue(newEnemy);
                    }

                    _pools[w.enemyPrefab] = q;
                }
            }

            Debug.Log("EnemySpawner Start with " + _pools.Count + " pools");

            // start first wave for demo
            StartCoroutine(RunWaves());
        }

        IEnumerator RunWaves()
        {
            for (int i = 0; i < waves.Length; i++)
            {
                yield return StartCoroutine(SpawnWave(waves[i]));
                // wait between waves if you want
                yield return new WaitForSeconds(3f);
            }
        }

        IEnumerator SpawnWave(SpawnWaveType wave)
        {
            for (int i = 0; i < wave.count; i++)
            {
                Spawn(wave.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnRate);
            }
        }

        void Spawn(GameObject prefab)
        {
            if (prefab == null || spawnPoints.Length == 0) return;

            GameObject enemy = GetFromPool(prefab);
            if (enemy == null)
            {
                enemy = Instantiate(prefab);
            }

            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            enemy.transform.position = spawnPoint.position;
            enemy.transform.rotation = spawnPoint.rotation;
            enemy.SetActive(true);

            // reset health if using EnemyHealth
            var eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null) eh.currentHealth = eh.maxHealth;
        }

        GameObject GetFromPool(GameObject prefab)
        {
            if (!_pools.ContainsKey(prefab)) return null;
            var q = _pools[prefab];
            if (q.Count == 0) return null;
            var go = q.Dequeue();
            return go;
        }

        public void ReturnToPool(GameObject prefab, GameObject instance)
        {
            instance.SetActive(false);
            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = new Queue<GameObject>();
            }

            _pools[prefab].Enqueue(instance);
        }
    }
}