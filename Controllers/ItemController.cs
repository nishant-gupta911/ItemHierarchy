using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ItemProcessingSystemCore.DAL;
using ItemProcessingSystemCore.Models;
using System.Collections.Generic;

namespace ItemProcessingSystemCore.Controllers
{
    public class ItemController : Controller
    {
        private readonly DbHelper _db;

        public ItemController(DbHelper db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<Item> items = new List<Item>();

            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Items";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(new Item
                    {
                        ItemId = (int)reader["ItemId"],
                        Name = reader["Name"] != null ? reader["Name"].ToString() : null,
                        Weight = (double)reader["Weight"]
                    });
                }
            }

            return View(items);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Item item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();

                    string query = "INSERT INTO Items (Name, Weight) VALUES (@Name, @Weight)";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@Name", item.Name ?? "");
                    cmd.Parameters.AddWithValue("@Weight", item.Weight);

                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Item created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating item: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the item. Please try again.");
                return View(item);
            }
        }

        public IActionResult Edit(int id)
        {
            Item? item = null;

            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT ItemId, Name, Weight, CreatedAt FROM Items WHERE ItemId = @ItemId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ItemId", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        item = new Item
                        {
                            ItemId = (int)reader["ItemId"],
                            Name = reader["Name"] != null ? reader["Name"].ToString() : null,
                            Weight = (double)reader["Weight"],
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading item: {ex.Message}");
            }

            if (item == null)
            {
                TempData["ErrorMessage"] = "Item not found.";
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Item item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();

                    string query = "UPDATE Items SET Name = @Name, Weight = @Weight WHERE ItemId = @ItemId";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                    cmd.Parameters.AddWithValue("@Name", item.Name ?? "");
                    cmd.Parameters.AddWithValue("@Weight", item.Weight);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        TempData["ErrorMessage"] = "Item not found.";
                        return RedirectToAction("Index");
                    }
                }

                TempData["SuccessMessage"] = "Item updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating item: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating the item. Please try again.");
                return View(item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();

                    string deleteRelationsQuery = "DELETE FROM ItemRelations WHERE ParentItemId = @ItemId OR ChildItemId = @ItemId";
                    SqlCommand deleteRelationsCmd = new SqlCommand(deleteRelationsQuery, conn);
                    deleteRelationsCmd.Parameters.AddWithValue("@ItemId", id);
                    deleteRelationsCmd.ExecuteNonQuery();

                    string deleteItemQuery = "DELETE FROM Items WHERE ItemId = @ItemId";
                    SqlCommand deleteItemCmd = new SqlCommand(deleteItemQuery, conn);
                    deleteItemCmd.Parameters.AddWithValue("@ItemId", id);

                    int rowsAffected = deleteItemCmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        TempData["ErrorMessage"] = "Item not found.";
                        return RedirectToAction("Index");
                    }
                }

                TempData["SuccessMessage"] = "Item deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting item: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the item. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Process()
        {
            List<Item> items = new List<Item>();

            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT ItemId, Name FROM Items";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        items.Add(new Item
                        {
                            ItemId = (int)reader["ItemId"],
                            Name = reader["Name"] != null ? reader["Name"].ToString() : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Process: {ex.Message}");
            }

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Process(int parentId, int[] childIds)
        {
            if (childIds == null || childIds.Length == 0)
            {
                ModelState.AddModelError("", "Please select at least one child item.");
                return RedirectToAction("Process");
            }

            if (childIds.Contains(parentId))
            {
                ModelState.AddModelError("", "Cannot select the same item as both parent and child.");
                return RedirectToAction("Process");
            }

            try
            {
                List<ItemRelation> existingRelations = _db.GetAllRelations();

                foreach (int childId in childIds)
                {
                    if (_db.RelationExists(parentId, childId))
                    {
                        ModelState.AddModelError("", $"Relation already exists between selected items.");
                        return RedirectToAction("Process");
                    }

                    if (WouldCreateCircle(parentId, childId, existingRelations))
                    {
                        ModelState.AddModelError("", $"Creating this relation would result in a circular dependency.");
                        return RedirectToAction("Process");
                    }

                    _db.InsertRelation(parentId, childId);
                }

                TempData["SuccessMessage"] = $"Successfully linked {childIds.Length} child item(s) to parent.";
                return RedirectToAction("Tree");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Process POST: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while processing relations. Please try again.");
                return RedirectToAction("Process");
            }
        }

        private bool WouldCreateCircle(int parentId, int childId, List<ItemRelation> relations)
        {
            // simple check - if same item selected as parent and child
            if (parentId == childId)
                return true;

            // recursively check if child is ancestor of parent
            return CheckCircularDependency(parentId, childId, relations);
        }

        private bool CheckCircularDependency(int currentId, int targetId, List<ItemRelation> relations)
        {
            var parents = relations.Where(r => r.ChildItemId == currentId).Select(r => r.ParentItemId).ToList();
            
            foreach (var parentId in parents)
            {
                if (parentId == targetId)
                    return true;

                // recursive check
                if (CheckCircularDependency(parentId, targetId, relations))
                    return true;
            }

            return false;
        }

        public IActionResult Tree()
        {
            List<Item> items = new List<Item>();
            List<ItemRelation> relations = new List<ItemRelation>();

            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();

                    SqlCommand cmd1 = new SqlCommand("SELECT * FROM Items", conn);
                    SqlDataReader reader1 = cmd1.ExecuteReader();
                    while (reader1.Read())
                    {
                        items.Add(new Item
                        {
                            ItemId = (int)reader1["ItemId"],
                            Name = reader1["Name"] != null ? reader1["Name"].ToString() : null
                        });
                    }
                    reader1.Close();

                    SqlCommand cmd2 = new SqlCommand("SELECT * FROM ItemRelations", conn);
                    SqlDataReader reader2 = cmd2.ExecuteReader();
                    while (reader2.Read())
                    {
                        relations.Add(new ItemRelation
                        {
                            ParentItemId = (int)reader2["ParentItemId"],
                            ChildItemId = (int)reader2["ChildItemId"]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Tree: {ex.Message}");
            }

            ViewBag.Items = items;
            ViewBag.Relations = relations;
            ViewBag.TreeHtml = BuildTreeHtml(items, relations);

            return View();
        }

        private string BuildTreeHtml(List<Item> items, List<ItemRelation> relations)
        {
            try
            {
                if (items == null || items.Count == 0)
                    return "<p>No items yet. <a href=\"/Item/Create\">Create one</a>.</p>";

                if (relations == null || relations.Count == 0)
                {
                    string html = "<p>No relations defined. Items:</p><ul>";
                    foreach (var item in items)
                    {
                        html += $"<li>{item.Name} (Weight: {item.Weight})</li>";
                    }
                    html += "</ul>";
                    return html;
                }

                // build a mapping of parent -> children
                Dictionary<int, List<ItemRelation>> childrenMap = new Dictionary<int, List<ItemRelation>>();
                foreach (var rel in relations)
                {
                    if (!childrenMap.ContainsKey(rel.ParentItemId))
                        childrenMap[rel.ParentItemId] = new List<ItemRelation>();
                    childrenMap[rel.ParentItemId].Add(rel);
                }

                // find root items (items that are not children of any other)
                HashSet<int> childIds = new HashSet<int>(relations.Select(r => r.ChildItemId));
                var roots = items.Where(i => !childIds.Contains(i.ItemId)).ToList();

                // build HTML recursively
                string output = "<div style=\"padding: 15px;\">";
                foreach (var root in roots)
                {
                    output += BuildItemTree(root, childrenMap, items);
                }

                // add orphaned items
                var orphaned = items.Where(i => !childIds.Contains(i.ItemId) && !childrenMap.ContainsKey(i.ItemId)).ToList();
                if (orphaned.Count > 0)
                {
                    output += "<div style=\"margin-top: 20px; padding: 10px; background: #ffffcc; border: 1px solid #ccc;\">";
                    output += "<p><strong>Items with no relations:</strong></p>";
                    foreach (var item in orphaned)
                    {
                        output += $"<p>• {item.Name} (Weight: {item.Weight})</p>";
                    }
                    output += "</div>";
                }
                output += "</div>";
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building tree: {ex.Message}");
                return "<p style=\"color: red;\">Error rendering tree</p>";
            }
        }

        private string BuildItemTree(Item item, Dictionary<int, List<ItemRelation>> childrenMap, List<Item> allItems)
        {
            string html = $"<div style=\"margin-bottom: 15px; padding: 10px; border: 1px solid #ddd; border-radius: 4px;\">";
            html += $"<strong>{item.Name}</strong> (Weight: {item.Weight})";

            if (childrenMap.ContainsKey(item.ItemId))
            {
                html += "<ul>";
                foreach (var relation in childrenMap[item.ItemId])
                {
                    var child = allItems.FirstOrDefault(i => i.ItemId == relation.ChildItemId);
                    if (child != null)
                    {
                        html += $"<li>{BuildItemTree(child, childrenMap, allItems)}</li>";
                    }
                }
                html += "</ul>";
            }
            html += "</div>";
            return html;
        }

    }
}