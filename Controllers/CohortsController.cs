//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Threading.Tasks;
//using StudentsExercisesAPI.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;

//namespace StudentsExercisesAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class CohortsController : ControllerBase
//    {
//        private readonly IConfiguration _config;

//        public CohortsController(IConfiguration config)
//        {
//            _config = config;
//        }

//        public SqlConnection Connection
//        {
//            get
//            {
//                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
//            }
//        }

//        [HttpGet]
//        public ActionResult<List<Exercise>> Get([FromQuery] string exerciseLanguage/*, [FromQuery] string sortBy*/)
//        {
//            if (exerciseLanguage == null)
//            {
//                exerciseLanguage = "";
//            };

//            //if (sortBy.ToLower() != "beantype" && sortBy == null)
//            //{
//            //    sortBy = "Id";
//            //};

//            using (SqlConnection conn = Connection)
//            {

//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"
//                            SELECT Id, Exercise_Name, Exercise_Language 
//                            FROM Exercises
//                            WHERE Exercise_Language LIKE '%' + @exerciseLanguage + '%'";

//                    cmd.Parameters.Add(new SqlParameter("@exerciseLanguage", exerciseLanguage));
//                    //cmd.Parameters.Add(new SqlParameter("@sorted", sortBy));
//                    SqlDataReader reader = cmd.ExecuteReader();
//                    List<Exercise> exercises = new List<Exercise>();

//                    while (reader.Read())
//                    {
//                        Exercise exercise = new Exercise
//                        {
//                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
//                            Exercise_Name = reader.GetString(reader.GetOrdinal("Exercise_Name")),
//                            Exercise_Language = reader.GetString(reader.GetOrdinal("Exercise_Language"))
//                        };

//                        exercises.Add(exercise);
//                    }
//                    reader.Close();

//                    return Ok(exercises);
//                }
//            }
//        }

//        [HttpGet("{id}", Name = "GetExercise")]
//        public ActionResult<Exercise> Get([FromRoute] int id)
//        {
//            using (SqlConnection conn = Connection)
//            {
//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"
//                        SELECT
//                            Id, Exercise_Name, Exercise_Language
//                        FROM Exercises
//                        WHERE Id = @id";
//                    cmd.Parameters.Add(new SqlParameter("@id", id));
//                    SqlDataReader reader = cmd.ExecuteReader();

//                    Exercise exercise = null;

//                    if (reader.Read())
//                    {
//                        exercise = new Exercise
//                        {
//                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
//                            Exercise_Name = reader.GetString(reader.GetOrdinal("Exercise_Name")),
//                            Exercise_Language = reader.GetString(reader.GetOrdinal("Exercise_Language"))
//                        };
//                    }
//                    else if (exercise == null)
//                    {
//                        return NotFound($"Exercise with the Id {id} was not found");
//                    }

//                    reader.Close();

//                    return Ok(exercise);
//                }
//            }
//        }

//        [HttpPost]
//        public ActionResult<Exercise> Post([FromBody] Exercise exercise)
//        {
//            using (SqlConnection conn = Connection)
//            {
//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"INSERT INTO Exercises (Exercise_Name, Exercise_Language)
//                                        OUTPUT INSERTED.Id
//                                        VALUES (@exerciseName, @exerciseLanguage)";
//                    cmd.Parameters.Add(new SqlParameter("@exerciseName", exercise.Exercise_Name));
//                    cmd.Parameters.Add(new SqlParameter("@exerciseLanguage", exercise.Exercise_Language));

//                    int newId = (int)cmd.ExecuteScalar();
//                    exercise.Id = newId;
//                    return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
//                }
//            }
//        }

//        [HttpPut("{id}")]
//        public ActionResult<Exercise> Put([FromRoute] int id, [FromRoute] Exercise exercise)
//        {
//            try
//            {
//                using (SqlConnection conn = Connection)
//                {
//                    conn.Open();
//                    using (SqlCommand cmd = conn.CreateCommand())
//                    {
//                        cmd.CommandText = @"UPDATE Exercise
//                                            SET Exercise_Name = @exerciseName,
//                                                Exercise_Language = @exerciseLanguage
//                                            WHERE Id = @id";
//                        cmd.Parameters.Add(new SqlParameter("@exerciseName", exercise.Exercise_Name));
//                        cmd.Parameters.Add(new SqlParameter("@ExerciseLanguage", exercise.Exercise_Language));
//                        cmd.Parameters.Add(new SqlParameter("@id", id));

//                        int rowsAffected = cmd.ExecuteNonQuery();
//                        if (rowsAffected > 0)
//                        {
//                            return new StatusCodeResult(StatusCodes.Status204NoContent);
//                        }
//                        throw new Exception("No rows affected");
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                if (!ExerciseExists(id))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }
//        }

//        [HttpDelete("{id}")]
//        public ActionResult<Exercise> Delete([FromRoute] int id)
//        {
//            try
//            {
//                using (SqlConnection conn = Connection)
//                {
//                    conn.Open();
//                    using (SqlCommand cmd = conn.CreateCommand())
//                    {
//                        cmd.CommandText = @"DELETE FROM Exercises WHERE Id = @id";
//                        cmd.Parameters.Add(new SqlParameter("@id", id));

//                        int rowsAffected = cmd.ExecuteNonQuery();
//                        if (rowsAffected > 0)
//                        {
//                            return new StatusCodeResult(StatusCodes.Status204NoContent);
//                        }
//                        throw new Exception("No rows affected");
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                if (!ExerciseExists(id))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }
//        }

//        private bool ExerciseExists(int id)
//        {
//            using (SqlConnection conn = Connection)
//            {
//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"
//                        SELECT Id, Exercise_Name, Exercise_Language
//                        FROM Exercises
//                        WHERE Id = @id";
//                    cmd.Parameters.Add(new SqlParameter("@id", id));

//                    SqlDataReader reader = cmd.ExecuteReader();
//                    return reader.Read();
//                }
//            }
//        }
//    }
//}