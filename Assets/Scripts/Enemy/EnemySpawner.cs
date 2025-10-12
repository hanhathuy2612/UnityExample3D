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
        private readonly Dictionary<GameObject, EnemyHealth> _enemyHealths = new();

        private void Start()
        {
            // create pools
            foreach (var w in waves)
            {
                if (w.enemyPrefab == null) continue;
                if (_pools.ContainsKey(w.enemyPrefab)) continue;

                var q = new Queue<GameObject>();
                for (var i = 0; i < poolSizePerPrefab; i++)
                {
                    var newEnemy = Instantiate(w.enemyPrefab);
                    newEnemy.SetActive(false);
                    q.Enqueue(newEnemy);
                    _enemyHealths[newEnemy] = newEnemy.GetComponent<EnemyHealth>();
                }

                _pools[w.enemyPrefab] = q;
            }

            // start first wave for demo
            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            foreach (var wave in waves)
            {
                yield return StartCoroutine(SpawnWave(wave));
                // wait between waves if you want
                yield return new WaitForSeconds(3f);
            }
        }

        private IEnumerator SpawnWave(SpawnWaveType wave)
        {
            for (var i = 0; i < wave.count; i++)
            {
                Spawn(wave.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnRate);
            }
        }

        private void Spawn(GameObject prefab)
        {
            if (!prefab || spawnPoints.Length == 0) return;

            var enemy = GetFromPool(prefab);
            if (!enemy)
            {
                enemy = Instantiate(prefab);
            }

            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            enemy.transform.position = spawnPoint.position;
            enemy.transform.rotation = spawnPoint.rotation;
            enemy.SetActive(true);

            // reset health if using EnemyHealth
            _enemyHealths.TryGetValue(enemy, out var enemyHealth);
            if (enemyHealth) enemyHealth.currentHealth = enemyHealth.maxHealth;
        }

        private GameObject GetFromPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out _)) return null;
            var q = _pools[prefab];
            if (q.Count == 0) return null;
            var go = q.Dequeue();
            return go;
        }
    }
}