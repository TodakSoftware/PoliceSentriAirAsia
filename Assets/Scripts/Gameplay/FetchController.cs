using System.Web.Mvc;
using UnityEngine;

public class FetchController : Controller
{
    public ActionResult Play()
    {
        string nickname = (string)RouteData.Values["nickname"];
        string avatarUrl = (string)RouteData.Values["avatar"];

        // do something with nickname and avatarUrl
        // ...

        return View();
    }
}
