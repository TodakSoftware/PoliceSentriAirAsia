using System.Web.Mvc;
using System.Web.Routing;

public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "GameRoute",
            url: "games/police-sentri/index.html",
            defaults: new { controller = "Game", action = "Play" }
        );
    }
}
