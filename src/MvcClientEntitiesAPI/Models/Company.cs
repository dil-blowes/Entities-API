using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcClientEntitiesAPI.Models
{
    public class Company
    {
        public string QR { get; set; }
        public string Name { get; set; }

        public Company(string qr, string name)
        {
            QR = qr;
            Name = name;
        }
    }
}
