using BusinessLayer.Implementations.HRMS.SaaS.API.Core.DTOs;
using DataAccessLayer.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Implementations
{
    public class PlanService
    {
        private readonly HRMSContext context;

        public PlanService(HRMSContext context)
        {
            context = context;
        }

        public async Task<List<PlanDto>> GetPlans()
        {
            return await context.Plans
                .Select(p => new PlanDto
                {
                    PlanId = p.PlanId,
                    PlanName = p.PlanName,
                    MaxUsers = p.MaxUsers,
                    Price = p.Price
                })
                .ToListAsync();
        }

        public async Task<Plan?> CreatePlan(Plan plan)
        {
            // Check if plan already exists
            var existingPlan = await context.Plans
    .FirstOrDefaultAsync(p => p.PlanName.ToLower() == plan.PlanName.ToLower());

            if (existingPlan != null)
            {
                return null; // Duplicate found
            }

            context.Plans.Add(plan);
            await context.SaveChangesAsync();

            return plan;
        }

        public async Task<Plan> UpdatePlan(Plan plan)
        {
            context.Plans.Update(plan);
            await context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> DeletePlan(int id)
        {
            var plan = await context.Plans.FindAsync(id);
            if (plan == null) return false;

            context.Plans.Remove(plan);
            await context.SaveChangesAsync();
            return true;
        }
    }
    namespace HRMS.SaaS.API.Core.DTOs
    {
        public class PlanDto
        {
            public int PlanId { get; set; }
            public string PlanName { get; set; }
            public int? MaxUsers { get; set; }
            public int? DurationInDays { get; set; }
            public decimal? Price { get; set; }
        }
    }
}
