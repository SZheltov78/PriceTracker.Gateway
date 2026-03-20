using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.Contracts
{
    public static class QueueNames
    {        
        public const string CreateTasks = "create.tasks";

        public const string WbAddTasks = "wb.tasks";       
        
        public const string OzonHistoryRequest = "ozon.history.requests";
        public const string WbHistoryRequest = "wb.history.requests";

        public const string OzonHistoryResponse = "ozon.history.response";
        public const string WbHistoryResponse = "wb.history.response";

        public const string NotificationTasks = "notification.tasks";
    }
}
