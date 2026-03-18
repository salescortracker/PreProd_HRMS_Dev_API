using BusinessLayer.DTOs;
using DataAccessLayer.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Implementations
{
    public class SuperAdminService
    {
        private readonly HRMSContext context;

        public SuperAdminService(HRMSContext context)
        {
            context = context;
        }

        //public async Task CreateCompanyAsync(CreateCompanyDto dto)
        //{
        //    var plan = await context.Plans
        //        .Include(p => p.PlanModules)
        //        .FirstOrDefaultAsync(p => p.PlanId == dto.PlanId);

        //    if (plan == null)
        //        throw new Exception("Invalid Plan");

        //    var company = new Company
        //    {
        //        CompanyName = dto.CompanyName,
        //        PlanId = dto.PlanId,
        //        ExpiryDate = DateTime.Now.AddDays(plan.DurationInDays),
        //        IsActive = true
        //    };

        //    context.Companies.Add(company);
        //    await context.SaveChangesAsync();

        //    // Copy Plan Modules to CompanyModules
        //    foreach (var pm in plan.PlanModules)
        //    {
        //        context.CompanyModules.Add(new CompanyModule
        //        {
        //            CompanyId = company.CompanyId,
        //            ModuleId = pm.ModuleId,
        //            IsEnabled = true
        //        });
        //    }

        //    await context.SaveChangesAsync();
        //}

        public async Task CreateCompanyAsync(CreateCompanyDto dto)
        {
            var plan = await context.Plans
     .FirstOrDefaultAsync(p => p.PlanId == dto.PlanId);

            if (plan == null)
                throw new Exception("Invalid Plan");

            var company = new Company
            {
                CompanyName = dto.CompanyName,
                PlanId = dto.PlanId,
                PlanStartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(plan.DurationInMonths ?? 0)
            };

            context.Companies.Add(company);
            await context.SaveChangesAsync();

            foreach (var planModule in plan.PlanModules)
            {
                context.CompanyModules.Add(new CompanyModule
                {
                    CompanyId = company.PlanId,
                    AppModuleId = planModule.AppModuleId,
                    IsActive = true
                });
            }

            await context.SaveChangesAsync();
        }

        public async Task CreatePlanAsync(CreatePlanDto dto)
        {
            var plan = new Plan
            {
                PlanName = dto.PlanName,
                DurationInMonths = dto.DurationInMonths
            };

            context.Plans.Add(plan);
            await context.SaveChangesAsync();

            foreach (var moduleId in dto.ModuleIds)
            {
                context.PlanModules.Add(new PlanModule
                {
                    PlanId = plan.PlanId,
                    AppModuleId = moduleId
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
