using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Domain.Entities
{
    public class ProviderRates
    {
        public int Id { get; set; }
        public string ProviderId { get; set; }
        public double Rate { get; set; }
        public string Comment { get; set; }
        public ServiceProvider? Provider { get; set; }
    }
}
