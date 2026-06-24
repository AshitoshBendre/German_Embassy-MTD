using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ScrollCarousel
{
    public enum CarouselMode { Horizontal, Vertical, Infinite }

    public class Carousel : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        [Header("Scrolling Mode")]
        public CarouselMode Mode = CarouselMode.Horizontal;

        [Header("Items")]
        public List<RectTransform> Items = new List<RectTransform>();

        [Header("Position")]
        public int StartItem = 0;
        public float Itemspacing = 50f;

        [Header("Scale Falloff")]
        public float CenteredScale = 1f;
        public float MaxNonCenteredScale = 0.7f;
        public float MinNonCenteredScale = 0.1f;
        public float FalloffSlotCount = 3f;

        [Header("Rotation")]
        [SerializeField] public float MaxRotationAngle = 10f;
        [SerializeField] private float _rotationSmoothSpeed = 5f;

        [Header("Swipe Settings")]
        [SerializeField] private float _snapSpeed = 10f;
        [SerializeField] private float _swipeThreshold = 35f;
        [SerializeField] public float CircleRadius = 500f;

        [Header("Colors")]
        public bool ColorAnimation = false;
        public Color FocustedColor = Color.white;
        public Color NonFocustedColor = Color.gray;

        [SerializeField] private RectTransform _rectTransform;
        private int _currentItemIndex = 0;
        private bool _isSnapping = false;
        private float _currentRotationOffset = 0f;
        private Vector2 _totalDragDelta;
        private Dictionary<RectTransform, Coroutine> _activeColorAnimations = new();
        private List<(RectTransform item, float distance)> _depthSortBuffer = new();

        private void Awake() { if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>(); }
        private void OnEnable() { if (Items != null && Items.Count > 0) ForceUpdate(); }
        private void Start() { FocusItem(StartItem); ForceUpdate(); }
        private void Update() { if (_isSnapping) MoveToItem(); UpdateItemsAppearance(); }

        // --- MATH HELPER 1: Distance immune to Modulo wrapping ---
        private int GetSlotDistance(int indexA, int indexB)
        {
            int raw = Mathf.Abs(indexA - indexB);
            if (Mode == CarouselMode.Infinite && Items.Count > 0)
            {
                return Mathf.Min(raw, Items.Count - raw);
            }
            return raw;
        }

        // --- MATH HELPER 2: Scannable pixel distance grabber ---
        private float GetPhysicalDistanceFromCenter(int index)
        {
            Vector2 center = _rectTransform.rect.center;
            if (Mode == CarouselMode.Infinite)
            {
                float angle = (360f / Items.Count) * (index - _currentItemIndex) + _currentRotationOffset;
                return Mathf.Abs(Mathf.DeltaAngle(0, angle)) / (360f / Items.Count) * CircleRadius;
            }
            else if (Mode == CarouselMode.Vertical) return Mathf.Abs(Items[index].anchoredPosition.y - center.y);
            else return Mathf.Abs(Items[index].anchoredPosition.x - center.x);
        }

        private float GetTargetRestScale(int index)
        {
            int slotsAway = GetSlotDistance(index, _currentItemIndex);
            if (slotsAway == 0) return CenteredScale;
            if (slotsAway == 1) return MaxNonCenteredScale;

            float safeFalloff = Mathf.Max(0.01f, FalloffSlotCount);
            float t = Mathf.Clamp01((slotsAway - 1f) / safeFalloff);
            return Mathf.Lerp(MaxNonCenteredScale, MinNonCenteredScale, t);
        }

        private float GetItemSpacing(int index, bool isHorizontal)
        {
            float currentItemScale = GetTargetRestScale(index);
            float currentSize = isHorizontal ? Items[index].rect.width : Items[index].rect.height;
            currentSize *= currentItemScale;

            if (index + 1 >= Items.Count) return currentSize + Itemspacing;

            float nextItemScale = GetTargetRestScale(index + 1);
            float nextSize = isHorizontal ? Items[index + 1].rect.width : Items[index + 1].rect.height;
            nextSize *= nextItemScale;

            return (currentSize + nextSize) / 2f + Itemspacing;
        }

        private float GetTotalOffset(int index, bool isHorizontal)
        {
            float offset = 0f;
            int startIdx = Math.Min(index, _currentItemIndex);
            int endIdx = Math.Max(index, _currentItemIndex);

            for (int i = startIdx; i < endIdx; i++) offset += GetItemSpacing(i, isHorizontal);

            if (isHorizontal) return index < _currentItemIndex ? -offset : offset;
            else return index < _currentItemIndex ? offset : -offset;
        }

        private void PositionItems(bool animate = true)
        {
            if (Items.Count == 0) return;
            Vector2 center = _rectTransform.rect.center;
            float targetTime = animate ? Time.deltaTime * _snapSpeed : 1f;

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == null) continue;

                Vector2 targetPosition;
                if (Mode == CarouselMode.Infinite)
                {
                    float angle = (360f / Items.Count) * (i - _currentItemIndex);
                    float rad = angle * Mathf.Deg2Rad;
                    targetPosition = new Vector2(center.x + Mathf.Sin(rad) * CircleRadius, center.y + (1 - Mathf.Cos(rad)) * CircleRadius * 0.5f);
                }
                else if (Mode == CarouselMode.Vertical) targetPosition = new Vector2(center.x, center.y + (GetTotalOffset(i, false)));
                else targetPosition = new Vector2(center.x + (GetTotalOffset(i, true)), center.y);

                Items[i].anchoredPosition = animate ? Vector2.Lerp(Items[i].anchoredPosition, targetPosition, targetTime) : targetPosition;
            }
        }

        private void UpdateItemsAppearance()
        {
            if (Items.Count == 0) return;
            Vector2 center = _rectTransform.rect.center;

            // 1. Find the item closest to the crosshairs right now (Neighbor A)
            int closestIdx = 0;
            float minDistA = float.MaxValue;
            _depthSortBuffer.Clear();

            for (int i = 0; i < Items.Count; i++)
            {
                if (!Items[i]) continue;
                float dist = GetPhysicalDistanceFromCenter(i);
                _depthSortBuffer.Add((Items[i], dist));

                if (dist < minDistA)
                {
                    minDistA = dist;
                    closestIdx = i;
                }
            }

            // 2. Find which of its two immediate side-kicks is the "second closest" (Neighbor B)
            int secondClosestIdx = closestIdx;
            float minDistB = float.MaxValue;

            int leftIdx = (Mode == CarouselMode.Infinite) ? (closestIdx - 1 + Items.Count) % Items.Count : closestIdx - 1;
            if (leftIdx >= 0 && leftIdx < Items.Count)
            {
                float d = GetPhysicalDistanceFromCenter(leftIdx);
                if (d < minDistB) { minDistB = d; secondClosestIdx = leftIdx; }
            }

            int rightIdx = (Mode == CarouselMode.Infinite) ? (closestIdx + 1) % Items.Count : closestIdx + 1;
            if (rightIdx >= 0 && rightIdx < Items.Count)
            {
                float d = GetPhysicalDistanceFromCenter(rightIdx);
                if (d < minDistB) { minDistB = d; secondClosestIdx = rightIdx; }
            }

            // 3. The golden ratio of the drag (0.0 means dead centered on A, 1.0 means arrived at B)
            float dragLerp = (minDistA + minDistB > 0.001f) ? (minDistA / (minDistA + minDistB)) : 0f;

            float slotDistance = (Mode == CarouselMode.Infinite) ? CircleRadius : GetItemSpacing(0, Mode == CarouselMode.Horizontal);
            if (slotDistance == 0) slotDistance = 1f;

            for (int i = 0; i < Items.Count; i++)
            {
                if (!Items[i]) continue;

                // --- THE PURE INDEX SCALE CALCULATOR ---
                float distToA = GetSlotDistance(i, closestIdx);
                float distToB = GetSlotDistance(i, secondClosestIdx);
                float mathSlotsAway = Mathf.Lerp(distToA, distToB, dragLerp);

                float targetScale;
                if (mathSlotsAway <= 1f)
                {
                    targetScale = Mathf.Lerp(CenteredScale, MaxNonCenteredScale, mathSlotsAway);
                }
                else
                {
                    float safeFalloff = Mathf.Max(0.01f, FalloffSlotCount);
                    float falloffProgress = Mathf.Clamp01((mathSlotsAway - 1f) / safeFalloff);
                    targetScale = Mathf.Lerp(MaxNonCenteredScale, MinNonCenteredScale, falloffProgress);
                }

                Vector3 newScale = new Vector3(targetScale, targetScale, 1f);
                if (!float.IsNaN(newScale.x)) Items[i].localScale = newScale;

                // --- ROTATION ---
                float pxDist = GetPhysicalDistanceFromCenter(i);
                float normDist = Mathf.Clamp01(pxDist / slotDistance);

                if (Mode == CarouselMode.Vertical)
                {
                    float rotSign = (Items[i].anchoredPosition.y > center.y) ? -1f : 1f;
                    float rotX = MaxRotationAngle * normDist * rotSign;
                    if (!float.IsNaN(rotX)) Items[i].localRotation = Quaternion.Slerp(Items[i].localRotation, Quaternion.Euler(rotX, 0, 0), Time.deltaTime * _rotationSmoothSpeed);
                }
                else
                {
                    float rotSign = (Items[i].anchoredPosition.x > center.x) ? 1f : -1f;
                    float rotY = MaxRotationAngle * normDist * rotSign;
                    if (!float.IsNaN(rotY)) Items[i].localRotation = Quaternion.Slerp(Items[i].localRotation, Quaternion.Euler(30, rotY, 0), Time.deltaTime * _rotationSmoothSpeed);
                }
            }

            _depthSortBuffer.Sort(static (a, b) => b.distance.CompareTo(a.distance));
            foreach (var entry in _depthSortBuffer) entry.item.SetAsLastSibling();

            if (ColorAnimation && closestIdx != -1)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == null) continue;
                    StartColorAnimation(Items[i], (i == closestIdx) ? FocustedColor : NonFocustedColor);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData) { _isSnapping = false; _totalDragDelta = Vector2.zero; }

        public void OnDrag(PointerEventData eventData)
        {
            if (Items.Count == 0) return;
            _totalDragDelta += eventData.delta;

            if (Mode == CarouselMode.Infinite)
            {
                _currentRotationOffset += (eventData.delta.x / CircleRadius) * 45f;
                Vector2 center = _rectTransform.rect.center;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == null) continue;
                    float rad = ((360f / Items.Count) * (i - _currentItemIndex) + _currentRotationOffset) * Mathf.Deg2Rad;
                    Items[i].anchoredPosition = new Vector2(center.x + Mathf.Sin(rad) * CircleRadius, center.y + (1 - Mathf.Cos(rad)) * CircleRadius * 0.5f);
                }
            }
            else if (Mode == CarouselMode.Vertical)
            {
                float dragFactor = ((_currentItemIndex == 0 && eventData.delta.y < 0) || (_currentItemIndex == Items.Count - 1 && eventData.delta.y > 0)) ? 0.35f : 1f;
                foreach (RectTransform item in Items) if (item != null) item.anchoredPosition += new Vector2(0, eventData.delta.y * dragFactor);
            }
            else
            {
                float dragFactor = ((_currentItemIndex == 0 && eventData.delta.x > 0) || (_currentItemIndex == Items.Count - 1 && eventData.delta.x < 0)) ? 0.35f : 1f;
                foreach (RectTransform item in Items) if (item != null) item.anchoredPosition += new Vector2(eventData.delta.x * dragFactor, 0);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Items.Count == 0) return;

            if (Mode == CarouselMode.Infinite)
            {
                float closestDist = float.MaxValue; int closestIdx = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == null) continue;
                    float dist = Mathf.Abs(Mathf.DeltaAngle(0, (360f / Items.Count) * (i - _currentItemIndex) + _currentRotationOffset));
                    if (dist < closestDist) { closestDist = dist; closestIdx = i; }
                }
                FocusItem(closestIdx);
            }
            else if (Mode == CarouselMode.Vertical)
            {
                if (_totalDragDelta.y > _swipeThreshold) GoToNext();
                else if (_totalDragDelta.y < -_swipeThreshold) GoToPrevious();
                else FocusItem(_currentItemIndex);
            }
            else
            {
                if (_totalDragDelta.x < -_swipeThreshold) GoToNext();
                else if (_totalDragDelta.x > _swipeThreshold) GoToPrevious();
                else FocusItem(_currentItemIndex);
            }
            _currentRotationOffset = 0f;
        }

        private void MoveToItem()
        {
            PositionItems(true);
            if (Items.Count == 0 || Items[_currentItemIndex] == null) return;

            float currentPos = (Mode == CarouselMode.Vertical) ? Items[_currentItemIndex].anchoredPosition.y : Items[_currentItemIndex].anchoredPosition.x;
            float targetCenter = (Mode == CarouselMode.Vertical) ? _rectTransform.rect.center.y : _rectTransform.rect.center.x;

            if (Mathf.Abs(currentPos - targetCenter) < 0.1f) { _isSnapping = false; PositionItems(false); }
        }

        public void FocusItem(RectTransform item) => FocusItem(Items.IndexOf(item));
        private void FocusItem(int index)
        {
            if (Items.Count == 0) return;
            _currentItemIndex = Mathf.Clamp(index, 0, Items.Count - 1);
            _currentRotationOffset = 0f; _isSnapping = true;
            for (int i = 0; i < Items.Count; i++) if (Items[i] != null) Items[i].GetComponent<CarouselButton>()?.SetFocus(i == _currentItemIndex);
        }

        public void GoToNext() { if (Items.Count > 1) FocusItem(Mode == CarouselMode.Infinite ? (_currentItemIndex + 1) % Items.Count : Mathf.Min(_currentItemIndex + 1, Items.Count - 1)); }
        public void GoToPrevious() { if (Items.Count > 1) FocusItem(Mode == CarouselMode.Infinite ? (_currentItemIndex - 1 + Items.Count) % Items.Count : Mathf.Max(_currentItemIndex - 1, 0)); }
        public void ForceUpdate() { Items.RemoveAll(x => x == null); PositionItems(false); UpdateItemsAppearance(); }

        private void StartColorAnimation(RectTransform item, Color target)
        {
            if (_activeColorAnimations.TryGetValue(item, out Coroutine r)) { if (r != null) StopCoroutine(r); _activeColorAnimations.Remove(item); }
            if (item == null || !item.TryGetComponent<Image>(out var img)) return;

            if (!gameObject.activeInHierarchy || !item.gameObject.activeInHierarchy) { img.color = target; return; }
            _activeColorAnimations[item] = StartCoroutine(ColorAnim(item, target, img));
        }

        private IEnumerator ColorAnim(RectTransform item, Color targetColor, Image img)
        {
            Color start = img.color; float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                if (item == null || img == null) { _activeColorAnimations.Remove(item); yield break; }
                elapsed += Time.deltaTime; img.color = Color.Lerp(start, targetColor, elapsed / 0.2f);
                yield return null;
            }
            img.color = targetColor; _activeColorAnimations.Remove(item);
        }

        public void SetItems(List<RectTransform> items) { ClearItems(); Items.AddRange(items); _currentItemIndex = Mathf.Clamp(StartItem, 0, Mathf.Max(0, Items.Count - 1)); ForceUpdate(); }
        public void AddItem(RectTransform item, int index = -1) { if (item == null || Items.Contains(item)) return; int target = (index < 0 || index > Items.Count) ? Items.Count : index; Items.Insert(target, item); if (target <= _currentItemIndex && Items.Count > 1) _currentItemIndex++; item.localScale = Vector3.one * MaxNonCenteredScale; ForceUpdate(); }
        public void RemoveItem(RectTransform item) { if (item == null || !Items.Contains(item)) return; int idx = Items.IndexOf(item); if (_activeColorAnimations.TryGetValue(item, out Coroutine r)) { if (r != null) StopCoroutine(r); _activeColorAnimations.Remove(item); } Items.RemoveAt(idx); if (Items.Count == 0) { _currentItemIndex = 0; _isSnapping = false; return; } if (idx < _currentItemIndex) _currentItemIndex--; else if (idx == _currentItemIndex) FocusItem(Mathf.Clamp(_currentItemIndex, 0, Items.Count - 1)); ForceUpdate(); }
        public void ClearItems() { _isSnapping = false; _currentItemIndex = 0; foreach (var kvp in _activeColorAnimations) if (kvp.Value != null) StopCoroutine(kvp.Value); _activeColorAnimations.Clear(); Items.Clear(); }
    }
}