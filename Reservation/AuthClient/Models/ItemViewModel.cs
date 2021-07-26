using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AuthClient.Models
{
    public class ItemViewModel
    {
        public JArray Items { get; private set; }
        public Dictionary<int, String> ItemTypes { get; private set; }


        public ItemViewModel(JArray items, Dictionary<int, String> itemTypes)
        {
            Items = items;
            ItemTypes = itemTypes;
            
            foreach (KeyValuePair<int, String> pair in ItemTypes)
            {

            }
        }
    }
}
