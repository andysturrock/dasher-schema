using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dasher.Schema.Comparison
{
    public class FieldDifference
    {
        public enum DifferenceLevelEnum
        {
            Warning,
            Critical
        }

        public FieldDifference(Field field, string description, DifferenceLevelEnum differenceLevel)
        {
            Field = field;
            Description = description;
            DifferenceLevel = differenceLevel;
        }

        public Field Field { get; }

        public String Description { get; }

        public DifferenceLevelEnum DifferenceLevel { get; }
    }
}
