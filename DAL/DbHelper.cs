using Microsoft.Data.SqlClient;
using ItemProcessingSystemCore.Models;

namespace ItemProcessingSystemCore.DAL
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new Exception("Connection string not found");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

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

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if relation exists: {ex.Message}");
                return false;
            }
        }

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