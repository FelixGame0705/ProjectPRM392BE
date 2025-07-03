using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.BLL.Services
{
    public interface IChatClient
    {
        Task RecieveMessage(string message);
    }
}
