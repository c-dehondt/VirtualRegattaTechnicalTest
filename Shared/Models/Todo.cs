using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
   public class Todo
    {
        [Immutable]
        [GenerateSerializer]
        public record class TodoItem(
            Guid Key,
            string Title,
            bool IsDone,
            Guid OwnerKey,
            DateTime? Timestamp = null
        );
    }
}
