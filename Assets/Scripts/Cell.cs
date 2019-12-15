using System;
using System.Collections;
using UnityEngine;

namespace GTS.AOC
{
    public class Cell : MonoBehaviour
    {
        [HideInInspector] public bool HasBeenHit { get; private set; }

        [HideInInspector] public Vector2 Coordinates = Vector2.zero;
        [HideInInspector] public Vector3 position;
        [SerializeField] private GameObject explosionPrefab;
        private Animator explosion;

        private CellType cellType = CellType.NOT_ASTEROID;
        private Renderer rend;
        private MaterialPropertyBlock block;
        private BaseStation baseStation;
        
        public void Init(CellType _type, Vector2 _coordinates, Vector3 _position)
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);
            float zRot = UnityEngine.Random.Range(0, 180);
            GameObject go = Instantiate(explosionPrefab, pos, Quaternion.Euler(0, 0, zRot), this.transform);
            explosion = go.GetComponent<Animator>();
            rend = GetComponentInChildren<Renderer>();
            block = new MaterialPropertyBlock();
            this.position = _position;
            this.Coordinates = _coordinates;
            SetType(_type);
        }

        public void JustCount()
        {
            HasBeenHit = true;
        }

        public void ResetHit()
        {
            HasBeenHit = false;
            ChangeToAsteroid();
        }

        public void GetHit()
        {
            HasBeenHit = true;
            block.SetColor("_Color", Color.red);
            block.SetColor("_EmissionColor", Color.red * 0.5f);
            rend.material.EnableKeyword("_EMISSION");
            rend.SetPropertyBlock(block);
        }

        public void HighLight()
        {
            block.SetColor("_Color", Color.yellow);
            block.SetColor("_EmissionColor", Color.yellow * 1.5f);
            rend.material.EnableKeyword("_EMISSION");
            rend.SetPropertyBlock(block);
        }

        public void Explode()
        {
            StartCoroutine(ExplodeRoutine());
        }

        private IEnumerator ExplodeRoutine()
        {
            explosion.SetTrigger("Explode");
            yield return new WaitForSeconds(1f);
            float time = 1.5f;
            explosion.transform.SetParent(null);
            while(time > 0)
            {
                time -= Time.deltaTime;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Clamp(scale.x -= .6f, 0f, 100);
                scale.y = Mathf.Clamp(scale.y -= .6f, 0f, 100);
                scale.z = Mathf.Clamp(scale.z -= .6f, 0f, 100);


                this.transform.localScale = scale;

                yield return null;
            }
            explosion.transform.SetParent(this.transform);
            yield return new WaitForSeconds(0.25f);
            Destroy(this.gameObject);
        }

        private void SetType(CellType type)
        {
            switch (type)
            {
                case CellType.ASTEROID:
                    ChangeToAsteroid();
                    break;
                case CellType.NOT_ASTEROID:
                    ChangeToNotAsteroid();
                    break;
                case CellType.BASE_STATION:
                    ChangeToBaseStation();
                    break;
                default:
                    ChangeToNotAsteroid();
                    break;
            }
        }

        private void ChangeToNotAsteroid()
        {
            cellType = CellType.NOT_ASTEROID;
            block.SetColor("_Color", Color.gray);
            rend.SetPropertyBlock(block);
            rend.material.DisableKeyword("_EMISSION");
            this.gameObject.layer = 0;
        }

        private void ChangeToAsteroid()
        {
            cellType = CellType.ASTEROID;
            
            this.gameObject.layer = 8;
            block.SetColor("_Color", Color.white);
            rend.material.DisableKeyword("_EMISSION");
            rend.SetPropertyBlock(block);
            if (baseStation != null)
            {
                Destroy(baseStation);
            }
        }

        private void ChangeToBaseStation()
        {
            cellType = CellType.BASE_STATION;
            this.gameObject.layer = 9;
            block.SetColor("_Color", Color.green);
            block.SetColor("_EmissionColor", Color.green * 0.5f);
            rend.material.EnableKeyword("_EMISSION");
            rend.SetPropertyBlock(block);
            baseStation = gameObject.AddComponent<BaseStation>();
        }

        private void OnMouseDown()
        {
            ChangeType();
        }

        private void ChangeType()
        {
            switch (cellType)
            {
                case CellType.ASTEROID:
                    ChangeToNotAsteroid();
                    break;
                case CellType.NOT_ASTEROID:
                    ChangeToBaseStation();
                    break;
                case CellType.BASE_STATION:
                    ChangeToAsteroid();
                    break;
                default:
                    ChangeToNotAsteroid();
                    break;
            }
        }
    }

    public enum CellType
    {
        ASTEROID,
        NOT_ASTEROID,
        BASE_STATION
    }
}