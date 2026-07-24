using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DrawAndRace.TrackEditor
{
    /// <summary>
    /// Captures 2D screen drawing gestures from mouse or touch input
    /// and emits raw point streams for track validation and 3D extrusion.
    /// </summary>
    public class TrackDrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Canvas Settings")]
        [SerializeField] private float _minDistanceBetweenPoints = 10f; // in screen pixels
        [SerializeField] private Color _lineColor = Color.cyan;
        [SerializeField] private float _lineWidth = 4f;

        private readonly List<Vector2> _rawScreenPoints = new List<Vector2>();
        private bool _isDrawing;

        public event Action OnDrawingStarted;
        public event Action<IReadOnlyList<Vector2>> OnDrawingUpdated;
        public event Action<IReadOnlyList<Vector2>> OnDrawingCompleted;

        public bool IsDrawing => _isDrawing;
        public IReadOnlyList<Vector2> RawScreenPoints => _rawScreenPoints;

        public void OnPointerDown(PointerEventData eventData)
        {
            ClearDrawing();
            _isDrawing = true;
            AddPoint(eventData.position);
            OnDrawingStarted?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDrawing) return;

            Vector2 currentPoint = eventData.position;
            if (_rawScreenPoints.Count == 0 || Vector2.Distance(_rawScreenPoints[_rawScreenPoints.Count - 1], currentPoint) >= _minDistanceBetweenPoints)
            {
                AddPoint(currentPoint);
                OnDrawingUpdated?.Invoke(_rawScreenPoints);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDrawing) return;
            _isDrawing = false;
            AddPoint(eventData.position);
            OnDrawingCompleted?.Invoke(_rawScreenPoints);
        }

        private void AddPoint(Vector2 screenPos)
        {
            _rawScreenPoints.Add(screenPos);
        }

        public void ClearDrawing()
        {
            _rawScreenPoints.Clear();
            _isDrawing = false;
        }
    }
}
