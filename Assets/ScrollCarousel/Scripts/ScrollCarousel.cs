using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.ComponentModel;

namespace ScrollCarousel
{
    public enum CarouselMode
    {
        Horizontal,
        Vertical,
        Infinite
    }

    public class Carousel : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        [Header("Scrolling Mode")]
        public CarouselMode Mode = CarouselMode.Horizontal;

        [HideInInspector]
        [Obsolete("Use 'Mode = CarouselMode.Infinite' instead.")]
        public bool InfiniteScroll
        {
            get => Mode == CarouselMode.Infinite;
            set => Mode = value ? CarouselMode.Infinite : CarouselMode.Horizontal;
        }

        [Header("Items")]
        public List<RectTransform> Items = new List<RectTransform>();

        [Header("Position")]
        public int StartItem = 0;
        public float Itemspacing = 50f;

        [Header("Scale")]
        public float CenteredScale = 1f;
        public float NonCenteredScale = 0.7f;

        [Header("Rotation")]
        [SerializeField] public float MaxRotationAngle = 10f;
        [SerializeField] private float _rotationSmoothSpeed = 5f;

        [Header("Swipe Settings")]
        [SerializeField] private float _snapSpeed = 10f;
        [SerializeField] private float _swipeThreshold = 35f; // The distance a finger must drag to count as a "Swipe"
        [SerializeField] public float CircleRadius = 500f;

        [Header("Colors")]
        public bool ColorAnimation = false;
        public Color FocustedColor = Color.white;
        public Color NonFocustedColor = Color.gray;

        [SerializeField] private RectTransform _rectTransform;
        private int _currentItemIndex = 0;
        private bool _isSnapping = false;
        private float _currentRotationOffset = 0f;
        private Vector2 _totalDragDelta; // <--- Tracks the cumulative momentum of a swipe
        private Dictionary<RectTransform, Coroutine> _activeColorAnimations = new Dictionary<RectTransform, Coroutine>();
        private List<(RectTransform item, float distance)> _depthSortBuffer = new();

