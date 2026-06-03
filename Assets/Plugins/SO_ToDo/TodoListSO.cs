using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName ="TodoList",
    menuName ="Tools/Todo List")]
public class TodoListSO : ScriptableObject
{
    public List<TodoItem> Items = new();
}
