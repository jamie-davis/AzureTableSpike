using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace asSpike.Entities
{
    class DayStats : TableEntity
    {
        public DateTime Date { get; set; }
        public int MovementCalories { get; set; }
        public int Steps { get; set; }
        public decimal DistanceKm { get; set; }
    }
}
