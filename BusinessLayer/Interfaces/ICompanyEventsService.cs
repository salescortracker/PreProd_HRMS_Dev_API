using BusinessLayer.DTOs;
using DataAccessLayer.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface ICompanyEventsService
    {
        Task<IEnumerable<CompanyEventsDto>> GetAllEvents(int userId);

        Task<IEnumerable<CompanyEventsDto>> GetDepartmentEvents(int departmentId);

        Task<int> CreateEvent(CompanyEvent model);

        Task<int> UpdateEvent(CompanyEvent model);

        Task<int> DeleteEvent(int id);
    }
}
