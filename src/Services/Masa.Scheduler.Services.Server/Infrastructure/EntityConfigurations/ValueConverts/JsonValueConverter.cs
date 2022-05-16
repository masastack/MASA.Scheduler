// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.EntityConfigurations.ValueConverts;

public class JsonValueConverter<T> : ValueConverter<T?, string> where T : class
{
    public JsonValueConverter()
        : base(x => SerializeObject(x), x => DeserializeObject(x))
    {

    }
   

    private static string SerializeObject(T? obj)
    {
        if (obj == null)
            return "";

        return JsonConvert.SerializeObject(obj);
    }

    private static T? DeserializeObject(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(json);
    }
}
