using System;
using System.Threading.Tasks;

namespace ChatApp.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Entities.User> Users { get; }
        IRepository<Entities.Chat> Chats { get; }
        IRepository<Entities.Message> Messages { get; }
        IRepository<Entities.ChatMember> ChatMembers { get; }
        Task<int> CompleteAsync();
    }
}
