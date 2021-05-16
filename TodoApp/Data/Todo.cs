using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Data
{
    public class Todo: IComparable<Todo>, IEquatable<Todo>
    {
        private static int TodoCounter = 0;

        public int TodoId { get; set; }
        public string Description { get; set; }
        public DateTime TargetTime { get; set; }

        public Todo() 
        {
            TodoId = ++TodoCounter;
        }

        public int CompareTo(Todo other)
        {
            
            if (this.TargetTime < other.TargetTime)
                return -1;
            if (this.TargetTime > other.TargetTime)
                return 1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Todo objAsTodo = obj as Todo;

            return objAsTodo == null? false : Equals(objAsTodo);
        }
        public override int GetHashCode()
        {
            return Description.GetHashCode();
        }
        public bool Equals(Todo other)
        {
            if(other == null) return false;

            return (this.Description == other.Description && this.TargetTime == other.TargetTime);
        }
    }
}