        private void Awake()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            if (Items != null && Items.Count > 0)
            {
                ForceUpdate();
            }
        }

        private void Start()
        {
            FocusItem(StartItem);
            ForceUpdate();
        }

        private void Update()
        {
            if (_isSnapping) MoveToItem();
            UpdateItemsAppearance();
        }

        private float GetItemSpacing(int index, bool isHorizontal)
        {
            float currentItemScale = (index == _currentItemIndex) ? CenteredScale : NonCenteredScale;
            float currentSize = isHorizontal ? Items[index].rect.width : Items[index].rect.height;
            currentSize *= currentItemScale;

            if (index + 1 >= Items.Count) return currentSize + Itemspacing;

            float nextItemScale = (index + 1 == _currentItemIndex) ? CenteredScale : NonCenteredScale;
            float nextSize = isHorizontal ? Items[index + 1].rect.width : Items[index + 1].rect.height;
            nextSize *= nextItemScale;

            return (currentSize + nextSize) / 2f + Itemspacing;
        }

        private float GetTotalOffset(int index, bool isHorizontal)
        {
            float offset = 0f;
            int startIdx = Math.Min(index, _currentItemIndex);
            int endIdx = Math.Max(index, _currentItemIndex);

            for (int i = startIdx; i < endIdx; i++)
            {
                offset += GetItemSpacing(i, isHorizontal);
            }

            if (isHorizontal) return index < _currentItemIndex ? -offset : offset;
            else return index < _currentItemIndex ? offset : -offset;
        }

        private void PositionItems(bool animate = true)
        {
            if (Items.Count == 0) return;

            Vector2 centerPoint = _rectTransform.rect.center;
            float targetTime = animate ? Time.deltaTime * _snapSpeed : 1f;

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == null) continue;

                Vector2 targetPosition;
                if (Mode == CarouselMode.Infinite)
                {
                    float angle = (360f / Items.Count) * (i - _currentItemIndex);
                    float radians = angle * Mathf.Deg2Rad;
                    targetPosition = new Vector2(
                        centerPoint.x + Mathf.Sin(radians) * CircleRadius,
                        centerPoint.y + (1 - Mathf.Cos(radians)) * CircleRadius * 0.5f
                    );
                }
                else if (Mode == CarouselMode.Vertical)
                {
                    float offset = GetTotalOffset(i, false);
                    targetPosition = new Vector2(centerPoint.x, centerPoint.y + offset);
                }
                else // Horizontal
                {
                    float offset = GetTotalOffset(i, true);
                    targetPosition = new Vector2(centerPoint.x + offset, centerPoint.y);
                }

                Items[i].anchoredPosition = animate
                    ? Vector2.Lerp(Items[i].anchoredPosition, targetPosition, targetTime)
                    : targetPosition;
            }
        }

        private void UpdateItemsAppearance()
        {
            if (Items.Count == 0) return;

            Vector2 centerPoint = _rectTransform.rect.center;
            float maxDistance;

            if (Mode == CarouselMode.Infinite) maxDistance = CircleRadius;
            else if (Mode == CarouselMode.Vertical) maxDistance = GetItemSpacing(0, false);
            else maxDistance = GetItemSpacing(0, true);

            if (maxDistance == 0) maxDistance = 1f;

            float minDistance = float.MaxValue;
            int closestIndex = -1;

            _depthSortBuffer.Clear(); // Clear our zero-allocation buffer

            for (int i = 0; i < Items.Count; i++)
            {
                if (!Items[i]) continue;

                float distance;
                float angleDistance;

                if (Mode == CarouselMode.Infinite)
                {
                    float angle = (360f / Items.Count) * (i - _currentItemIndex) + _currentRotationOffset;
                    distance = Mathf.Abs(Mathf.DeltaAngle(0, angle)) / (360f / Items.Count) * CircleRadius;
                    angleDistance = Mathf.Abs(Mathf.DeltaAngle(0, angle)) / (360f / Items.Count);
                }
                else if (Mode == CarouselMode.Vertical)
                {
                    distance = Mathf.Abs(Items[i].anchoredPosition.y - centerPoint.y);
                    angleDistance = Mathf.Abs(i - _currentItemIndex);
                }
                else // Horizontal
                {
                    distance = Mathf.Abs(Items[i].anchoredPosition.x - centerPoint.x);
                    angleDistance = Mathf.Abs(i - _currentItemIndex);
                }

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }

                // 1. Capture the item and its true physical distance this frame
                _depthSortBuffer.Add((Items[i], distance));

                float normalizedDistance = Mathf.Clamp01(distance / maxDistance);

                // --- Scale ---
                float targetScale = Mathf.Lerp(CenteredScale, NonCenteredScale, normalizedDistance);
                Vector3 newScale = new Vector3(targetScale, targetScale, 1f);
                if (!float.IsNaN(newScale.x)) Items[i].localScale = newScale;

                // --- Rotation ---
                if (Mode == CarouselMode.Vertical)
                {
                    float rotationSign = (Items[i].anchoredPosition.y > centerPoint.y) ? -1f : 1f;
                    float targetRotationX = MaxRotationAngle * normalizedDistance * rotationSign;
                    if (!float.IsNaN(targetRotationX))
                    {
                        Items[i].localRotation = Quaternion.Slerp(Items[i].localRotation, Quaternion.Euler(targetRotationX, 0, 0), Time.deltaTime * _rotationSmoothSpeed);
                    }
                }
                else
                {
                    float rotationSign = (Items[i].anchoredPosition.x > centerPoint.x) ? 1f : -1f;
                    float targetRotationY = MaxRotationAngle * normalizedDistance * rotationSign;
                    if (!float.IsNaN(targetRotationY))
                    {
                        Items[i].localRotation = Quaternion.Slerp(Items[i].localRotation, Quaternion.Euler(30, targetRotationY, 0), Time.deltaTime * _rotationSmoothSpeed);
                    }
                }
            }

            // 2. Sort buffer DESCENDING (Furthest distance first -> Closest distance last)
            // (Using a C# 9 'static' lambda guarantees no hidden closure memory allocations)
            _depthSortBuffer.Sort(static (a, b) => b.distance.CompareTo(a.distance));

            // 3. Render stack push. The closest item gets pushed last, putting it on top.
            foreach (var entry in _depthSortBuffer)
            {
                entry.item.SetAsLastSibling();
            }

            if (ColorAnimation && closestIndex != -1)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == null) continue;
                    Color targetColor = (i == closestIndex) ? FocustedColor : NonFocustedColor;
                    StartColorAnimation(Items[i], targetColor);
                }
            }
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            _isSnapping = false;
            _totalDragDelta = Vector2.zero; // Reset swipe tracker
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Items.Count == 0) return;

            _totalDragDelta += eventData.delta; // Accumulate mouse/finger distance

            if (Mode == CarouselMode.Infinite)
            {
                float rotationDelta = (eventData.delta.x / CircleRadius) * 45f;
                _currentRotationOffset += rotationDelta;
                RotateItemsCircular(_currentRotationOffset);
            }
            else if (Mode == CarouselMode.Vertical)
            {
                float dragFactor = 1f;
                // Add physical "heaviness" if pulling against the hard top/bottom limits
                if ((_currentItemIndex == 0 && eventData.delta.y < 0) || (_currentItemIndex == Items.Count - 1 && eventData.delta.y > 0))
                    dragFactor = 0.35f;

                foreach (RectTransform item in Items)
                {
                    if (item != null) item.anchoredPosition += new Vector2(0, eventData.delta.y * dragFactor);
                }
            }
            else // Horizontal
            {
                float dragFactor = 1f;
                // Add physical "heaviness" if pulling against the hard left/right limits
                if ((_currentItemIndex == 0 && eventData.delta.x > 0) || (_currentItemIndex == Items.Count - 1 && eventData.delta.x < 0))
                    dragFactor = 0.35f;

                foreach (RectTransform item in Items)
                {
                    if (item != null) item.anchoredPosition += new Vector2(eventData.delta.x * dragFactor, 0);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Items.Count == 0) return;

            if (Mode == CarouselMode.Infinite)
            {
                float closestDistance = float.MaxValue;
                int closestIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == null) continue;
                    float angle = (360f / Items.Count) * (i - _currentItemIndex) + _currentRotationOffset;
                    float distance = Mathf.Abs(Mathf.DeltaAngle(0, angle));
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestIndex = i;
                    }
                }
                FocusItem(closestIndex);
            }
            else if (Mode == CarouselMode.Vertical)
            {
                // Swiping UP (positive delta.y) moves content up, bringing the NEXT item into view
                if (_totalDragDelta.y > _swipeThreshold) GoToNext();
                else if (_totalDragDelta.y < -_swipeThreshold) GoToPrevious();
                else FocusItem(_currentItemIndex); // Didn't swipe hard enough; spring back to current
            }
            else // Horizontal
            {
                // Swiping LEFT (negative delta.x) moves content left, bringing NEXT item into view
                if (_totalDragDelta.x < -_swipeThreshold) GoToNext();
                else if (_totalDragDelta.x > _swipeThreshold) GoToPrevious();
                else FocusItem(_currentItemIndex); // Didn't swipe hard enough; spring back to current
            }

            _currentRotationOffset = 0f;
        }

        // =========================================================

        private void RotateItemsCircular(float rotationOffset)
        {
            Vector2 centerPoint = _rectTransform.rect.center;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == null) continue;
                float baseAngle = (360f / Items.Count) * (i - _currentItemIndex);
                float angle = baseAngle + rotationOffset;
                float radians = angle * Mathf.Deg2Rad;

                Items[i].anchoredPosition = new Vector2(
                    centerPoint.x + Mathf.Sin(radians) * CircleRadius,
                    centerPoint.y + (1 - Mathf.Cos(radians)) * CircleRadius * 0.5f
                );
            }
        }

        private void MoveToItem()
        {
            PositionItems(true);
            if (Items.Count == 0 || Items[_currentItemIndex] == null) return;

            RectTransform targetItem = Items[_currentItemIndex];
            float currentPos = (Mode == CarouselMode.Vertical) ? targetItem.anchoredPosition.y : targetItem.anchoredPosition.x;
            float targetCenter = (Mode == CarouselMode.Vertical) ? _rectTransform.rect.center.y : _rectTransform.rect.center.x;

            if (Mathf.Abs(currentPos - targetCenter) < 0.1f)
            {
                _isSnapping = false;
                PositionItems(false);
            }
        }

        public void FocusItem(RectTransform item) => FocusItem(Items.IndexOf(item));

        private void FocusItem(int index)
        {
            if (Items.Count == 0) return;

            _currentItemIndex = Mathf.Clamp(index, 0, Items.Count - 1);
            _currentRotationOffset = 0f;
            _isSnapping = true;

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] != null) Items[i].GetComponent<CarouselButton>()?.SetFocus(i == _currentItemIndex);
            }
        }

        public void GoToNext()
        {
            if (Items.Count <= 1) return;
            FocusItem(Mode == CarouselMode.Infinite ? (_currentItemIndex + 1) % Items.Count : Mathf.Min(_currentItemIndex + 1, Items.Count - 1));
        }

        public void GoToPrevious()
        {
            if (Items.Count <= 1) return;
            FocusItem(Mode == CarouselMode.Infinite ? (_currentItemIndex - 1 + Items.Count) % Items.Count : Mathf.Max(_currentItemIndex - 1, 0));
        }

        public void ForceUpdate()
        {
            Items.RemoveAll(x => x == null);
            PositionItems(false);
            UpdateItemsAppearance();
        }

        private void StartColorAnimation(RectTransform item, Color targetColor)
        {
            if (_activeColorAnimations.TryGetValue(item, out Coroutine existingRoutine))
            {
                if (existingRoutine != null) StopCoroutine(existingRoutine);
                _activeColorAnimations.Remove(item);
            }

            if (item == null || !item.TryGetComponent<Image>(out var image)) return;

            if (!this.gameObject.activeInHierarchy || !item.gameObject.activeInHierarchy)
            {
                image.color = targetColor;
                return;
            }

            _activeColorAnimations[item] = StartCoroutine(ColorAnimationCoroutine(item, targetColor, image));
        }

        private IEnumerator ColorAnimationCoroutine(RectTransform item, Color targetColor, Image image)
        {
            Color startColor = image.color;
            float elapsedTime = 0f;
            float duration = 0.2f;

            while (elapsedTime < duration)
            {
                if (item == null || image == null)
                {
                    _activeColorAnimations.Remove(item);
                    yield break;
                }

                elapsedTime += Time.deltaTime;
                image.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
                yield return null;
            }

            if (image != null) image.color = targetColor;
            _activeColorAnimations.Remove(item);
        }

        #region NEW ADD AND REMOVE ITEMS

        public void SetItems(List<RectTransform> items)
        {
            ClearItems();
            Items.AddRange(items);
            _currentItemIndex = Mathf.Clamp(StartItem, 0, Mathf.Max(0, Items.Count - 1));
            ForceUpdate();
        }

        public void AddItem(RectTransform item, int insertAtIndex = -1)
        {
            if (item == null || Items.Contains(item)) return;

            int targetIndex = (insertAtIndex < 0 || insertAtIndex > Items.Count) ? Items.Count : insertAtIndex;
            Items.Insert(targetIndex, item);

            if (targetIndex <= _currentItemIndex && Items.Count > 1) _currentItemIndex++;

            item.localScale = Vector3.one * NonCenteredScale;
            if (ColorAnimation && item.TryGetComponent<Image>(out var img)) img.color = NonFocustedColor;

            ForceUpdate();
        }

        public void RemoveItem(RectTransform item)
        {
            if (item == null || !Items.Contains(item)) return;

            int removedIndex = Items.IndexOf(item);
            if (_activeColorAnimations.TryGetValue(item, out Coroutine routine))
            {
                if (routine != null) StopCoroutine(routine);
                _activeColorAnimations.Remove(item);
            }
            Items.RemoveAt(removedIndex);

            if (Items.Count == 0)
            {
                _currentItemIndex = 0;
                _isSnapping = false;
                return;
            }

            if (removedIndex < _currentItemIndex) _currentItemIndex--;
            else if (removedIndex == _currentItemIndex)
            {
                _currentItemIndex = Mathf.Clamp(_currentItemIndex, 0, Items.Count - 1);
                FocusItem(_currentItemIndex);
            }

            ForceUpdate();
        }

        public void ClearItems()
        {
            _isSnapping = false;
            _currentItemIndex = 0;

            foreach (var kvp in _activeColorAnimations)
            {
                if (kvp.Value != null) StopCoroutine(kvp.Value);
            }
            _activeColorAnimations.Clear();
            Items.Clear();
        }

        #endregion
    }
}