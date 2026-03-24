using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class ApproveRejectAssetDto
    {
        public List<int> AssetIds { get; set; } = new();
        public int ManagerId { get; set; }
        public string Action { get; set; } = string.Empty; // Approved / Rejected
    }
}
