using FIXIT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Application
{
    public class ServiceManager(IUnitOfWork unitOfWork) : IServiceManager
    {
        //=======>>>> Example of dependency injection for future services  <<<<=========

        // private readonly IUnitOfWork _unitOfWork;

        //public IUnitOfWork UnitOfWork => unitOfWork;
    }
}
