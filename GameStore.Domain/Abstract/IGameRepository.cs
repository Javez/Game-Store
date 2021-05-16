using GameStore.Domain.Entities;
using System.Collections.Generic;

namespace GameStore.Domain.Abstract
{
    public interface IGameRepository
    {
        IEnumerable<Game> Games { get; }
    }
}
