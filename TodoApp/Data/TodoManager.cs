using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Data
{
    public class TodoManager : ITodoManager
    {
        private ILogger<TodoManager> _logger;
        private AppConfiguration _config;
        private List<Todo> _todos = new();

        public TodoManager(ILogger<TodoManager> logger, IOptions<AppConfiguration> options)
        {
            _logger = logger;
            _config = options.Value;
        }

        public void Add(Todo todo)
        {
            if (!IsOverlapping(todo.TargetTime))
            {
                _todos.Add(todo);
                return;
            }

            // Raise an exception.
            throw new InvalidOperationException("The todo item is overlapping with existing todos.");

        }
        public void Remove(Todo todo) => _todos.Remove(todo);
        public void Remove(int todoId)
        {
            var index = -1;
            for(int i = 0; i < _todos.Count; i++)
            {
                if(_todos[i].TodoId == todoId)
                {
                    index = i;
                    break;
                }
            }

            if(index == -1)
            {
                throw new InvalidOperationException($"No todo with id {todoId}");
            }

            _todos.RemoveAt(index);
        }

        public Todo ParseTodo(string fromString)
        {
            return new Todo
            {
                Description = fromString.Substring(0, _config.DescriptionLength).Trim(),

                TargetTime = fromString.Substring(_config.DescriptionLength).Trim().ToDateTime(_config.TodoDateFormat)
            };
        }

        public string FormatTodo(Todo todo, bool includeId = false)
        {
            var desc = todo.Description.PadRight(_config.DescriptionLength);
            var dateTime = todo.TargetTime.ToFormatted(_config.TodoDateFormat);

            if (includeId)
            {
                return $"{todo.TodoId}. {desc}{dateTime}";
            }

            return desc + dateTime;
        }

        public void ListTodos()
        {
            if(_todos.Count == 0)
            {
                Console.Error.WriteLine("No todos added yet.");
            }

            var todos = (from todo in _todos
                         orderby todo.TodoId
                         select todo).ToList();

            foreach (var todo in todos)
            {
                Console.WriteLine(FormatTodo(todo, true));
            }
        }

        private List<string> GetFormattedTodos()
        {
            _todos.Sort();

            List<string> formatted = new();
            foreach (var todo in _todos)
            {
                formatted.Add(FormatTodo(todo));
            }

            return formatted;
        }

        public async Task LoadFromFile(string filePath)
        {
            // Clear the todos before loading.
            _todos.Clear();

            // Check whether file exists.
            if (File.Exists(filePath))
            {
                var entries = await File.ReadAllLinesAsync(filePath);

                foreach (var entry in entries)
                {
                    _todos.Add(ParseTodo(entry));
                }
            }
        }

        public async Task Save(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath); // Remove the file before writing to it.
            }

            // Get the formatted todos.
            var formattedTodos = GetFormattedTodos();

            await File.WriteAllLinesAsync(filePath, formattedTodos);
        }

        public List<Todo> GetTodos() => _todos;

        public List<int> GetTodoIds() => _todos.Select(t => t.TodoId).ToList();

        public bool IsOverlapping(DateTime parsedTime)
        {
            _todos.Sort();

            foreach (var todo in _todos)
            {
                var gap = todo.TargetTime - parsedTime;

                if (Math.Abs(gap.TotalSeconds) < _config.MinGapBetweenTodosInMinutes * 60)
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _todos.Clear();
        }
    }
}
