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
    public class CohortsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config)
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
        public ActionResult<List<Student>> Get([FromQuery] string cohortName/*, [FromQuery] string sortBy*/)
        {
            if (cohortName == null)
            {
                cohortName = "";
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
                            SELECT Cohorts.Id, Cohort_Name
                            FROM Cohorts
                            WHERE Cohort_Name LIKE '%' + @cohortName + '%'";

                    cmd.Parameters.Add(new SqlParameter("@cohortName", cohortName));
                    //cmd.Parameters.Add(new SqlParameter("@sorted", sortBy));
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Cohort_Name = reader.GetString(reader.GetOrdinal("Cohort_Name")),
                            Students = new List<Student>(),
                            Instructors = new List<Instructor>()
                        };

                        cohorts.Add(cohort);
                    }
                    reader.Close();

                    return Ok(cohorts);
                }
            }
        }

        [HttpGet("{id}", Name = "GetCohort")]
        public ActionResult<Cohort> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Cohort_Name
                        FROM Cohorts
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    if (reader.Read())
                    {
                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Cohort_Name = reader.GetString(reader.GetOrdinal("Cohort_Name")),
                            Instructors = new List<Instructor>(),
                            Students = new List<Student>()
                        };
                    }
                    else if (cohort == null)
                    {
                        return NotFound($"Student with the Id {id} was not found");
                    }

                    reader.Close();

                    return Ok(cohort);
                }
            }
        }

        [HttpPost]
        public ActionResult<Cohort> Post([FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Cohorts (Cohort_Name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@cohortName)";
                    cmd.Parameters.Add(new SqlParameter("@cohortName", cohort.Cohort_Name));

                    int newId = (int)cmd.ExecuteScalar();
                    cohort.Id = newId;
                    return CreatedAtRoute("GetCohort", new { id = newId }, cohort);
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
        public ActionResult<Cohort> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Cohorts WHERE Id = @id";
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
                if (!CohortExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CohortExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Cohort_Name
                        FROM Cohorts
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}