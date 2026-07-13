using System;
using System.Threading.Tasks;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;

namespace ChatApp.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ChatAppContext _context;

        public UnitOfWork(ChatAppContext context)
        {
            _context = context;
            Users = new Repository<User>(_context);
            Chats = new Repository<Chat>(_context);
            Messages = new Repository<Message>(_context);
            ChatMembers = new Repository<ChatMember>(_context);
        }

        public IRepository<User> Users { get; private set; }
        public IRepository<Chat> Chats { get; private set; }
        public IRepository<Message> Messages { get; private set; }
        public IRepository<ChatMember> ChatMembers { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
