namespace Nillero.Core.Domain.Interfaces.Base
{
    public interface IGenericRepository<Entity> where Entity : class
    {
        Task<Entity?> AddAsync(Entity entity);
        Task DeleteAsync(int id);
        Task<List<Entity>> GetAllListAsync();
        Task<List<Entity>> GetAllListWithIncludeAsync(List<string> properties);
        IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);
        IQueryable<Entity> GetAllQuery();
        Task<Entity?> GetByIdAsync(int id);
        Task<Entity?> UpdateAsync(int id, Entity entity);
    }
}
