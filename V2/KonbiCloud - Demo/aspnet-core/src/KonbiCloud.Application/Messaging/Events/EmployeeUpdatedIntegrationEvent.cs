
using System;
using Abp.AutoMapper;
using KonbiCloud.Employees;

namespace KonbiCloud.Messaging.Events
{
    [AutoMapFrom(typeof(Employee))]
    public class EmployeeUpdatedIntegrationEvent: IntegrationEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CardId { get; set; }
        public double Quota { get; set; }
        public string Period { get; set; }
        public double Ordered { get; set; }
        public int TenantId { get; set; }
        public bool QuotaCash { get; set; }
        public bool IsDeleted { get; set; }

    }
    public class EmployeeDeletedIntegrationEvent : IntegrationEvent
    {
        public Guid Id { get; set; }
    }
}
