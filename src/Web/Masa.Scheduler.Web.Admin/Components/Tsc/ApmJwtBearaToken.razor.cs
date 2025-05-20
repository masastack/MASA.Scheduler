// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.IdentityModel.Tokens.Jwt;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Masa.Scheduler.Web.Admin.Components.Tsc;

public partial class ApmJwtBearaToken
{
    [Parameter]
    public string Value { get; set; } = default!;

    string? _header = default;
    string? _payload = default;
    string? _text = default;
    JwtSecurityTokenHandler handler = new();
    JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ConvertToJsonString();
    }

    private void ConvertToJsonString()
    {
        _header = default;
        _payload = default;
        if (string.IsNullOrEmpty(Value))
            return;
        if (Value.Split(' ').Length == 2)
            Value = Value.Split(' ')[1];

        try
        {
            var jwtToken = handler.ReadJwtToken(Value);
            _header = JsonSerializer.Serialize(jwtToken.Header, _options);
            _payload = JsonSerializer.Serialize(jwtToken.Payload, _options);
            _text = $"{_header}\r\n\r\n{_payload}";
        }
        catch
        {

        }
    }
}