// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Text;

namespace Masa.Scheduler.Services.Server.Application.Jobs.Queries;

public record SchedulerJobQueryByIdentity(GetSchedulerJobByIdentityRequest Request) : IQuery<SchedulerJobDto?>
{
    private Guid _eventId;

    private DateTime _creationTime;

    public SchedulerJobQueryByIdentity()
        : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }

    public SchedulerJobQueryByIdentity(Guid eventId, DateTime creationTime)
        :this(new GetSchedulerJobByIdentityRequest())
    {
        _eventId = eventId;
        _creationTime = creationTime;
    }

    public Guid GetEventId()
    {
        return _eventId;
    }

    public void SetEventId(Guid eventId)
    {
        _eventId = eventId;
    }

    public DateTime GetCreationTime()
    {
        return _creationTime;
    }

    public void SetCreationTime(DateTime creationTime)
    {
        _creationTime = creationTime;
    }

    [CompilerGenerated]
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("Query");
        stringBuilder.Append(" { ");
        if (PrintMembers(stringBuilder))
        {
            stringBuilder.Append(' ');
        }

        stringBuilder.Append('}');
        return stringBuilder.ToString();
    }

    [CompilerGenerated]
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append("Result = ");
        builder.Append(Result);
        return true;
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
        return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(_eventId)) * -1521134295 + EqualityComparer<DateTime>.Default.GetHashCode(_creationTime);
    }

    [CompilerGenerated]
    public virtual bool Equals(SchedulerJobQueryByIdentity? other)
    {
        return (object)this == other || (other != null && EqualityContract == other!.EqualityContract && EqualityComparer<Guid>.Default.Equals(_eventId, other!._eventId) && EqualityComparer<DateTime>.Default.Equals(_creationTime, other!._creationTime));
    }

    [CompilerGenerated]
    protected SchedulerJobQueryByIdentity(SchedulerJobQueryByIdentity original)
    {
        _eventId = original._eventId;
        _creationTime = original._creationTime;
    }

    public SchedulerJobDto? Result { get; set; }
}
