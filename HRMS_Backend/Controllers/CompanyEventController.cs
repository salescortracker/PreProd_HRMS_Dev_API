using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using Microsoft.AspNetCore.Mvc;

namespace HRMS_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyEventController:ControllerBase
    {
        private readonly ICompanyEventsService _service;
        private readonly HRMSContext _context;
        public CompanyEventController(ICompanyEventsService service, HRMSContext context)
        {
            _service = service;
            _context = context;
        }
        // Get All Events (Admin Screen)

        [HttpGet("GetAllEvents")]
        public async Task<IActionResult> GetAllEvents(int userId)
        {
            var result = await _service.GetAllEvents(userId);
            return Ok(result);
        }


        // Get Events by Company + Region + Department (User Screen)

        [HttpGet("GetDepartmentEvents")]
        public async Task<IActionResult> GetDepartmentEvents( int departmentId)
        {
            var result = await _service.GetDepartmentEvents( departmentId);
            return Ok(result);
        }


        // Create Event

        [HttpPost("CreateEvent")]
        public async Task<IActionResult> CreateEvent([FromBody] CompanyEvent model)
        {
            if (model == null)
            {
                return BadRequest("Invalid event data.");
            }

            var result = await _service.CreateEvent(model);

            return Ok(new
            {
                Message = "Event created successfully",
                EventId = result
            });
        }


        // Update Event

        [HttpPost("UpdateEvent")]
        public async Task<IActionResult> UpdateEvent([FromBody] CompanyEvent model)
        {
            if (model == null)
            {
                return BadRequest("Invalid event data.");
            }

            var result = await _service.UpdateEvent(model);

            return Ok(new
            {
                Message = "Event updated successfully",
                EventId = result
            });
        }


        // Delete Event (Soft Delete)

        [HttpPost("DeleteEvent")]
        public async Task<IActionResult> DeleteEvent([FromQuery] int id)
        {
            var result = await _service.DeleteEvent(id);

            return Ok(new
            {
                Message = "Event deleted successfully",
                EventId = result
            });
        }
        [HttpGet("GetMyCalendar")]
        public IActionResult GetMyCalendar(int userId)
        {
            var leaves = _context.LeaveRequests
                .Where(x => x.UserId == userId)
                .Select(x => new CalendarEventDto
                {
                    date =x.StartDate,
                    type = "leave",
                    title = "Leave Applied"
                });

            var tickets = _context.Tickets
                .Where(x => x.UserId == userId)
                .Select(x => new CalendarEventDto
                {
                    date = DateOnly.FromDateTime(x.CreatedAt),// x.CreatedAt,
                    type = "helpdesk",
                    title = x.Subject
                });

            var expenses = _context.Expenses
                .Where(x => x.UserId == userId)
                .Select(x => new CalendarEventDto
                {

                    date = x.ExpenseDate,
                    type = "expense",
                    title = x.ProjectName
                });

            var result = leaves
                .Union(tickets)
                .Union(expenses)
                .ToList();

            return Ok(result);
        }

    }
}
