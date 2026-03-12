using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BusinessLayer.Implementations
{
    public class AssetApprovalService:IAssetApprovalService
    {
        private readonly HRMSContext _context;

        public AssetApprovalService(HRMSContext context)
        {
            _context = context;
        }

        // 🔹 Manager sees team pending assets
        public async Task<List<AssetApprovalDto>> GetPendingAssetsForManagerAsync(int managerUserId)
        {
            return await _context.Assets
                .Where(a => a.ReportingTo == managerUserId &&
                            a.ApprovalStatus == "Pending")
                .Select(a => new AssetApprovalDto
                {
                    AssetID = a.AssetId,
                    AssetName = a.AssetName,
                    AssetCode = a.AssetCode,
                    AssetLocation = a.AssetLocation,
                    AssetCost = a.AssetCost,
                    CurrencyCode = a.CurrencyCode,
                    ApprovalStatus = a.ApprovalStatus,
                    EmployeeName = a.EmployeeName
                })
                .OrderByDescending(a => a.AssetID)
                .ToListAsync();
        }

        // 🔹 Single API → Approve / Reject
        public async Task<bool> ApproveOrRejectAssetAsync(
            int assetId,
            int managerUserId,
            string action)
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a =>
                    a.AssetId == assetId &&
                    a.ReportingTo == managerUserId &&
                    a.ApprovalStatus == "Pending");

            if (asset == null)
                return false;

            if (action == "Approve")
                asset.ApprovalStatus = "Approved";
            else if (action == "Reject")
                asset.ApprovalStatus = "Rejected";
            else
                throw new Exception("Invalid action");

            asset.ApprovedBy = managerUserId;
            asset.ApprovedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ApproveRejectAssetsAsync(ApproveRejectAssetDto dto)
        {
            var assetsData = await (
                from a in _context.Assets
                join u in _context.Users on a.UserId equals u.UserId
                where dto.AssetIds.Contains(a.AssetId)
                select new
                {
                    Asset = a,
                    EmployeeName = u.FullName,
                    EmployeeEmail = u.Email
                }
            ).ToListAsync();

            if (!assetsData.Any())
                throw new Exception("No assets found");

            // -------------------------
            // UPDATE STATUS
            // -------------------------
            foreach (var item in assetsData)
            {
                item.Asset.ApprovalStatus = dto.Action;
                item.Asset.ApprovedBy = dto.ManagerId;
                item.Asset.ApprovedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // -------------------------
            // EMAIL TO EMPLOYEE
            // -------------------------
            foreach (var item in assetsData)
            {
                if (!string.IsNullOrWhiteSpace(item.EmployeeEmail))
                {
                    var body = BuildAssetEmail(
                        item.EmployeeName,
                        item.Asset.AssetName,
                        item.Asset.AssetCode,
                        item.Asset.AssetCost,
                        item.Asset.CurrencyCode,
                        dto.Action
                    );

                    //await _emailService.SendEmailAsync(
                    //    item.EmployeeEmail,
                    //    $"Asset {dto.Action}",
                    //    body
                    //);
                }
            }
        }

        private static string BuildAssetEmail(
    string employeeName,
    string assetName,
    string assetCode,
    decimal cost,
    string currency,
    string status)
        {
            var sb = new StringBuilder();

            sb.Append($"<p>Dear {employeeName},</p>");
            sb.Append("<p>Your asset request has been processed.</p>");

            sb.Append("<table border='1' cellpadding='6' cellspacing='0'>");
            sb.Append($"<tr><td><b>Asset</b></td><td>{assetName}</td></tr>");
            sb.Append($"<tr><td><b>Asset Code</b></td><td>{assetCode}</td></tr>");
            sb.Append($"<tr><td><b>Cost</b></td><td>{currency} {cost}</td></tr>");
            sb.Append($"<tr><td><b>Status</b></td><td><b>{status}</b></td></tr>");
            sb.Append("</table>");

            sb.Append("<p>Please login to HRMS for more details.</p>");
            sb.Append("<p>Regards,<br/>HRMS Team</p>");

            return sb.ToString();
        }
    }
}
