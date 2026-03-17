// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public static class DaprJobsTimeCalculator
{
    private static TimeZoneInfo GetChinaStandardTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.Local;
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.Local;
            }
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }

    public static Task<List<DateTimeOffset>> GetCronExecuteTimeByTimeRange(string cron, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var executeTimeList = new List<DateTimeOffset>();
        var cronExpression = new CronExpression(cron)
        {
            TimeZone = GetChinaStandardTimeZone()
        };

        while (startTime < endTime)
        {
            var nextExecuteTime = cronExpression.GetNextValidTimeAfter(startTime);
            if (nextExecuteTime == null)
            {
                break;
            }

            if (nextExecuteTime.Value <= startTime)
            {
                break;
            }

            startTime = nextExecuteTime.Value;
            if (nextExecuteTime < endTime)
            {
                executeTimeList.Add(nextExecuteTime.Value);
            }
        }

        return Task.FromResult(executeTimeList);
    }
}
