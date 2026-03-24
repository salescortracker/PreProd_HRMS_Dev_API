using BusinessLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IExpenseStatusService
    {
        Task<IEnumerable<ExpenseStatusDto>> GetExpenseStatus(int userId);

        Task<ExpenseStatusDto> CreateExpenseStatus(ExpenseStatusDto dto);

        Task<ExpenseStatusDto> UpdateExpenseStatus(ExpenseStatusDto dto);

        Task<bool> DeleteExpenseStatus(int id);
    }
}
