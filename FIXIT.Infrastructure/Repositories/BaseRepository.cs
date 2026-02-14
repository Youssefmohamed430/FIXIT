using FIXIT.Domain.IRepositries;
using FIXIT.Infrastructure.Data.Context;
using Mapster;
using System.Linq.Expressions;

namespace FIXIT.Infrastructure.Repositoriesك
{
    public class BaseRepository<T>(AppDbContext _context) : IBaseRepository<T> where T : class
    {
        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        public void DeleteAsync(T entity)
            => _context.Update(entity);

        public T Find(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            var entity = _context.Set<T>().AsNoTracking();

            if (includes != null)
                foreach (var include in includes ?? Array.Empty<string>())
                    entity = entity.Include(include);

            return entity.FirstOrDefault(criteria)!;
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            var entity = _context.Set<T>().AsNoTracking();

            if (includes != null)
                foreach (var include in includes ?? Array.Empty<string>())
                    entity = entity.Include(include);

            return await entity.FirstOrDefaultAsync(criteria);
        }

        public TDto Find<TDto>(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            var entity = _context.Set<T>()
                 .AsNoTracking();

            if (includes != null)
                foreach (var include in includes)
                    entity = entity.Include(include);

            return entity.Where(criteria).ProjectToType<TDto>().FirstOrDefault()!;
        }

        public IQueryable<T> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            var entity = _context.Set<T>()
                        .AsNoTracking();

            if (includes != null)
                foreach (var include in includes)
                    entity = entity.Include(include);

            return entity.Where(criteria)!;
        }

        public IQueryable<TDto> FindAll<TDto>(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            var entity = _context.Set<T>()
                    .AsNoTracking();

            if (includes != null)
                foreach (var include in includes)
                    entity = entity.Include(include);

            return entity.Where(criteria).ProjectToType<TDto>()!;
        }

        public T FindWithTracking(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            var entity = _context.Set<T>().AsTracking();

            if (includes != null)
                foreach (var include in includes ?? Array.Empty<string>())
                    entity = entity.Include(include);

            return entity.FirstOrDefault(criteria)!;
        }

        public IQueryable<TDto> GetAll<TDto>()
              => _context.Set<T>().AsNoTracking().ProjectToType<TDto>();

        public T GetById(int id)
           => _context.Set<T>().Find(id)!;

        public T GetByUserId(string id)
           => _context.Set<T>().Find(id)!;

        public Task<T> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            return Task.FromResult(entity);
        }
    }
}
