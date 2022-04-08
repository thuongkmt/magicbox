using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Employees.Dtos;

namespace KonbiCloud.Employees
{
    public interface IEmployeeAppService : IApplicationService
    {
        //Task<ListResultDto<EmployeeDto>> GetEmployeeAndCountOrdered(int maxResultCount, int skipCount, string empId, string from, string to);
        Task<PagedResultDto<EmployeeDto>> GetAllEmployee(EmployeeRequest request);
        //Task Delete(EntityDto<Guid> input);
        //Task<Employee> Update(EmployeeDto input);
        //Task Create(EmployeeDto input);
        //Task IncreaseOrdered(EmployeeDto employee);
    }
}
