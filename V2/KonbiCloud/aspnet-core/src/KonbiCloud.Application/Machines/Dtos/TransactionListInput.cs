using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Machines.Dtos
{
    public class TransactionListInput : IPagedAndSortedResultRequest
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Sorting { get; set; }

        public string MachineID { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string EmployeeId { get; set; }
    }

    public class DetailTransactionDto 
    {
        public long TransactionID { get; set; }
        //public long LocalId { get; set; }
        public string MachineId { get; set; }
        public string MachineLogicalId { get; set; }
        public string LocationCode { get; set; }
        public DateTime PaymentTime { get; set; }
        public double TotalValue { get; set; }
        public double SumCollected { get; set; }
        public double ChangeIssued { get; set; }
        public string ProductSKU { get; set; }
        public string PaymentMethod { get; set; }
        public string ProductName { get; set; }
        public long RemainProduct { get; set; }
        public KonbiCloud.Enums.TransactionStatus State { get; set; }

        public string Tid { get; set; }
        public string Mid { get; set; }
        public string Invoice { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public string ApproveCode { get; set; }
        public string Id { get; set; }
    }

    public class TransactionOfInventoryDto
    {
        public long TransactionID { get; set; }
        public string MachineId { get; set; }
        public string MachineLogicalId { get; set; }
        public string LocationCode { get; set; }
        public DateTime Time { get; set; }        
        public string ProductSKU { get; set; }       
        public string ProductName { get; set; }
        public long RemainProduct { get; set; }
    }
}
