using BusinessLayer.DTOs;
using BusinessLayer.Implementations;
using BusinessLayer.Implementations.HRMS.SaaS.API.Core.DTOs;
using DataAccessLayer.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace HRMS_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly PlanService planService;
        private readonly HRMSContext context;

        public PlansController(PlanService planService, HRMSContext dbContext)
        {
            planService = planService;
            context = dbContext;
        }

        [HttpGet("GetPlans")]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await context.Plans
                .Select(p => new PlanDto
                {
                    PlanId = p.PlanId,
                    PlanName = p.PlanName,
                    MaxUsers = p.MaxUsers,
                    DurationInDays = p.DurationInDays,
                    Price = p.Price
                })
                .ToListAsync();

            return Ok(plans);
        }

        // Create Plan
        [HttpPost("CreatePlan")]
        public async Task<IActionResult> CreatePlan([FromBody] PlanDto dto)
        {
            // Duplicate check
            var exists = await context.Plans
                .AnyAsync(p => p.PlanName.ToLower() == dto.PlanName.ToLower());

            if (exists)
            {
                return BadRequest("Plan already exists");
            }

            var plan = new Plan
            {
                PlanName = dto.PlanName,
                MaxUsers = dto.MaxUsers,
                Price = dto.Price,
                DurationInDays = dto.DurationInDays,
                CreatedAt = DateTime.Now
            };

            context.Plans.Add(plan);
            await context.SaveChangesAsync();

            return Ok(plan);
        }
        [HttpPut("UpdatePlan")]
        public async Task<IActionResult> UpdatePlan([FromBody] PlanDto dto)
        {
            var plan = await context.Plans.FindAsync(dto.PlanId);

            if (plan == null)
            {
                return NotFound("Plan not found");
            }

            // Duplicate name check (optional)
            var exists = await context.Plans
                .AnyAsync(p => p.PlanName.ToLower() == dto.PlanName.ToLower() && p.PlanId != dto.PlanId);

            if (exists)
            {
                return BadRequest("Plan name already exists");
            }

            plan.PlanName = dto.PlanName;
            plan.MaxUsers = dto.MaxUsers;
            plan.Price = dto.Price;
            plan.DurationInDays = dto.DurationInDays;
            plan.ModifiedAt = DateTime.Now;

            await context.SaveChangesAsync();

            return Ok(plan);
        }

        [HttpDelete("DeletePlan/{id}")]
        public async Task<IActionResult> DeletePlans(int id)
        {
            var plan = await context.Plans.FindAsync(id);

            if (plan == null)
            {
                return NotFound("Plan not found");
            }

            context.Plans.Remove(plan);
            await context.SaveChangesAsync();

            return Ok("Plan Deleted Successfully");
        }


        //[HttpGet("GetPlanModules/{planId}")]
        //public async Task<IActionResult> GetPlanModules(int planId)
        //{
        //    var modules = await context.Modules.ToListAsync();

        //    var planModules = await context.PlanModules
        //                        .Where(x => x.PlanId == planId)
        //                        .Select(x => x.AppModuleId)
        //                        .ToListAsync();

        //    var result = modules.Select(m => new
        //    {
        //        moduleId = m.ModuleId,
        //        moduleName = m.ModuleName,
        //        selected = planModules.Contains(m.ModuleId)
        //    });

        //    return Ok(result);
        //}

        [HttpGet("GetPlanMenus/{planId}")]
        public async Task<IActionResult> GetPlanMenus(int planId)
        {
            var menus = await context.MenuMasters
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.OrderNo)
                .ToListAsync();

            var planMenus = await context.PlanModules
                .Where(x => x.PlanId == planId)
                .Select(x => x.AppModuleId)   // FIXED
                .ToListAsync();

            var result = menus.Select(m => new
            {
                moduleId = m.MenuId,
                moduleName = m.MenuName,
                parentMenuId = m.ParentMenuId,
                selected = planMenus.Contains(m.MenuId)
            });

            return Ok(result);
        }
        //[HttpPost("AssignModules")]
        //public async Task<IActionResult> AssignModules([FromBody] PlanModuleRequestDto request)
        //{
        //    var existing = context.PlanModules
        //                    .Where(x => x.PlanId == request.PlanId);

        //    context.PlanModules.RemoveRange(existing);

        //    foreach (var moduleId in request.ModuleIds)
        //    {
        //        context.PlanModules.Add(new PlanModule
        //        {
        //            PlanId = request.PlanId,
        //            AppModuleId = moduleId,
        //            CreatedAt = DateTime.Now
        //        });
        //    }

        //    await context.SaveChangesAsync();

        //    //return Ok("Modules Assigned Successfully");
        //    return Ok(new { message = "Modules Assigned Successfully" });
        //}


        [HttpPost("AssignModules")]
        public async Task<IActionResult> AssignModules([FromBody] PlanModuleRequestDto request)
        {
            if (request.PlanId <= 0)
                return BadRequest("Invalid PlanId");

            if (request.ModuleIds == null || !request.ModuleIds.Any())
                return BadRequest("Please select at least one module");

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var existingModules = await context.PlanModules
                                        .Where(x => x.PlanId == request.PlanId)
                                        .ToListAsync();

                if (existingModules.Any())
                {
                    context.PlanModules.RemoveRange(existingModules);
                }

                var modulesToInsert = request.ModuleIds
                    .Distinct()
                    .Select(moduleId => new PlanModule
                    {
                        PlanId = request.PlanId,
                        AppModuleId = moduleId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1 // Replace with logged-in SuperAdmin
                    });

                await context.PlanModules.AddRangeAsync(modulesToInsert);

                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Modules assigned successfully"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error assigning modules",
                    error = ex.Message
                });
            }
        }
    }
}
