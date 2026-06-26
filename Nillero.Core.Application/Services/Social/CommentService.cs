using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Services.Base;
using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Social;
using Microsoft.EntityFrameworkCore;

namespace Nillero.Core.Application.Services.Social
{
    public class CommentService : GenericService<Comment, CommentDto>, ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentService(
            ICommentRepository commentRepository,
            IMapper mapper) : base(commentRepository, mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        public override async Task<CommentDto?> AddAsync(CommentDto dto)
        {
            try
            {
                if (dto.ParentCommentId.HasValue)
                {
                    var parent = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value);
                    if (parent != null)
                    {
                        dto.RootCommentId = parent.RootCommentId ?? parent.Id;
                        dto.ParentCommentId = dto.RootCommentId;
                    }
                }

                var entity = _mapper.Map<Comment>(dto);
                var result = await _commentRepository.AddAsync(entity);
                return _mapper.Map<CommentDto>(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddAsync (Comment): {ex.Message}");
                return null;
            }
        }

        public async Task<List<CommentDto>> GetCommentsByPostIdAsync(int postId)
        {
            try
            {
                var query = _commentRepository.GetAllQueryWithInclude(new List<string>
                {
                    "Replies"
                });

                // only root comments (without parent), with their direct replies
                var comments = await query
                    .Where(c => c.PostId == postId && c.ParentCommentId == null)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<CommentDto>>(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCommentsByPostIdAsync: {ex.Message}");
                throw;
            }
        }

    }
}
