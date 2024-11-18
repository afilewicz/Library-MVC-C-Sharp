using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

public class HelloWorldController : Controller
{
    // GET
    // public IActionResult Index()
    // {
    //     return View();
    // }

    public IActionResult Welcome(string name, int age = 1)
    {
        ViewData["Message"] = "Hello " + name;
        ViewData["Age"] = age;
        return View();
    }
}