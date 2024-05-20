using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace task4.Models
{
    public class StudentData
    {
        [JsonInclude]
        public long studentId { get; set; }

        [JsonInclude]
        public long courseId { get; set; }

        [JsonConstructor]
        public StudentData(long studentId, long courseId)
        {
            this.studentId = studentId;
            this.courseId = courseId;
        }
    }
}
