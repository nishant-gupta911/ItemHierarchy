using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ItemProcessingSystemCore.DAL;

namespace ItemProcessingSystemCore.Controllers
{
    public class TestController : Controller
    {
        private readonly DbHelper _db;

        public TestController(DbHelper db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                return Content("Database Connected Successfully");
            }
        }
    }
}