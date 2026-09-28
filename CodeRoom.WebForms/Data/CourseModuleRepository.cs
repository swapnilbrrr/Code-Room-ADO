using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>ADO.NET access to CourseModules, the grouping stage inside a course.</summary>
    public class CourseModuleRepository
    {
        private const string Columns = "Id, CourseId, Title, Description, ModuleOrder";

        public List<CourseModule> GetByCourse(int courseId)
        {
            const string sql =
                "SELECT " + Columns + " FROM dbo.CourseModules WHERE CourseId = @CourseId ORDER BY ModuleOrder;";

            var modules = SqlHelper.ReadList(sql, Map,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });

            return modules;
        }

        public CourseModule GetById(int moduleId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.CourseModules WHERE Id = @ModuleId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@ModuleId", SqlDbType.Int) { Value = moduleId });
        }

        /// <summary>Modules with their lessons attached, in the order the course page renders them.</summary>
        public List<CourseModule> GetCourseOutline(int courseId)
        {
            var modules = GetByCourse(courseId);

            foreach (var module in modules)
            {
                module.Lessons = LessonRepository.GetByModule(module.Id);
            }

            return modules;
        }

        public int HighestOrder(int courseId)
        {
            const string sql = "SELECT ISNULL(MAX(ModuleOrder), 0) FROM dbo.CourseModules WHERE CourseId = @CourseId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId }));
        }

        public int Insert(SqlConnection connection, SqlTransaction transaction, CourseModule module)
        {
            const string sql =
                "INSERT INTO dbo.CourseModules (CourseId, Title, Description, ModuleOrder) " +
                "OUTPUT INSERTED.Id VALUES (@CourseId, @Title, @Description, @ModuleOrder);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@CourseId", module.CourseId);
                SqlHelper.AddNVarChar(command, "@Title", module.Title, 120);
                SqlHelper.AddNVarChar(command, "@Description", module.Description, 500);
                SqlHelper.AddInt(command, "@ModuleOrder", module.Order);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Insert(CourseModule module)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, module));
        }

        public void Update(SqlConnection connection, SqlTransaction transaction, CourseModule module)
        {
            const string sql =
                "UPDATE dbo.CourseModules SET Title = @Title, Description = @Description, " +
                "ModuleOrder = @ModuleOrder WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddNVarChar(command, "@Title", module.Title, 120);
                SqlHelper.AddNVarChar(command, "@Description", module.Description, 500);
                SqlHelper.AddInt(command, "@ModuleOrder", module.Order);
                SqlHelper.AddInt(command, "@Id", module.Id);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Delete a module. Lessons keep existing: the original model set CourseModuleId to NULL, which
        /// SQL Server cannot express here because Courses already cascade to Lessons, so the reference is
        /// cleared explicitly before the module row goes.
        /// </summary>
        public void Delete(SqlConnection connection, SqlTransaction transaction, int moduleId)
        {
            using (var command = SqlHelper.Prepare(connection, transaction,
                "UPDATE dbo.Lessons SET CourseModuleId = NULL WHERE CourseModuleId = @ModuleId;"))
            {
                SqlHelper.AddInt(command, "@ModuleId", moduleId);
                command.ExecuteNonQuery();
            }

            using (var command = SqlHelper.Prepare(connection, transaction,
                "DELETE FROM dbo.CourseModules WHERE Id = @ModuleId;"))
            {
                SqlHelper.AddInt(command, "@ModuleId", moduleId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int moduleId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, moduleId));
        }

        internal static CourseModule Map(SqlDataReader reader)
        {
            return new CourseModule
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                Title = SqlHelper.GetString(reader, "Title"),
                Description = SqlHelper.GetString(reader, "Description"),
                Order = SqlHelper.GetInt(reader, "ModuleOrder")
            };
        }
    }
}
