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
                        Name = reader["Name"]?.ToString() ?? "",
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
            Item item = null;

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
                            Name = reader["Name"]?.ToString() ?? "",
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
                            Name = reader["Name"]?.ToString() ?? ""
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
            if (parentId == childId)
                return true;

            Queue<int> queue = new Queue<int>();
            HashSet<int> visited = new HashSet<int>();
            queue.Enqueue(parentId);
            visited.Add(parentId);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                var parentsOfCurrent = relations
                    .Where(r => r.ChildItemId == currentId)
                    .Select(r => r.ParentItemId)
                    .ToList();

                foreach (int parentOfCurrent in parentsOfCurrent)
                {
                    if (parentOfCurrent == childId)
                        return true;

                    if (!visited.Contains(parentOfCurrent))
                    {
                        visited.Add(parentOfCurrent);
                        queue.Enqueue(parentOfCurrent);
                    }
                }
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
                            Name = reader1["Name"]?.ToString() ?? ""
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
                {
                    return "<p style=\"color: #666; font-style: italic; padding: 10px; background-color: #f8f9fa; border-radius: 4px;\">No items in the system yet. <a href=\"/Item/Create\">Create one now</a>.</p>";
                }

                if (relations == null || relations.Count == 0)
                {
                    var noRelationsHtml = "<p style=\"color: #ff9800; padding: 10px; background-color: #fff3cd; border-radius: 4px;\"><strong>No item relations defined.</strong> All items are displayed below:</p>";
                    noRelationsHtml += "<div style=\"padding: 10px; background-color: #f8f9fa; border-radius: 4px; border-left: 4px solid #ffc107;\">";
                    foreach (var item in items)
                    {
                        noRelationsHtml += $"<p style=\"margin: 5px 0;\">• <strong>{SecurityEncode(item.Name)}</strong> (Weight: {item.Weight})</p>";
                    }
                    noRelationsHtml += "</div>";
                    return noRelationsHtml;
                }

                var childItemIds = relations.Select(r => r.ChildItemId).ToHashSet();
                var childrenByParent = new Dictionary<int, List<ItemRelation>>();
                foreach (var rel in relations)
                {
                    if (!childrenByParent.ContainsKey(rel.ParentItemId))
                        childrenByParent[rel.ParentItemId] = new List<ItemRelation>();
                    childrenByParent[rel.ParentItemId].Add(rel);
                }

                Func<int, string> renderTree = null;
                renderTree = (parentId) =>
                {
                    if (!childrenByParent.ContainsKey(parentId))
                        return "";

                    var children = childrenByParent[parentId];
                    if (children.Count == 0)
                        return "";

                    string html = "<ul style=\"list-style: none; padding-left: 20px; margin: 5px 0;\">";
                    foreach (var child in children)
                    {
                        var childItem = items?.FirstOrDefault(i => i.ItemId == child.ChildItemId);
                        if (childItem == null)
                            continue;

                        html += $"<li style=\"margin-bottom: 5px;\"><strong>{SecurityEncode(childItem.Name)}</strong> (Weight: {childItem.Weight})<br />";
                        html += renderTree(child.ChildItemId);
                        html += "</li>";
                    }
                    html += "</ul>";
                    return html;
                };

                var output = "<div style=\"padding: 15px; background-color: #f8f9fa; border-radius: 4px;\">";
                output += "<h3>Hierarchy Structure</h3>";
                foreach (var item in items.Where(i => !childItemIds.Contains(i.ItemId)))
                {
                    output += "<div style=\"margin-bottom: 20px; padding: 10px; border: 1px solid #ddd; border-radius: 4px; background-color: white;\">";
                    output += $"<p style=\"margin: 0; margin-bottom: 8px;\"><strong style=\"font-size: 1.1em; color: #007bff;\">📦 {SecurityEncode(item.Name)}</strong> (Weight: {item.Weight})</p>";
                    output += renderTree(item.ItemId);
                    output += "</div>";
                }

                var orphanedItems = items.Where(i => !childItemIds.Contains(i.ItemId) && !childrenByParent.ContainsKey(i.ItemId)).ToList();
                if (orphanedItems.Count > 0)
                {
                    output += "<hr style=\"margin: 20px 0; border: none; border-top: 1px solid #ddd;\" />";
                    output += "<div style=\"padding: 10px; background-color: #fff3cd; border-radius: 4px; border-left: 4px solid #ffc107;\">";
                    output += "<p><em style=\"color: #856404;\"><strong>Items with no relations:</strong></em></p>";
                    foreach (var item in orphanedItems)
                    {
                        output += $"<p style=\"margin: 5px 0; color: #856404;\">• {SecurityEncode(item.Name)} (Weight: {item.Weight})</p>";
                    }
                    output += "</div>";
                }

                output += "</div>";
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BuildTreeHtml: {ex.Message}");
                return "<p style=\"color: red;\">Error rendering tree. Please try again.</p>";
            }
        }

        private string SecurityEncode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return System.Net.WebUtility.HtmlEncode(input);
        }
    }
}