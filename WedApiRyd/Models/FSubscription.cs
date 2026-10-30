using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WedApiRyd.Models
{
    public class FSubscription
    {
            public int? Id { get; set; } = null;
            public string? Name { get; set; } = null;
            public decimal? Price { get; set; } = null;
            public int? DurationDays { get; set; } = null;
            public string? Description { get; set; } = null;
            public bool? IsActive { get; set; } = null;
        }
    }