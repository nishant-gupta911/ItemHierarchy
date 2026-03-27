using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ItemProcessingSystemCore.Models;

namespace ItemProcessingSystemCore.Controllers;

/// <summary>
/// Home controller for basic application pages
/// Handles landing page, privacy, and error pages
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Returns the home page view
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Returns the privacy policy page
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Handles application errors with error details
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
