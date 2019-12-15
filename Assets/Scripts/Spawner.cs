using UnityEngine;
using System.Collections.Generic;

namespace GTS.AOC
{
    public class Spawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private List<Cell> asteroidPrefabs = new List<Cell>();
        [SerializeField] private Cell plainPrefab;

        [Header("Holders")]
        [SerializeField] private Transform asteroidHolder;
        [SerializeField] private Transform emptyHolder;

        [Header("Settings")]
        [SerializeField] private float asteroidOffset = 4f;
        public SearchType scanType = SearchType.LINEAR_SCAN_FAST;
        [Range(0.005f, 0.1f), Tooltip("0.01 Finds the correct solution")]
        [SerializeField] private float speed = 0.01f;
        [SerializeField] private int asteroidToFind = 200;
        [SerializeField] private Vector2 baseCoordiantes = Vector2.zero;

        private float distance_X = 0;
        private float distance_Y = 0;
        private BaseStation baseStation;

        private void Awake()
        {
            TextAsset Input = Resources.Load("day-10") as TextAsset;
            string text = Input.text.Trim();
            int size = (int)Mathf.Sqrt(text.Length);
            List<Cell> AllAsteroids = new List<Cell>();

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = Random.Range(0, asteroidPrefabs.Count);
                    if (text[y * size + x].Equals('#'))
                    {
                        Cell asteroid = Instantiate(asteroidPrefabs[index], new Vector3(distance_X, distance_Y, 0), Quaternion.identity, asteroidHolder);
                        asteroid.transform.GetChild(0).transform.rotation = Random.rotation;
                        asteroid.name = string.Format("Asteroid: {0},{1}", x, y);

                        if (x == baseCoordiantes.x && y == baseCoordiantes.y)
                        {
                            asteroid.transform.SetParent(null);
                            asteroid.name = string.Format("Base Station {0},{1}", x, y);
                            asteroid.Init(CellType.BASE_STATION, new Vector2(x, y), asteroid.transform.position);
                            baseStation = asteroid.GetComponent<BaseStation>();
                        }
                        else
                        {
                            asteroid.Init(CellType.ASTEROID, new Vector2(x, y), asteroid.transform.position);
                            AllAsteroids.Add(asteroid);
                        }
                    }

                    Cell plain = Instantiate(plainPrefab, new Vector3(distance_X, distance_Y, 0), Quaternion.identity, emptyHolder);
                    plain.Init(CellType.NOT_ASTEROID, new Vector2(x, y), plain.transform.position);
                    plain.name = string.Format("Empty: {0},{1}", x, y);

                    distance_X += asteroidOffset;
                }
                distance_X = 0;
                distance_Y -= asteroidOffset;
            }

            baseStation.Init(AllAsteroids, scanType, speed, asteroidToFind);
        }
    }
}