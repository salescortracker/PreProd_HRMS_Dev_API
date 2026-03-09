using BusinessLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IModeOfStudyService
    {
        Task<IEnumerable<ModeOfStudyDto>> GetAllModeOfStudtAsync(int userId);

        Task<ModeOfStudyDto?> GetByIdModeOfStudtAsync(int id);

        Task<bool> CreateModeOfStudtAsync(ModeOfStudyDto dto);

        Task<bool> UpdateModeOfStudtAsync(ModeOfStudyDto dto);

        Task<bool> DeleteModeOfStudtAsync(int id);


    }
}
