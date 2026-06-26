using AutoMapper;
using Nillero.Core.Application.Interfaces.Base;
using Nillero.Core.Domain.Interfaces.Base;

namespace Nillero.Core.Application.Services.Base
{
    public class GenericService<Entity, DtoModel> : IGenericService<DtoModel>
        where Entity : class
        where DtoModel : class
    {
        private readonly IGenericRepository<Entity> _repository;
        private readonly IMapper _mapper;
        public GenericService(IGenericRepository<Entity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<DtoModel?> AddAsync(DtoModel dto)
        {
            try
            {
                Entity entity = _mapper.Map<Entity>(dto);
                Entity? returnEntity = await _repository.AddAsync(entity);

                if (returnEntity == null)
                    return null;

                return _mapper.Map<DtoModel>(returnEntity);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public virtual async Task<DtoModel?> UpdateAsync(DtoModel dto, int id)
        {
            try
            {
                Entity entity = _mapper.Map<Entity>(dto);
                Entity? returnEntity = await _repository.UpdateAsync(id, entity);

                if (returnEntity == null)
                    return null;

                return _mapper.Map<DtoModel>(returnEntity);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public virtual async Task<DtoModel?> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return null;

                DtoModel dto = _mapper.Map<DtoModel>(entity);

                return dto;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public virtual async Task<List<DtoModel>> GetAll()
        {
            try
            {
                var listEntities = await _repository.GetAllListAsync() ?? new List<Entity>();
                var listEntitiesDtos = _mapper.Map<List<DtoModel>>(listEntities);

                return listEntitiesDtos;
            }
            catch (Exception ex)
            {
                return [];
            }
        }
    }
}
