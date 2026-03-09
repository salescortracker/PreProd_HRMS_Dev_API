using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Implementations
{
    public class CompanyEventsService: ICompanyEventsService
    {

        private readonly HRMSContext _context;

        public CompanyEventsService(HRMSContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompanyEventsDto>> GetAllEvents(int userId)
        {
            var data = from e in _context.CompanyEvents
                       join d in _context.Departments
                       on e.DepartmentId equals d.DepartmentId
                       where e.IsActive == true
                       && e.UserId==userId
                       select new CompanyEventsDto
                       {
                           Id = e.Id,
                           CompanyId = e.CompanyId,
                           RegionId = e.RegionId,
                           DepartmentId = e.DepartmentId,
                          // DepartmentName = d.DepartmentName,
                           EventTitle = e.EventTitle,
                           EventDescription = e.EventDescription,
                           EventDate = e.EventDate,
                           StartTime = e.StartTime,
                           EndTime = e.EndTime,
                           MeetingLink = e.MeetingLink,
                           EventLocation = e.EventLocation,
                           EventType = e.EventType,
                           IsMeeting = e.IsMeeting
                       };

            return await data.ToListAsync();
        }

        public async Task<IEnumerable<CompanyEventsDto>> GetDepartmentEvents( int departmentId)
        {
            var data = from e in _context.CompanyEvents
                       join d in _context.Departments
                       on e.DepartmentId equals d.DepartmentId
                       where e.DepartmentId == departmentId
                       && e.IsActive == true
                       select new CompanyEventsDto
                       {
                           Id = e.Id,
                          // DepartmentName = d.DepartmentName,
                           EventTitle = e.EventTitle,
                           EventDate = e.EventDate,
                           StartTime = e.StartTime,
                           EndTime = e.EndTime,
                           MeetingLink = e.MeetingLink,
                           EventLocation = e.EventLocation
                       };

            return await data.ToListAsync();
        }

        public async Task<int> CreateEvent(CompanyEvent model)
        {
            _context.CompanyEvents.Add(model);
            await _context.SaveChangesAsync();
            return model.Id;
        }

        public async Task<int> UpdateEvent(CompanyEvent model)
        {
            model.IsActive = true;
            _context.CompanyEvents.Update(model);
            await _context.SaveChangesAsync();
            return model.Id;
        }

        public async Task<int> DeleteEvent(int id)
        {
            var data = await _context.CompanyEvents.FindAsync(id);

            data.IsActive = false;

            await _context.SaveChangesAsync();

            return id;
        }
    }
}
