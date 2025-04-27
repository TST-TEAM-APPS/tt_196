using System.Collections.Generic;

namespace Flagsmith
{
    [System.Serializable]
    public class IdentityFlagsJsonBody
    {
        public string identifier;
        public List<Trait> traits;

        public IdentityFlagsJsonBody(string identifier, List<Trait> traits)
        {
            this.identifier = identifier;
            this.traits = traits;
        }
    }
}