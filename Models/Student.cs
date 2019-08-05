using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentsExercisesAPI.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public int Cohort_Id { get; set; }
        public Cohort Cohort { get; set; }
        public List<Exercise> Exercises { get; set; }
    }
}
