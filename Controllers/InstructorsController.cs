using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using StudentsExercisesAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace StudentsExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public ActionResult<List<Instructor>> Get([FromQuery] string lastName/*, [FromQuery] string sortBy*/)
        {
            if (lastName == null)
            {
                lastName = "";
            };

            //if (sortBy.ToLower() != "beantype" && sortBy == null)
            //{
            //    sortBy = "Id";
            //};

            using (SqlConnection conn = Connection)
            {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT Instructors.Id, FirstName, LastName, SlackHandle, Cohort_Id, SpecialAttack, c.Cohort_Name
                            FROM Instructors
                            JOIN Cohorts c ON C.Id = Cohort_Id
                            WHERE LastName LIKE '%' + @lastName + '%'";

                    cmd.Parameters.Add(new SqlParameter("@lastName", lastName));
                    //cmd.Parameters.Add(new SqlParameter("@sorted", sortBy));
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Instructor> instructors = new List<Instructor>();

                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Cohort_Id")),
                            Cohort_Name = reader.GetString(reader.GetOrdinal("Cohort_Name")),
                            Students = new List<Student>(),
                            Instructors = new List<Instructor>()
                        };

                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Cohort_Id = reader.GetInt32(reader.GetOrdinal("Cohort_Id")),
                            SpecialAttack = reader.GetString(reader.GetOrdinal("SpecialAttack")),
                            Cohort = cohort
                        };

                        instructors.Add(instructor);
                    }
                    reader.Close();

                    return Ok(instructors);
                }
            }
        }

        [HttpGet("{id}", Name = "GetInstructor")]
        public ActionResult<Instructor> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, FirstName, LastName, SlackHandle, Cohort_Id, SpecialAttack
                        FROM Instructors
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    if (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Cohort_Id = reader.GetInt32(reader.GetOrdinal("Cohort_Id")),
                            SpecialAttack = reader.GetString(reader.GetOrdinal("SpecialAttack")),
                            Cohort = null
                        };
                    }
                    else if (instructor == null)
                    {
                        return NotFound($"Instructor with the Id {id} was not found");
                    }

                    reader.Close();

                    return Ok(instructor);
                }
            }
        }

        [HttpPost]
        public ActionResult<Instructor> Post([FromBody] Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructors (FirstName, LastName, SlackHandle, Cohort_Id, SpecialAttack)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @slackHandle, @cohortId, @specialAttack)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", instructor.Cohort_Id));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", instructor.SpecialAttack));

                    int newId = (int)cmd.ExecuteScalar();
                    instructor.Id = newId;
                    return CreatedAtRoute("GetInstructor", new { id = newId }, instructor);
                }
            }
        }

        //[HttpPut("{id}")]
        //public ActionResult<Student> Put([FromRoute] int id, [FromRoute] Student student)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"UPDATE Student
        //                                    SET FirstName = @firstName,
        //                                        LastName = @lastName,
        //                                        SlackHandle = @slackHandle,
        //                                        Cohort_Id = @cohortId
        //                                    WHERE Id = @id";
        //                cmd.Parameters.Add(new SqlParameter("@id", id));
        //                cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
        //                cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
        //                cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
        //                cmd.Parameters.Add(new SqlParameter("@cohortId", student.Cohort_Id));

        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!StudentExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        [HttpDelete("{id}")]
        public ActionResult<Instructor> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Instructors WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!InstructorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool InstructorExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, Cohort_Id, SpecialAttack
                        FROM Instructors
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}