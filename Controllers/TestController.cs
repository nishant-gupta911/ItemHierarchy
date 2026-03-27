using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ItemProcessingSystemCore.DAL;

namespace ItemProcessingSystemCore.Controllers
{
    /// <summary>
    /// Test controller for development and debugging
    /// Provides endpoints to verify database connectivity
    /// </summary>
    public class TestController : Controller
    {
        private readonly DbHelper _db;

        public TestController(DbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// Tests database connection
        /// </summary>
        /// <returns>Success message if connected, exception if failed</returns>
        public IActionResult Index()
        {
            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();
                    return Content("Database Connected Successfully");
                }
            }
            catch (Exception ex)
            {
                return Content($"Database Connection Failed: {ex.Message}");
            }
        }
    }
}