using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoApp.Data
{
    public interface ITodoManager
    {
        void Add(Todo todo);
        string FormatTodo(Todo todo, bool includeId = false);
        List<Todo> GetTodos();
        void ListTodos();
        Task LoadFromFile(string filePath);
        Todo ParseTodo(string fromString);
        void Remove(Todo todo);
        Task Save(string filePath);

        List<int> GetTodoIds();
        void Remove(int todoId);
        bool IsOverlapping(DateTime parsedTime);
        void Clear();
    }
}