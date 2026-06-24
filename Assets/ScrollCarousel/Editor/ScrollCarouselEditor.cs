using System;
using UnityEditor;
using UnityEngine;

namespace ScrollCarousel
{
    [CustomEditor(typeof(Carousel))]
    public class ScrollCarouselEditor : Editor
    {
        private Carousel carousel;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            carousel = (Carousel)target;

            GUILayout.BeginVertical("box");
            GUILayout.Label("Carousel Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Add All Children to Items"))
            {
                AddAllChildrenToItemList();
            }

            if (GUILayout.Button("Organize Items (Preview Layout)"))
            {
                Undo.RecordObject(carousel, "Organize Carousel Items");
                foreach (var item in carousel.Items)
                {
                    if (item != null) Undo.RecordObject(item, "Organize Carousel Items");
                }

                OrganizeItemsInEditor();
                UpdateItemsAppearanceInEditor();
                OrganizeItemsInEditor();
            }

            GUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // 1. Layout Mode
            SerializedProperty modeProp = serializedObject.FindProperty("Mode");
            EditorGUILayout.PropertyField(modeProp);

            if (modeProp.enumValueIndex == (int)CarouselMode.Infinite)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CircleRadius"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Items"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StartItem"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Itemspacing"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Scale Falloff", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CenteredScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxNonCenteredScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MinNonCenteredScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FalloffSlotCount"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Appearance & Physics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxRotationAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rotationSmoothSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_snapSpeed"));

