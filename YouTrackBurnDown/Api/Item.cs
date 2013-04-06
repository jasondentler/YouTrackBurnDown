using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YouTrackBurnDown.Api
{
    public class Item
    {

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

    }
}