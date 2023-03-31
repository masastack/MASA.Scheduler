// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class AuditedEntityDto
{
    public DateTime CreationTime { get; set; }

    public Guid Creator { get; set; }    

    public string CreatorName { get; set; } = string.Empty;

    public DateTime ModificationTime { get; set; }

    public Guid Modifier { get; set; }

    public string ModifierName { get; set; } = string.Empty;
}
