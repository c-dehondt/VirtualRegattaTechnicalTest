using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Shared.Grains;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.Models.Todo;

namespace GameSilos.Grains
{
    public class TodoGrain : Grain, ITodoGrain
    {
        private readonly ILogger<TodoGrain> _logger;
        private readonly IPersistentState<State> _state;

        private static string GrainType => nameof(TodoGrain);
        private Guid GrainKey => this.GetPrimaryKey();

        public TodoGrain(ILogger<TodoGrain> logger, [PersistentState("State")] IPersistentState<State> state)
        {
            _logger = logger;
            _state = state;
        }

        public Task<TodoItem?> GetAsync() => Task.FromResult(_state.State.Item);

        public async Task SetAsync(TodoItem item)
        {
            _state.State.Item = item;
            await _state.WriteStateAsync();

            _logger.LogInformation(
                "{@GrainType} {@GrainKey} now contains {@Todo}",
                GrainType, GrainKey, item);

           
        }

        [GenerateSerializer]
        public class State
        {
            [Id(0)] 
            public TodoItem? Item { get; set; }
        }
    }
}
