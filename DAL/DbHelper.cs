using Microsoft.Data.SqlClient;
using ItemProcessingSystemCore.Models;

namespace ItemProcessingSystemCore.DAL
{
    /// <summary>
    /// Database access layer for SQL Server operations
    /// Handles connection management and common database operations
    /// </summary>
    public class DbHelper
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes the DbHelper with connection string from configuration
        /// </summary>
        /// <param name="configuration">Application configuration containing connection strings</param>
        public DbHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new Exception("Connection string not found");
        }

        /// <summary>
        /// Gets a new SQL connection instance
        /// </summary>
        /// <returns>New SqlConnection to the configured database</returns>
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Checks if a relationship exists between parent and child items
        /// </summary>
        /// <param name="parentItemId">The ID of the parent item</param>
        /// <param name="childItemId">The ID of the child item</param>
        /// <returns>True if relationship exists, false otherwise</returns>
        public bool RelationExists(int parentItemId, int childItemId)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM ItemRelations WHERE ParentItemId = @ParentId AND ChildItemId = @ChildId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ParentId", parentItemId);
                    cmd.Parameters.AddWithValue("@ChildId", childItemId);
                    cmd.CommandTimeout = 30;

                    int count = (int)cmd.ExecuteScalar() ?? 0;
                    return count > 0;
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Database error checking relation: {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if relation exists: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a new relationship between parent and child items
        /// </summary>
        /// <param name="parentItemId">The ID of the parent item</param>
        /// <param name="childItemId">The ID of the child item</param>
        /// <returns>True if relationship was created successfully, false otherwise</returns>
        public bool InsertRelation(int parentItemId, int childItemId)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO ItemRelations (ParentItemId, ChildItemId) VALUES (@ParentId, @ChildId)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ParentId", parentItemId);
                    cmd.Parameters.AddWithValue("@ChildId", childItemId);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting relation: {ex.Message}");
                return false;
            }
        }

        public List<ItemRelation> GetAllRelations()
        {
            List<ItemRelation> relations = new List<ItemRelation>();
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string query = "SELECT ParentItemId, ChildItemId FROM ItemRelations";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        relations.Add(new ItemRelation
                        {
                            ParentItemId = (int)reader["ParentItemId"],
                            ChildItemId = (int)reader["ChildItemId"]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting relations: {ex.Message}");
            }

            return relations;
        }
    }
}