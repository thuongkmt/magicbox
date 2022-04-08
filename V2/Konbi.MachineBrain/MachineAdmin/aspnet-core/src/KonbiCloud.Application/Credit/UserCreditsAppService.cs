using KonbiCloud.Authorization.Users;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Credit.Exporting;
using KonbiCloud.Credit.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.DeviceSettings;

namespace KonbiCloud.Credit
{
    [AbpAuthorize(AppPermissions.Pages_UserCredits)]
    public class UserCreditsAppService : KonbiCloudAppServiceBase, IUserCreditsAppService
    {
        private readonly IRepository<UserCredit, Guid> _userCreditRepository;
        private readonly IRepository<CreditHistory, Guid> _creditHistoryRepository;
        private readonly IUserCreditsExcelExporter _userCreditsExcelExporter;
        private readonly IRepository<User, long> _userRepository;
        private readonly IBillAcceptorHanlderService _billAcceptorHanlderService;

        public UserCreditsAppService(IRepository<UserCredit, Guid> userCreditRepository,
            IUserCreditsExcelExporter userCreditsExcelExporter, IRepository<User, long> userRepository,
            IBillAcceptorHanlderService billAcceptorHanlderService,
            IRepository<CreditHistory, Guid> creditHistoryRepository)
        {
            _userCreditRepository = userCreditRepository;
            _userCreditsExcelExporter = userCreditsExcelExporter;
            _userRepository = userRepository;
            _billAcceptorHanlderService = billAcceptorHanlderService;
            _creditHistoryRepository = creditHistoryRepository;
        }

        public async Task<PagedResultDto<GetUserCreditForViewDto>> GetAll(GetAllUserCreditsInput input)
        {

            var filteredUserCredits = _userCreditRepository.GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Hash.Contains(input.Filter))
                .WhereIf(input.MinValueFilter != null, e => e.Value >= input.MinValueFilter)
                .WhereIf(input.MaxValueFilter != null, e => e.Value <= input.MaxValueFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.HashFilter),
                    e => e.Hash.ToLower() == input.HashFilter.ToLower().Trim());


