namespace Nillero.Core.Application.Interfaces.Base
{
    public interface IGenericService<DtoModel>
        where DtoModel : class
    {
        Task<DtoModel?> AddAsync(DtoModel dto);
        Task<DtoModel?> UpdateAsync(DtoModel dto, int id);
        Task<bool> DeleteAsync(int id);
        Task<DtoModel?> GetById(int id);
        Task<List<DtoModel>> GetAll();

    }
}
