using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Contracts
{
    public interface ITokenService
    {
        Task<string?> GetMiscinfo();
        Task<string?> GetToken();
    }
}
