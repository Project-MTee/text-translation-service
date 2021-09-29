using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Extensions.RabbitMQ
{
    public class RequestCorrelator
    {
        Dictionary<Guid, object> savedItems;

        public RequestCorrelator()
        {

        }

        public Task Request()
        {
            Guid correlationId;
            while (true) {
                correlationId = Guid.NewGuid();

                if(savedItems.TryAdd(correlationId, ""))
                {

                    break;
                }
            }
        }

        public void Update(Guid correlationId)
        {

        }
    }
}