            EditorGUILayout.Space(5);
            SerializedProperty colorAnimProp = serializedObject.FindProperty("ColorAnimation");
            EditorGUILayout.PropertyField(colorAnimProp);
            if (colorAnimProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FocustedColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("NonFocustedColor"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Internal References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rectTransform"));

            serializedObject.ApplyModifiedProperties();
        }

        private void AddAllChildrenToItemList()
        {
            Undo.RecordObject(carousel, "Add All Children to Carousel");
            carousel.Items.Clear();

            foreach (Transform child in carousel.transform)
            {
                if (child is RectTransform rectTransform)
                {
                    carousel.Items.Add(rectTransform);
                }
            }
            EditorUtility.SetDirty(carousel);
        }

        // --- NEW: Editor equivalent of GetTargetRestScale relative to 'StartItem' ---
        private float GetTargetRestScaleInEditor(int index)
        {
            int slotsAway = Mathf.Abs(index - carousel.StartItem);

            if (slotsAway == 0) return carousel.CenteredScale;
            if (slotsAway == 1) return carousel.MaxNonCenteredScale;

            float safeFalloff = Mathf.Max(0.01f, carousel.FalloffSlotCount);
            float t = Mathf.Clamp01((slotsAway - 1f) / safeFalloff);
            return Mathf.Lerp(carousel.MaxNonCenteredScale, carousel.MinNonCenteredScale, t);
        }

        private void OrganizeItemsInEditor()
        {
            carousel.Items.RemoveAll(x => x == null);
            if (carousel.Items.Count == 0) return;

            RectTransform baseRect = carousel.GetComponent<RectTransform>();
            Vector2 centerPoint = baseRect != null ? baseRect.rect.center : Vector2.zero;

            for (int i = 0; i < carousel.Items.Count; i++)
            {
                Vector2 targetPosition;
                if (carousel.Mode == CarouselMode.Infinite)
                {
                    float angle = (360f / carousel.Items.Count) * (i - carousel.StartItem);
                    float radians = angle * Mathf.Deg2Rad;
                    targetPosition = new Vector2(
                        centerPoint.x + Mathf.Sin(radians) * carousel.CircleRadius,
                        centerPoint.y + (1 - Mathf.Cos(radians)) * carousel.CircleRadius * 0.5f
                    );
                }
                else if (carousel.Mode == CarouselMode.Vertical)
                {
                    float offset = GetTotalOffset(i, false) - GetTotalOffset(carousel.StartItem, false);
                    targetPosition = new Vector2(centerPoint.x, centerPoint.y + offset);
                }
                else // Horizontal
                {
                    float offset = GetTotalOffset(i, true) - GetTotalOffset(carousel.StartItem, true);
                    targetPosition = new Vector2(centerPoint.x + offset, centerPoint.y);
                }

                carousel.Items[i].anchoredPosition = targetPosition;
                EditorUtility.SetDirty(carousel.Items[i]);
            }

            EditorUtility.SetDirty(carousel);
        }

        private void UpdateItemsAppearanceInEditor()
        {
            if (carousel.Items.Count == 0) return;

            RectTransform baseRect = carousel.GetComponent<RectTransform>();
            Vector2 centerPoint = baseRect != null ? baseRect.rect.center : Vector2.zero;

            float maxDistance;
            if (carousel.Mode == CarouselMode.Infinite) maxDistance = carousel.CircleRadius;
            else if (carousel.Mode == CarouselMode.Vertical) maxDistance = GetItemSpacing(0, false);
            else maxDistance = GetItemSpacing(0, true);

            if (maxDistance == 0) maxDistance = 1f;

            for (int i = 0; i < carousel.Items.Count; i++)
            {
                if (!carousel.Items[i]) continue;

                float visualDistance = Mathf.Abs(i - carousel.StartItem);
                carousel.Items[i].SetSiblingIndex(carousel.Items.Count - (int)(visualDistance * 2));

                float distance;
                if (carousel.Mode == CarouselMode.Infinite)
                {
                    float angle = (360f / carousel.Items.Count) * (i - carousel.StartItem);
                    distance = Mathf.Abs(angle) * carousel.CircleRadius / 180f;
                }
                else if (carousel.Mode == CarouselMode.Vertical)
                {
                    distance = Mathf.Abs(carousel.Items[i].anchoredPosition.y - centerPoint.y);
                }
                else // Horizontal
                {
                    distance = Mathf.Abs(carousel.Items[i].anchoredPosition.x - centerPoint.x);
                }

                float normalizedDistance = Mathf.Clamp01(distance / maxDistance);

                // --- UPDATED SCALE LOGIC ---
                float targetScale = GetTargetRestScaleInEditor(i);
                Vector3 newScale = new Vector3(targetScale, targetScale, 1f);
                if (!float.IsNaN(newScale.x)) carousel.Items[i].localScale = newScale;

                // Rotation
                if (carousel.Mode == CarouselMode.Vertical)
                {
                    float rotationSign = (carousel.Items[i].anchoredPosition.y > centerPoint.y) ? -1f : 1f;
                    float targetRotationX = carousel.MaxRotationAngle * normalizedDistance * rotationSign;
                    if (!float.IsNaN(targetRotationX))
                        carousel.Items[i].localRotation = Quaternion.Euler(targetRotationX, 0, 0);
                }
                else
                {
                    float rotationSign = (carousel.Items[i].anchoredPosition.x > centerPoint.x) ? 1f : -1f;
                    float targetRotationY = carousel.MaxRotationAngle * normalizedDistance * rotationSign;
                    if (!float.IsNaN(targetRotationY))
                        carousel.Items[i].localRotation = Quaternion.Euler(30, targetRotationY, 0);
                }

                EditorUtility.SetDirty(carousel.Items[i]);
            }
        }

        private float GetItemSpacing(int index, bool isHorizontal)
        {
            float currentItemScale = GetTargetRestScaleInEditor(index);
            float currentSize = isHorizontal ? carousel.Items[index].rect.width : carousel.Items[index].rect.height;
            currentSize *= currentItemScale;

            if (index + 1 >= carousel.Items.Count) return currentSize + carousel.Itemspacing;

            float nextItemScale = GetTargetRestScaleInEditor(index + 1);
            float nextSize = isHorizontal ? carousel.Items[index + 1].rect.width : carousel.Items[index + 1].rect.height;
            nextSize *= nextItemScale;

            return (currentSize + nextSize) / 2f + carousel.Itemspacing;
        }

        private float GetTotalOffset(int index, bool isHorizontal)
        {
            float offset = 0f;
            int startIdx = Math.Min(index, carousel.StartItem);
            int endIdx = Math.Max(index, carousel.StartItem);

            for (int i = startIdx; i < endIdx; i++)
            {
                offset += GetItemSpacing(i, isHorizontal);
            }

            if (isHorizontal) return index < carousel.StartItem ? -offset : offset;
            else return index < carousel.StartItem ? offset : -offset;
        }
    }
}