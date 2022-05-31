// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Responses;

public abstract class BaseResponse : BaseMessage
{ 
    public BaseResponse(Guid correlationId) : base()
    {
        base._correlationId = correlationId;
    }
    public BaseResponse()
    {
    }
}

public abstract class BaseResponse<T> : BaseResponse where T : class, new()
{
    public T Data { get; set; } = new();
}
