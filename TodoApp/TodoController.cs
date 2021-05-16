using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Data;

namespace TodoApp
{
    delegate void UserActionHandler();
    public class TodoController
    {
        private ILogger<TodoController> _logger;
        private AppConfiguration _config;
        private ITodoManager _todoMgr;
        public TodoController(ILogger<TodoController> logger, IOptions<AppConfiguration> options, ITodoManager todoMgr)
        {
            _logger = logger;
            _config = options.Value;
            _todoMgr = todoMgr;
        }

        public async Task Run()
        {
            try
            {

                // Load the data from the file first.
                var currentLocation = Directory.GetCurrentDirectory();
                string filePath = Path.Combine(currentLocation, _config.TodoFileName);
                await _todoMgr.LoadFromFile(filePath);

                UserAction userAction = UserAction.List;

                Dictionary<UserAction, UserActionHandler> actionHandlers = new();

                actionHandlers[UserAction.List] = () => _todoMgr.ListTodos();
                actionHandlers[UserAction.Add] = () => HandleAddAction();
                actionHandlers[UserAction.Remove] = () => HandleRemoveAction();

                do
                {
                    userAction = GetUserAction();

                    if(userAction == UserAction.Exit)
                    {
                        break;
                    }

                    // Handle the user action.
                    actionHandlers[userAction](); 

                } while (userAction != UserAction.Exit);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                _logger.LogError(ex.Message, ex);
            }
            finally
            {
                // save the data before quitting.
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), _config.TodoFileName);
                await _todoMgr.Save(filePath);
            }

        }

        private void HandleRemoveAction()
        {
            int todoId = GetIntFromUser(_todoMgr.GetTodoIds(), "Enter the number of todo to remove it: ");

            _todoMgr.Remove(todoId);

            Console.WriteLine("\n---Removed---");

        }



        private void HandleAddAction()
        {
            string description = ReadStringFromUser(2, _config.DescriptionLength - 1); // Ensure at least one space.
            DateTime targetTime = ReadDateFromUser(_config.TodoDateFormat);

            _todoMgr.Add(new Todo { Description = description, TargetTime = targetTime });

            Console.WriteLine("\n---Added---");
        }

        private int GetIntFromUser(List<int> validValues, string inputPrompt)
        {
            string todoIdStr = "";
            int? todoId = null;

            do
            {
                if (todoIdStr is not (null or ""))
                {
                    Console.Error.WriteLine($"The entered value is not valid. Please enter a valid todo no.");
                }

                // list the todos for the user to see all the todos.
                _todoMgr.ListTodos();
                Console.Write(inputPrompt);
                todoIdStr = Console.ReadLine();

                if (int.TryParse(todoIdStr, out int result))
                {
                    if (validValues.Contains(result))
                    {
                        todoId = result;
                    }
                }

            } while (todoId == null);

            return todoId ?? 0;
        }

        public string ReadStringFromUser(int minLength, int maxLength, string inputPrompt = "Please enter description: ")
        {
            string description = "";

            do
            {
                if(description is not (null or ""))
                {
                    Console.Error.WriteLine($"Description should be between {minLength} and {maxLength}.");
                }
                Console.Write(inputPrompt);
                description = Console.ReadLine();

            } while (description.Length < minLength || description.Length > maxLength);

            return description;
        }

        public DateTime ReadDateFromUser(string format, string inputPrompt = "Please enter target date: ")
        {
            string targetTimeStr = "";
            DateTime? targetTime = null;

            do
            {
                Console.Write(inputPrompt);
                targetTimeStr = Console.ReadLine();

                try
                {
                    DateTime parsedTime = targetTimeStr.ToDateTime(format);

                    // Check whether there is overlap among the todos.
                    if (_todoMgr.IsOverlapping(parsedTime))
                    {
                        Console.Error.WriteLine($"The entered time is overlapping with another todo.");
                        Console.Error.WriteLine($"Please ensure there is a gap of {_config.MinGapBetweenTodosInMinutes} minutes between todos.");
                    }
                    else
                    {
                        targetTime = parsedTime;
                    }
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine($"Entered date is wrong. Please enter target time in the format: {format}");
                    Console.Error.WriteLine($"For example, 29 Oct 2017 10:00 PM, 14 Jan 2018 08:00 AM");
                }

            } while (targetTime == null);

            return targetTime.Value;
        }

        

        private UserAction GetUserAction()
        {
            string userChoice = "";
            Dictionary<string, UserAction> userActionMapping = new()
            {
                ["list"] = UserAction.List,
                ["add"] = UserAction.Add,
                ["remove"] = UserAction.Remove,
                ["exit"] = UserAction.Exit
            };

            do
            {
                if(userChoice is not (null or ""))
                {
                    Console.Error.WriteLine($"Input value \"{userChoice}\" is wrong.");
                }

                Console.Write($"Enter your option({string.Join("/", userActionMapping.Keys)}): ");
                userChoice = Console.ReadLine().ToLower();

            } while (!userActionMapping.ContainsKey(userChoice));

            return userActionMapping[userChoice];
        }
    }
}
