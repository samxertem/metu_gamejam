using UnityEngine;

namespace Project
{
    public class MinimapIcons : MonoBehaviour
    {
        public RectTransform minimapRect;
        public Camera minimapCam;

        [Header("Player Tracking")]
        public RectTransform playerIcon;
        public Transform player;

        [Header("Point B Tracking")]
        public RectTransform bIcon;
        public Transform pointB;

        [Header("Direction Arrow (points from player to B)")]
        public RectTransform bArrowIcon;

        private void Update()
        {
            if (minimapCam == null || minimapRect == null) return;

            UpdateIconPosition(player, playerIcon, false);
            UpdateBPointWithEdgeClamping();
        }

        private void UpdateIconPosition(Transform worldTarget, RectTransform icon, bool clampToEdge)
        {
            if (worldTarget == null || icon == null)
            {
                if (icon != null && icon.gameObject.activeSelf) icon.gameObject.SetActive(false);
                return;
            }

            if (!icon.gameObject.activeSelf) icon.gameObject.SetActive(true);

            Vector3 viewPos = minimapCam.WorldToViewportPoint(worldTarget.position);
            Vector2 iconPos = new Vector2(
                (viewPos.x - 0.5f) * minimapRect.sizeDelta.x,
                (viewPos.y - 0.5f) * minimapRect.sizeDelta.y
            );

            icon.anchoredPosition = iconPos;
        }

        private void UpdateBPointWithEdgeClamping()
        {
            if (pointB == null || bIcon == null) return;
            if (!bIcon.gameObject.activeSelf) bIcon.gameObject.SetActive(true);

            Vector3 viewPos = minimapCam.WorldToViewportPoint(pointB.position);

            // Check if B is within the minimap viewport (0-1 range with small margin)
            bool isVisible = viewPos.x > 0.05f && viewPos.x < 0.95f &&
                             viewPos.y > 0.05f && viewPos.y < 0.95f;

            Vector2 mapSize = minimapRect.sizeDelta;
            Vector2 rawPos = new Vector2(
                (viewPos.x - 0.5f) * mapSize.x,
                (viewPos.y - 0.5f) * mapSize.y
            );

            if (isVisible)
            {
                // B is visible on minimap — show dot at exact position, hide arrow
                bIcon.anchoredPosition = rawPos;
                if (bArrowIcon != null)
                    bArrowIcon.gameObject.SetActive(false);
            }
            else
            {
                // B is outside minimap — clamp to edge and show arrow
                float halfW = mapSize.x * 0.45f; // slight margin from edge
                float halfH = mapSize.y * 0.45f;

                // Direction from center to B position
                Vector2 dir = rawPos.normalized;
                
                // Clamp to the minimap rectangle edge
                // Find the intersection of the direction ray with the rectangle bounds
                float scaleX = dir.x != 0 ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
                float scaleY = dir.y != 0 ? halfH / Mathf.Abs(dir.y) : float.MaxValue;
                float scale = Mathf.Min(scaleX, scaleY);
                
                Vector2 edgePos = dir * scale;
                bIcon.anchoredPosition = edgePos;

                // Show and rotate direction arrow at the edge
                if (bArrowIcon != null)
                {
                    bArrowIcon.gameObject.SetActive(true);
                    bArrowIcon.anchoredPosition = edgePos;

                    // Rotate arrow to point outward (towards B)
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    bArrowIcon.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
                }
            }
        }
    }
}