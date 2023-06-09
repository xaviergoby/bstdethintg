﻿using System.Net;

namespace ExplorerAPI.Utils;

public static class QueryStringService
{
    public static Uri CreateUrl(string path, Dictionary<string, object> parameter)
    {
        var urlParameters = new List<string>();
        foreach (var par in parameter)
        {
            urlParameters.Add(par.Value == null || string.IsNullOrWhiteSpace(par.Value.ToString()) ? null : $"{par.Key}={par.Value}");
        }

        var encodedParams = urlParameters
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(WebUtility.HtmlEncode)
            .Select((x, i) => i > 0 ? $"&{x}" : $"?{x}")
            .ToArray();
        var url = encodedParams.Length > 0 ? $"{path}{string.Join(string.Empty, encodedParams)}" : path;
        return new Uri(url);
    }

    public static bool IsAllValuesNumeric(string[] valueStrings)
    {
        var flag = true;
        foreach (var val in valueStrings)
        {
            if (int.TryParse(val, out var idResult))
            { continue; }

            flag = false;
            break;
        }

        return flag;
    }
    public static bool IsAllValuesString(string[] valueStrings)
    {
        var flag = true;
        foreach (var val in valueStrings)
        {
            if (!int.TryParse(val, out var idResult))
            { continue; }

            flag = false;
            break;
        }

        return flag;
    }
    public static string IsIdOrString(string[] valueStrings)
    {
        if (IsAllValuesNumeric(valueStrings))
        {
            return "Id";
        }

        if (IsAllValuesString(valueStrings))
        {
            return "Symbol";
        }
        throw new HttpRequestException("All Parameters must be Symbol or Id");
    }
}