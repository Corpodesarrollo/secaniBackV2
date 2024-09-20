using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Repositorios
{
    public class JobSchedule
    {
        public JobSchedule(Type jobType, string cronExpression, TimeZoneInfo timeZone)
        {
            JobType = jobType;
            CronExpression = cronExpression;
            TimeZone = timeZone;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
        public TimeZoneInfo TimeZone { get; }
    }
}
