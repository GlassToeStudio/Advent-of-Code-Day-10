using UnityEngine;

namespace GTS.AOC
{
    public class SimpleRotate : MonoBehaviour
    {
        private Vector3 axis;
        private float speed;

        private void Start()
        {
            axis = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            speed = Random.Range(20, 120);
        }

        private void Update()
        {
            transform.Rotate(axis, 100f * Time.deltaTime);
        }
    }
}