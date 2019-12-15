using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace GTS.AOC
{
    public class BaseStation : MonoBehaviour
    {
        public float speed = .01f;
        private SearchType type = SearchType.LINEAR_SCAN_FAST;

        private List<Cell> allAsteroids = new List<Cell>();
        private List<Vector2> coords = new List<Vector2>();
        private LineRenderer lineRenderer;

        private int layerMask;
        private int numAsteroidToFind = 0;

        public event Action<int> OnCounterUpdated;

        public void Init(List<Cell> _AllAsteroids, SearchType searchType, float _speed, int _numAsteroidToFind)
        {
            allAsteroids = _AllAsteroids;
            numAsteroidToFind = _numAsteroidToFind;
            type = searchType;
            speed = _speed;
            layerMask = 1 << 8;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Color", Color.cyan);
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.SetPositions(new Vector3[2]);
            lineRenderer.startWidth = 0.25f;
            lineRenderer.endWidth = 0.25f;
            lineRenderer.SetPropertyBlock(block);

            
        }

        public void StartOnClick(SearchType _type)
        {
            type = _type;
            StartCoroutine(StartScan());
        }

        private IEnumerator StartScan()
        {
            yield return new WaitForSeconds(1f);

            switch (type)
            {
                case SearchType.RADIAL_SCAN_FAST:
                    RadialScanFast();
                    break;
                case SearchType.RADIAL_SCAN_ANIMATED:
                    StartCoroutine(RadialScanAnimated());
                    break;
                case SearchType.RADIAL_SCAN_AND_DESTROY:
                    RadialScanAndDestroy();
                    break;
                case SearchType.RADIAL_SCAN_AND_DESTROY_ANIMATED:
                    RadialScanAndDestroyAnimated();
                    break;
                case SearchType.LINEAR_SCAN_FAST:
                    LinearScanFast(allAsteroids);
                    break;
                case SearchType.LINEAR_SCAN_ANIMATED:
                    StartCoroutine(LinearScanAnimated(allAsteroids, false));
                    break;
                default:
                    Debug.LogError("No Type set!");
                    break;
            }
        }

        private List<Cell> RadialScanFast()
        {
            List<Cell> destroyQueue = new List<Cell>();
            speed *= 0.5f; // Add more granularity
            float revolve = 360;

            while (revolve > 0)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    Cell c = hit.transform.GetComponent<Cell>();

                    if (c.HasBeenHit == false)
                    {
                        c.GetHit();
                        coords.Add(c.Coordinates);
                        destroyQueue.Add(c);

                        if (coords.Count == numAsteroidToFind)
                        {
                            c.HighLight();
                            c.name = string.Format("{0}'th asteroid: {1}, {2}", numAsteroidToFind, c.Coordinates.x, c.Coordinates.y);
                            c.transform.SetParent(null);
                            Debug.Log(string.Format("Found the {0}'th asteroid!, {1}: {2}", numAsteroidToFind, c.Coordinates, (100 * c.Coordinates.x + c.Coordinates.y)));
                        }
                    }
                }

                revolve -= speed;
                transform.Rotate(Vector3.forward, -speed);
            }

            OnCounterUpdated?.Invoke(coords.Count);
            Debug.Log(coords.Count);
            return destroyQueue;
        }

        private IEnumerator RadialScanAnimated()
        {
            yield return new WaitForSeconds(0.5f);

            float revolve = 360;
            int counter = 0;
            while (revolve > 0)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, hit.transform.position);
                    Cell c = hit.transform.GetComponent<Cell>();

                    if (c.HasBeenHit == false)
                    {
                        c.GetHit();
                        coords.Add(c.Coordinates);

                        if (coords.Count == numAsteroidToFind)
                        {
                            c.HighLight();
                            c.name = string.Format("{0}'th asteroid: {1}, {2}", numAsteroidToFind, c.Coordinates.x, c.Coordinates.y);
                            c.transform.SetParent(null);
                            Debug.Log(string.Format("Found the {0}'th asteroid!, {1}: {2}", numAsteroidToFind, c.Coordinates, (100 * c.Coordinates.x + c.Coordinates.y)));
                        }

                        counter += 1;
                        OnCounterUpdated?.Invoke(counter);
                    }
                }
                else
                {
                    lineRenderer.SetPosition(1, transform.position + (transform.up * 200f));
                }

                revolve -= speed;
                transform.Rotate(Vector3.forward, -speed);
                yield return null;
            }

            lineRenderer.SetPositions(new Vector3[2]);
            Debug.Log(coords.Count);
        }

        private void RadialScanAndDestroy()
        {
            List<Cell> toDestroy = RadialScanFast();

            foreach (var asteroid in toDestroy)
            {
                Destroy(asteroid.gameObject);
            }

            toDestroy.Clear();
            allAsteroids.Clear();
            OnCounterUpdated?.Invoke(0);
        }

        private void RadialScanAndDestroyAnimated()
        {
            List<Cell> toDestroy = RadialScanFast();

            foreach (var asteroid in toDestroy)
            {
                asteroid.ResetHit();
            }
            StartCoroutine(LinearScanAnimated(toDestroy, true));
        }

        private void LinearScanFast(List<Cell> asteroidMap)
        {
            Vector3 dir = Vector3.zero;

            for (int i = 0; i < asteroidMap.Count; i++)
            {
                if (asteroidMap[i].HasBeenHit)
                {
                    continue;
                }

                dir = asteroidMap[i].position - transform.position;

                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    if (hit.transform.position == asteroidMap[i].position)
                    {
                        asteroidMap[i].GetHit();
                        coords.Add(asteroidMap[i].Coordinates);
                    }
                    else
                    {
                        Cell c = hit.transform.GetComponent<Cell>();
                        if (c.HasBeenHit == false)
                        {
                            c.GetHit();
                            coords.Add(c.Coordinates);
                        }
                    }
                }
            }

            OnCounterUpdated?.Invoke(coords.Count);
            Debug.Log(coords.Count);
        }

        private IEnumerator LinearScanAnimated(List<Cell> asteroidMap, bool destroy)
        {
            int counter = 0;

            Vector3 dir = Vector3.zero;
            lineRenderer.SetPosition(0, transform.position);

            for (int i = 0; i < asteroidMap.Count; i++)
            {
                if (asteroidMap[i].HasBeenHit)
                {
                    continue;
                }

                dir = asteroidMap[i].position - transform.position;

                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    lineRenderer.SetPosition(1, hit.transform.position);

                    if (hit.transform.position == asteroidMap[i].position)
                    {
                        counter += 1;
                        asteroidMap[i].GetHit();
                        if (destroy)
                        {
                            asteroidMap[i].Explode();
                        }
                        else
                        {
                            coords.Add(asteroidMap[i].Coordinates);
                        }
                    }
                    else
                    {
                        Cell c = hit.transform.GetComponent<Cell>();
                        lineRenderer.SetPosition(1, c.position);

                        if (c.HasBeenHit == false)
                        {
                            counter += 1;
                            c.GetHit();
                            if (destroy)
                            {
                                c.Explode();
                            }
                            else
                            {
                                coords.Add(asteroidMap[i].Coordinates);
                            }
                        }
                    }

                    OnCounterUpdated?.Invoke(counter);
                }
                
                
                if (destroy)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    yield return null;
                }
            }

            Debug.Log(coords.Count);
            allAsteroids.Clear();
            lineRenderer.SetPositions(new Vector3[2]);
        }
    }

    public enum SearchType
    {
        RADIAL_SCAN_FAST,
        RADIAL_SCAN_ANIMATED,
        RADIAL_SCAN_AND_DESTROY,
        RADIAL_SCAN_AND_DESTROY_ANIMATED,
        LINEAR_SCAN_FAST,
        LINEAR_SCAN_ANIMATED
    }
}