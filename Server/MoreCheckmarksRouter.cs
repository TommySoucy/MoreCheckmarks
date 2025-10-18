using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Text.Json;

namespace MoreCheckmarks;

[Injectable]
public class CustomStaticRouter : StaticRouter
{
    private static HttpResponseUtil? httpResponseUtil;
    private static JsonUtil? jsonUtil;
    private static MoreCheckmarksServer? server;
    private static ISptLogger<MoreCheckmarksServer>? logger;
    public CustomStaticRouter(JsonUtil _jsonUtil, HttpResponseUtil _httpResponseUtil)
        : base(
            _jsonUtil,
            GetCustomRoutes()
        )
    {
        jsonUtil = _jsonUtil;
        httpResponseUtil = _httpResponseUtil;
    }

    public void Set(MoreCheckmarksServer _server)
    {
        server = _server;
    }

    public void Set(ISptLogger<MoreCheckmarksServer> _logger)
    {
        logger = _logger;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction(
                "/MoreCheckmarksRoutes/quests",
                static async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await HandleQuestsRoute(sessionId)
            ),
            new RouteAction(
                 "/MoreCheckmarksRoutes/assorts",
                 static async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await HandleAssortsRoute()
            ),
            new RouteAction(
                 "/MoreCheckmarksRoutes/items",
                 static async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await HandleItemsRoute()
            ),
            new RouteAction(
                 "/MoreCheckmarksRoutes/locales",
                 static async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await HandleLocalesRoute()
            ),
            new RouteAction(
                "/MoreCheckmarksRoutes/productions",
                static async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await HandleProductionsRoute()
            ),
        ];
    }

    private static ValueTask<string> HandleQuestsRoute(MongoId sessionId)
    {
        try
        {
            var quests = server!.HandleQuests(sessionId);
            return new ValueTask<string>(jsonUtil!.Serialize(quests!)!);
        }
        catch
        {
            logger?.Error("Exception when handling QuestsRoute!");
            return new ValueTask<string>(httpResponseUtil!.NullResponse());
        }
    }

    private static ValueTask<string> HandleAssortsRoute()
    {
        try
        {
            var assorts = server!.HandleAssorts();
            return new ValueTask<string>(jsonUtil!.Serialize(assorts!)!);
        }
        catch
        {
            logger?.Error("Exception when handling AssortsRoute!");
            return new ValueTask<string>(httpResponseUtil!.NullResponse());
        }
    }

    private static ValueTask<string> HandleItemsRoute()
    {
        try
        {
            var items = server!.HandleItems();
            return new ValueTask<string>(jsonUtil!.Serialize(items!)!);
        }
        catch
        {
            logger?.Error("Exception when handling ItemsRoute!");
            return new ValueTask<string>(httpResponseUtil!.NullResponse());
        }
    }

    private static ValueTask<string> HandleLocalesRoute()
    {
        try
        {
            var locales = server!.HandleLocales();
            return new ValueTask<string>(jsonUtil!.Serialize(locales!)!);
        }
        catch
        {
            logger?.Error("Exception when handling LocalesRoute!");
            return new ValueTask<string>(httpResponseUtil!.NullResponse());
        }
    }

    private static ValueTask<string> HandleProductionsRoute()
    {
        try
        {
            var productions = server!.HandleProductions();
            return new ValueTask<string>(jsonUtil!.Serialize(productions!)!);
        }
        catch
        {
            logger?.Error("Exception when handling ProductionsRoute!");
            return new ValueTask<string>(httpResponseUtil!.NullResponse());
        }
    }


}
