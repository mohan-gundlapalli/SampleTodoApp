using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp
{
    public class AppConfiguration
    {
        public string TodoFileName { get; set; }

        public string TodoDateFormat { get; set; }

        public int DescriptionLength { get; set; }
        public int DateTimeLength { get; set; }

        public int MinGapBetweenTodosInMinutes { get; set; }

    }
}
