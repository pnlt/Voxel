using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InfimaGames.LowPolyShooterPack
{
    public class SnappedItem : MonoBehaviour
    {
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private RectTransform sampleItem;
        [SerializeField] private HorizontalLayoutGroup horizontalLayout;
        [SerializeField] private float velocity = 200;
        [SerializeField] private float snapForce;

        private ScrollRect scrollRect;
        private bool isSnapped;
        private float snapSpeed;
        
        private void Start()
        {
            isSnapped = false;
            scrollRect = GetComponent<ScrollRect>();
        }

        private void Update()
        {
            int currentItem =
                Mathf.RoundToInt(
                    (0 - contentPanel.localPosition.x / (sampleItem.rect.width + horizontalLayout.spacing)));

            if (scrollRect.velocity.magnitude < velocity && !isSnapped)
            {
                scrollRect.velocity = Vector2.zero;
                snapSpeed += snapForce * Time.deltaTime;
                contentPanel.localPosition =
                    new Vector3(
                        Mathf.MoveTowards(contentPanel.localPosition.x, 0 - (currentItem * (sampleItem.rect.width + horizontalLayout.spacing)), snapSpeed),
                        contentPanel.localPosition.y, 
                        contentPanel.localPosition.z);

                if (contentPanel.localPosition.x == 0 - (currentItem * (sampleItem.rect.width + horizontalLayout.spacing)))
                {
                    isSnapped = true;
                }
            }

            if (scrollRect.velocity.magnitude > velocity)
            {
                isSnapped = false;
                snapSpeed = 0;
            }
        }
    }

}
