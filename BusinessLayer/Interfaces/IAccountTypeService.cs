using BusinessLayer.Common;
using BusinessLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IAccountTypeService
    {
        Task<ApiResponse<IEnumerable<AccountTypeDto>>> GetAll(int userId);

        Task<ApiResponse<AccountTypeDto?>> GetByIdAsync(int id);

        Task<ApiResponse<string>> CreateAsync(AccountTypeDto dto);

        Task<ApiResponse<string>> UpdateAsync(AccountTypeDto dto);

        Task<ApiResponse<string>> DeleteAsync(int id);
    }
}
