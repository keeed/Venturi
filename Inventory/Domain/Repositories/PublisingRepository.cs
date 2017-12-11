using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;

namespace Domain.Repositories
{
    public class PublisingRepository<T, TId> : IRepository<T, TId> where T : IEventSource
    {
        private readonly IRepository<T, TId> _baseRepository;
        private readonly IEventPublisher _eventPublisher;

        public PublisingRepository(IRepository<T, TId> baseRepository, IEventPublisher eventPublisher)
        {
            _baseRepository = baseRepository;
            _eventPublisher = eventPublisher;
        }

        public Task<T> GetByIdAsync(TId entityId, CancellationToken ct = default(CancellationToken))
        {
            return _baseRepository.GetByIdAsync(entityId, ct);
        }

        public async Task SaveAsync(T entity, CancellationToken ct = default(CancellationToken))
        {
            IEnumerable<IEvent> eventsCopy = entity.GetEvents();

            await _baseRepository.SaveAsync(entity, ct).ConfigureAwait(false);

            if(eventsCopy.Any())
            {
                await _eventPublisher.PublishAsync(eventsCopy, ct).ConfigureAwait(false);
            }

            entity.ClearEvents();
        }
    }
}