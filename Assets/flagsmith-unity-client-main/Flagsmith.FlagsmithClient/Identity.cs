using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flagsmith
{
    [System.Serializable]
    public class Identity : IIdentity
    {
        public Identity()
        {
        }
        public Identity(List<Flag> flags, List<Trait> traits)
        {
            this.flags = flags;
            this.traits = traits;
        }

        public Identity(List<Flag> flags)
        {
            this.flags = flags;
        }

        public Identity(List<Trait> traits)
        {
            this.traits = traits;
        }
        
        [JsonProperty]
        public List<Flag> flags;

        [JsonProperty]
        public List<Trait> traits;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
