using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentsExercisesAPI.Models
{
    public class Cohort
    {
        public int Id { get; set; }
        public string Cohort_Name { get; set; }

        public List<Student> Students { get; set; }
        public List<Instructor> Instructors { get; set; }
    }
}
