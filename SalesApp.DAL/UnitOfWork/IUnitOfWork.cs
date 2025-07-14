using SalesApp.DAL.Repositories;

namespace SalesApp.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;
        IProductRepository ProductRepository { get; }
        IUserRepository UserRepository { get; }
        IStoreLocationRepository StoreLocationRepository { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}