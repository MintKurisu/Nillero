using Nillero.Core.Domain.Interfaces.Base;
using Nillero.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Nillero.Infrastructure.Persistence.Repositories.Base
{
    public class GenericRepository<Entity> : IGenericRepository<Entity>
    where Entity : class
    {
        protected readonly NilleroAppContext _context;

        public GenericRepository(NilleroAppContext context)
        {
            _context = context;
        }

        public virtual async Task<Entity?> AddAsync(Entity entity)
        {
            try
            {
                await _context.Set<Entity>().AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding entity {typeof(Entity).Name}.", ex);
            }
        }

        public virtual async Task<Entity?> UpdateAsync(int id, Entity entity)
        {
            try
            {
                var entry = await _context.Set<Entity>().FindAsync(id);

                if (entry != null)
                {
                    _context.Entry(entry).CurrentValues.SetValues(entity);
                    await _context.SaveChangesAsync();
                    return entry;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating entity {typeof(Entity).Name} with Id {id}.", ex);
            }
        }

        public virtual async Task DeleteAsync(int id)
        {
            try
            {
                var entry = await _context.Set<Entity>().FindAsync(id);

                if (entry != null)
                {
                    _context.Set<Entity>().Remove(entry);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting entity {typeof(Entity).Name} with Id {id}.", ex);
            }
        }

        public virtual async Task<List<Entity>> GetAllListAsync()
        {
            try
            {
                return await _context.Set<Entity>().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entity list for {typeof(Entity).Name}.", ex);
            }
        }

        public virtual async Task<List<Entity>> GetAllListWithIncludeAsync(List<string> properties)
        {
            try
            {
                var query = _context.Set<Entity>().AsQueryable();

                foreach (var property in properties)
                {
                    query = query.Include(property);
                }

                return await query.ToListAsync(); // EF - Immediate Execution
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving included entities list for {typeof(Entity).Name}.", ex);
            }
        }

        public virtual async Task<Entity?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Entity>().FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entity {typeof(Entity).Name} with Id {id}.", ex);
            }
        }

        public virtual IQueryable<Entity> GetAllQuery()
        {
            try
            {
                return _context.Set<Entity>().AsQueryable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating query for entity {typeof(Entity).Name}.", ex);
            }
        }

        public virtual IQueryable<Entity> GetAllQueryWithInclude(List<string> properties)
        {
            try
            {
                var query = _context.Set<Entity>().AsQueryable();

                foreach (var property in properties)
                {
                    query = query.Include(property);
                }

                return query; // EF - Deferred Execution
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating query with includes for {typeof(Entity).Name}.", ex);
            }
        }
    }
}