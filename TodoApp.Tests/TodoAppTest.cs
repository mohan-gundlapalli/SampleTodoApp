using System;
using System.Collections.Generic;
using TodoApp.Data;
using Xunit;
using Moq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace TodoApp.Tests
{
    public class TodoAppTest
    {
        List<Todo> todos = new()
        {
            new Todo { Description = "Take C# coding test", TargetTime = DateTime.Now.AddHours(3) },
            new Todo { Description = "Bake a cake", TargetTime = DateTime.Now.AddHours(1) },
            new Todo { Description = "Go to kite flying festival", TargetTime = DateTime.Now.AddHours(2) }
        };

        AppConfiguration config = new()
        {
            TodoFileName = "todo-test.txt",
            TodoDateFormat = "dd MMM yyyy hh:mm tt",
            DescriptionLength = 60,
            DateTimeLength = 20,
            MinGapBetweenTodosInMinutes = 5
        };

        

        [Fact]
        public async Task Check_Todos_Saved_To_File()
        {
            // Arrange
            IOptions<AppConfiguration> configOptions = Options.Create<AppConfiguration>(config);
            var loggerMock = new Mock<ILogger<TodoManager>>();
            TodoManager todoMgr = new TodoManager(loggerMock.Object, configOptions);


            // Act
            todoMgr.Add(todos[0]);
            todoMgr.Add(todos[1]);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), config.TodoFileName);

            await todoMgr.Save(filePath);

            // Assert
            Assert.True(File.Exists(filePath));
            var savedTodos = await File.ReadAllLinesAsync(filePath);

            Assert.Equal(todoMgr.GetTodos().Count, savedTodos.Length);
        }

        [Fact]
        public void Check_Cannot_Add_Overlapping_Todo()
        {
            // Arrange
            IOptions<AppConfiguration> configOptions = Options.Create<AppConfiguration>(config);
            var loggerMock = new Mock<ILogger<TodoManager>>();
            TodoManager todoMgr = new TodoManager(loggerMock.Object, configOptions);


            // Act
            todoMgr.Add(todos[0]);
            todoMgr.Add(todos[1]);

            var overlappingTodo = new Todo { Description = "Overlapping task", 
                TargetTime = todos[0].TargetTime.AddMinutes(config.MinGapBetweenTodosInMinutes - 1) };

            
            Assert.Throws<InvalidOperationException>( () => todoMgr.Add(overlappingTodo));
        }

        [Fact]
        public void Check_Can_Parse_Todo()
        {
            // Arrange
            IOptions<AppConfiguration> configOptions = Options.Create<AppConfiguration>(config);
            var loggerMock = new Mock<ILogger<TodoManager>>();
            TodoManager todoMgr = new TodoManager(loggerMock.Object, configOptions);

            var description = "Take C# coding test";
            var date = "29 Oct 2017 10:00 PM";
            string todoStr = $"{description.PadRight(config.DescriptionLength)} {date}";

            var todo = todoMgr.ParseTodo(todoStr);

            Assert.Equal(description, todo.Description);
            Assert.Equal(date, todo.TargetTime.ToFormatted(config.TodoDateFormat));
        }

        [Fact]
        public void Check_Cannot_Parse_Malformed_Todo()
        {
            // Arrange
            IOptions<AppConfiguration> configOptions = Options.Create<AppConfiguration>(config);
            var loggerMock = new Mock<ILogger<TodoManager>>();
            TodoManager todoMgr = new TodoManager(loggerMock.Object, configOptions);

            var description = "Take C# coding test";
            var date = "October 10 2017 23:00 PM";
            string todoStr = $"{description.PadRight(config.DescriptionLength)} {date}";

            Assert.Throws<FormatException>(() => todoMgr.ParseTodo(todoStr));
        }

        [Fact]
        public async Task Check_Todos_Saved_To_File_Are_Ordered()
        {
            // Arrange
            IOptions<AppConfiguration> configOptions = Options.Create<AppConfiguration>(config);
            var loggerMock = new Mock<ILogger<TodoManager>>();
            TodoManager todoMgr = new TodoManager(loggerMock.Object, configOptions);


            // Act
            todoMgr.Add(todos[0]);
            todoMgr.Add(todos[1]);
            todoMgr.Add(todos[2]);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), config.TodoFileName);

            await todoMgr.Save(filePath);

            todoMgr.Clear();

            // Ensure the todo manager is empty.
            Assert.Empty(todoMgr.GetTodos());

            Assert.True(File.Exists(filePath));

            await todoMgr.LoadFromFile(filePath);
            var savedTodos = todoMgr.GetTodos();

            Assert.Equal(savedTodos[0].Description, todos[1].Description);
            Assert.Equal(savedTodos[1].Description, todos[2].Description);
            Assert.Equal(savedTodos[2].Description, todos[0].Description);

            Assert.Equal(todos.Count, savedTodos.Count);

            
        }
    }
}
