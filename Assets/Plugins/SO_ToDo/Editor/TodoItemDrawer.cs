
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TodoItem))]
public class TodoItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var isDone = property.FindPropertyRelative("IsDone");
        var task = property.FindPropertyRelative("Task");
        float toggleWidth = 20f;
        Rect toggleRect = new(
           position.x,
           position.y,
           toggleWidth,
           position.height);

        Rect textRect = new(
            position.x + toggleWidth + 5,
            position.y,
            position.width - toggleWidth - 5,
            position.height);

        EditorGUI.PropertyField(toggleRect, isDone, GUIContent.none);
        EditorGUI.PropertyField(textRect, task, GUIContent.none);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
