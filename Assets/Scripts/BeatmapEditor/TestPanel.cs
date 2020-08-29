using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatmapEditor
{
    public class TestPanel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IDragHandler, IEndDragHandler
    {
        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("click");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("pointer enter");
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0f);
            Debug.Log("draggg");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("end drag|");
        }
    }
}