            var query = (from o in filteredUserCredits
                    join o1 in _userRepository.GetAll() on o.UserId equals o1.Id into j1
                    from s1 in j1.DefaultIfEmpty()

                    select new GetUserCreditForViewDto()
                    {
                        UserCredit = ObjectMapper.Map<UserCreditDto>(o),
                        UserName = s1 == null ? "" : s1.Name.ToString()
                    })
                .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter),
                    e => e.UserName.ToLower() == input.UserNameFilter.ToLower().Trim());

            var totalCount = await query.CountAsync();

            var userCredits = await query
                .OrderBy(input.Sorting ?? "userCredit.id asc")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<GetUserCreditForViewDto>(
                totalCount,
                userCredits
            );
        }


        public async Task<GetUserCreditForViewDto> GetUserCreditForView(Guid id)
        {
            var userCredit = await _userCreditRepository.GetAsync(id);

            var output = new GetUserCreditForViewDto {UserCredit = ObjectMapper.Map<UserCreditDto>(userCredit)};

            if (output.UserCredit.UserId != null)
            {
                var user = await _userRepository.FirstOrDefaultAsync((long) output.UserCredit.UserId);
                output.UserName = user.Name.ToString();
            }

            return output;
        }

        [AbpAllowAnonymous]
        public async Task<decimal> GetUserCredit(string userName)
        {
            //var credits = await _userCreditRepository.GetAll().Include(x => x.User).ToListAsync();
            userName = userName.ToUpper();
            var userCredit = await _userCreditRepository.GetAll().Where(x => x.User.UserName.ToUpper() == userName)
                .FirstOrDefaultAsync();
            return userCredit?.Value ?? 0;
        }

        [AbpAllowAnonymous]
        public async Task<object> GetUserCreditHistory(string userName)
        {
            userName = userName.ToUpper();
            var userCredits = await _creditHistoryRepository.GetAll()
                .Where(x => x.UserCredit.User.UserName.ToUpper() == userName && x.Value != 0)
                .OrderByDescending(x => x.CreationTime)
                .Take(20)
                .Select(x => new
                {
                    Value = x.Value,
                    Message = x.Message,
                    CreatedDate = x.CreationTime
                })
                .ToListAsync();

            return userCredits;
        }

        [AbpAllowAnonymous]
        public async Task<bool> EnableTopup()
        {
            _billAcceptorHanlderService.Enable();
            return true;
        }

        [AbpAllowAnonymous]
        public async Task<bool> AddTopup(string userName, int cents)
        {
            userName = userName.ToUpper();
            var dollar = Decimal.Round(cents / 100, 2);
            var userCredit = await _userCreditRepository.FirstOrDefaultAsync(x => x.User.UserName.ToUpper().Equals(userName));
            userCredit.Value += dollar;
            await _userCreditRepository.UpdateAsync(userCredit);

            var history = new CreditHistory
            {
                Value = dollar,
                Message = $"Topup ${dollar} from machine Konbini001",
                UserCredit = userCredit
            };
            await _creditHistoryRepository.InsertAsync(history);
            await CurrentUnitOfWork.SaveChangesAsync();
            

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_UserCredits_Edit)]
        public async Task<GetUserCreditForEditOutput> GetUserCreditForEdit(EntityDto<Guid> input)
        {
            var userCredit = await _userCreditRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetUserCreditForEditOutput
                {UserCredit = ObjectMapper.Map<CreateOrEditUserCreditDto>(userCredit)};

            if (output.UserCredit.UserId != null)
            {
                var user = await _userRepository.FirstOrDefaultAsync((long) output.UserCredit.UserId);
                output.UserName = user.Name.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditUserCreditDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_UserCredits_Create)]
        private async Task Create(CreateOrEditUserCreditDto input)
        {
            var userCredit = ObjectMapper.Map<UserCredit>(input);



            await _userCreditRepository.InsertAsync(userCredit);
        }

        [AbpAuthorize(AppPermissions.Pages_UserCredits_Edit)]
        private async Task Update(CreateOrEditUserCreditDto input)
        {
            var userCredit = await _userCreditRepository.FirstOrDefaultAsync((Guid) input.Id);
            ObjectMapper.Map(input, userCredit);
        }

        [AbpAuthorize(AppPermissions.Pages_UserCredits_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _userCreditRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetUserCreditsToExcel(GetAllUserCreditsForExcelInput input)
        {

            var filteredUserCredits = _userCreditRepository.GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Hash.Contains(input.Filter))
                .WhereIf(input.MinValueFilter != null, e => e.Value >= input.MinValueFilter)
                .WhereIf(input.MaxValueFilter != null, e => e.Value <= input.MaxValueFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.HashFilter),
                    e => e.Hash.ToLower() == input.HashFilter.ToLower().Trim());


            var query = (from o in filteredUserCredits
                    join o1 in _userRepository.GetAll() on o.UserId equals o1.Id into j1
                    from s1 in j1.DefaultIfEmpty()

                    select new GetUserCreditForViewDto()
                    {
                        UserCredit = ObjectMapper.Map<UserCreditDto>(o),
                        UserName = s1 == null ? "" : s1.Name.ToString()
                    })
                .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter),
                    e => e.UserName.ToLower() == input.UserNameFilter.ToLower().Trim());


            var userCreditListDtos = await query.ToListAsync();

            return _userCreditsExcelExporter.ExportToFile(userCreditListDtos);
        }



        [AbpAuthorize(AppPermissions.Pages_UserCredits)]
        public async Task<PagedResultDto<UserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _userRepository.GetAll().WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                e => e.Name.ToString().Contains(input.Filter)
            );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<UserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new UserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<UserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }
    }
}