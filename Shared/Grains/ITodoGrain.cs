using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.Models.Todo;

namespace Shared.Grains
{
   
        public interface ITodoGrain : IGrainWithGuidKey
        {
            Task SetAsync(TodoItem item);
            Task<TodoItem?> GetAsync();
        }
    
}
