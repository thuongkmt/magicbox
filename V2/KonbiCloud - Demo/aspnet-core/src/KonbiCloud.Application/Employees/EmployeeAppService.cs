using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.Employees.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KonbiCloud.Employees
{
    [AbpAuthorize(AppPermissions.Pages_Employees)]
    public class EmployeeAppService : KonbiCloudAppServiceBase, IEmployeeAppService
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;

        public EmployeeAppService(IRepository<Employee, Guid> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        //public async Task<ListResultDto<EmployeeDto>> GetEmployeeAndCountOrdered(int maxResultCount,
        //    int skipCount,
        //    string empId,
        //    string from,
        //    string to)
        //{
        //    var tenantId = AbpSession.TenantId ?? 0;

        //    List<Employee> employees = new List<Employee>();
        //    int totalItem = 0;
        //    try
        //    {
        //        Guid id = Guid.Empty;
        //        Guid.TryParse(empId, out id);

        //        var data = await _employeeRepository.GetAllListAsync(x => x.TenantId == tenantId && (id == Guid.Empty ? true : x.Id == id) && !x.IsDeleted);
        //        totalItem = data.Count();
        //        employees = data.Skip(skipCount)
        //                    .Take(maxResultCount)
        //                    .OrderBy(x => x.Name)
        //                    .ToList();

        //        var results = employees.Select(x => new EmployeeDto
        //        {
        //            Id = x.Id,
        //            Name = x.Name,
        //            CardId = x.CardId,
        //            Quota = x.Quota,
        //            Period = x.Period,
        //            Ordered = x.Ordered,
        //            QuotaCash = x.QuotaCash
        //        }).OrderBy(x => x.Name).ToArray();

        //        //Count emp transaction
        //        foreach (var emp in results)
        //        {
        //            var allItems = _transactionRepository.GetAll().Where(t => t.Employee.CardId == emp.CardId);

        //            if (!string.IsNullOrEmpty(from))
        //            {
        //                DateTime fromTime = DateTime.ParseExact(from, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        //                //allItems = allItems.Where(t => t.PaymentTime >= fromTime);
        //            }

        //            if (!string.IsNullOrEmpty(to))
        //            {
        //                DateTime toTime = DateTime.ParseExact(to, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        //                toTime = toTime.AddHours(24);
        //                //allItems = allItems.Where(t => t.PaymentTime <= toTime);
        //            }

        //            //allItems = allItems.GroupBy(a => a.LocalId).Select(g => g.OrderByDescending(t => t.PaymentTime).FirstOrDefault());
        //            //if (emp.QuotaCash)
        //            //{
        //            //    emp.Ordered = allItems.Where(x => x.State == TransactionStatus.Success).Sum(x => x.TotalValue);
        //            //}
        //            //else
        //            //{
        //            //    emp.Ordered = allItems.Count();
        //            //}
        //        }

        //        var output = new PageResultListDto<EmployeeDto>(results)
        //        {
        //            TotalCount = totalItem
        //        };
        //        return output;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("Get All Employee Error", ex);
        //    }
        //    return new PageResultListDto<EmployeeDto>(new List<EmployeeDto>());
        //}

        [AbpAuthorize(AppPermissions.Pages_Employees)]
        public async Task<PagedResultDto<EmployeeDto>> GetAllEmployee(EmployeeRequest request)
        {
            try
            {
                var employees = _employeeRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(request.NameFilter), e => e.Name.Equals(request.NameFilter));

                var totalCount = employees.Count();

                var dto = employees.Select(x => x.MapTo<EmployeeDto>());

                var results = await dto
                    .OrderBy(request.Sorting ?? "Name asc")
                    .PageBy(request)
                    .ToListAsync();

                return new PagedResultDto<EmployeeDto>(totalCount, results);
            }
            catch (UserFriendlyException ue)
            {
                throw ue;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new PagedResultDto<EmployeeDto>(0, new List<EmployeeDto>());
            }
        }

        //public async Task Create(EmployeeDto input)
        //{
        //    var employee = ObjectMapper.Map<Employee>(input);
        //    employee.TenantId = AbpSession.TenantId ?? 0;
        //    employee.Id = Guid.NewGuid();
        //    await _employeeRepository.InsertAsync(employee);
        //    var evt = employee.MapTo<EmployeeUpdatedIntegrationEvent>();
        //    _eventBus.Publish(evt);
        //    await CurrentUnitOfWork.SaveChangesAsync();
        //}

        //public async Task<Employee> Update(EmployeeDto input)
        //{
        //    var employee = await _employeeRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
        //    if (employee != null)
        //    {
        //        employee.Name = input.Name;
        //        employee.CardId = input.CardId;
        //        employee.Quota = input.Quota;
        //        employee.Period = input.Period;
        //        employee.Ordered = input.Ordered;
        //        employee.QuotaCash = input.QuotaCash;
        //        var evt = employee.MapTo<EmployeeUpdatedIntegrationEvent>();
        //        _eventBus.Publish(evt);
        //        await _employeeRepository.UpdateAsync(employee);
        //    }
        //    return employee;
        //}

        //public async Task Delete(EntityDto<Guid> input)
        //{
        //    var emp = await _employeeRepository.FirstOrDefaultAsync(e => e.Id == input.Id);
        //    var evt = new EmployeeDeletedIntegrationEvent
        //    {
        //        Id = input.Id
        //    };
        //    _eventBus.Publish(evt);
        //    await _employeeRepository.DeleteAsync(emp);
        //}

        //public async Task ImportEmployee(UploadEmployeeDto input)
        //{
        //    if (input == null || input.FileContent == null) return;
        //    try
        //    {
        //        string[] lines = input.FileContent.Split(Environment.NewLine);
        //        if (lines.Length < 1)
        //        {
        //            throw new Exception("Invalid format file");
        //        }

        //        for (int i = 1; i < lines.Length; i++)
        //        {
        //            string trimedLine = lines[i].Trim();
        //            if (!string.IsNullOrEmpty(trimedLine))
        //            {
        //                string[] items = trimedLine.Split(',');
        //                if (items.Length == 4)
        //                {
        //                    var employee = new Employee
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        Name = items[0],
        //                        CardId = items[1],
        //                        QuotaCash = true,
        //                        Quota = 0
        //                    };
        //                    //Check duplicate
        //                    var existed = _employeeRepository.FirstOrDefault(x => x.Name.Equals(employee.Name)
        //                                                                          || x.CardId.Equals(employee.CardId));
        //                    if (existed != null)
        //                    {
        //                        continue;
        //                    }
        //                    //get Quota
        //                    if (double.TryParse(items[2], out double quota))
        //                    {
        //                        employee.Quota = quota;
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }
        //                    //get Period
        //                    if (Enum.TryParse(items[3], out Period p))
        //                    {
        //                        employee.Period = items[3];
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }

        //                    _employeeRepository.Insert(employee);
        //                    var evt = employee.MapTo<EmployeeUpdatedIntegrationEvent>();
        //                    _eventBus.Publish(evt);
        //                }
        //            }
        //        }
        //        await CurrentUnitOfWork.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("Import Employee Error", ex);
        //    }
        //}

        //public async Task IncreaseOrdered(EmployeeDto input)
        //{
        //    try
        //    {
        //        var employee = _employeeRepository.Get(input.Id);
        //        if (employee != null)
        //        {
        //            employee.Ordered = input.Ordered;
        //            await _employeeRepository.UpdateAsync(employee);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("Increase Ordered Error", ex);
        //    }
        //}


        //public async Task<dynamic> UserInfo(List<UserInfoDto> users)
        //{
        //    return new
        //    {
        //        status = "1200",
        //        message = "正常",
        //        data = new List<string>()
        //    };
        //}

        //public async Task<dynamic> DealDataSyncHand(List<DealDataSyncHandDto> data)
        //{
        //    return new
        //    {
        //        status = 1200,
        //        message = "正常",
        //        data = new List<object>
        //        {
        //            new
        //            {
        //                idCard = "136",
        //                time = "2018/09/21/08:24",
        //                itemPrice = 66,
        //                num = 22,
        //                machineId = "12652301"
        //            }
        //        }
        //    };
        //}
    }
}