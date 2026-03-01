using UnityEngine;

namespace Project
{
    public class MinimapFollow : MonoBehaviour
    {
        public Transform target;
        public float height = 80f;

        private void LateUpdate()
        {
            if (target != null)
            {
                transform.position = new Vector3(target.position.x, target.position.y + height, target.position.z);
                transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Fasten perfectly top-down
            }
            else
            {
                // Fallback attempt to find player car if not explicitly assigned in inspector
                GameObject player = GameObject.Find("PlayerCar");
                if (player != null) target = player.transform;
            }
        }
    }
}