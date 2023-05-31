using System.Text;
using System.Text.RegularExpressions;

namespace Billing.Models
{
    public class CoinModel
    {
        internal sealed class CoinGenerator
        {
            // А может лучше GUID?
            private static long _nextId = 1;

            public static CoinModel GenerateCoin(string userName)
            {
                CoinModel coin = new CoinModel(_nextId, userName);
                _nextId++;
                return coin;
            }

            public static List<CoinModel> GenerateCoin(string userName, long amount)
            {
                if (amount <= 0)
                    throw new ArgumentOutOfRangeException();

                List<CoinModel> result = new List<CoinModel>();
                for(int i = 0; i < amount; ++i)
                {
                    result.Add(GenerateCoin(userName));
                }
                return result;
            }

        }

        private CoinModel()
        {
            // Unused code
            throw new NotImplementedException();
        }

        private CoinModel(long id)
        {
            Id = id;
            History = new StringBuilder();
            History.Append("Created; ");
        }

        private CoinModel(long id, string userName)
        {
            Id = id;
            History = new StringBuilder();
            User = userName;
            History.Append("Created; " + User + "; ");
        }

        public long Id { get; init; } 

        public string? User { get; set; }
        
        public StringBuilder History { get; private set; }

        public void AddHistory(string newOwner)
        {
            History.Append(newOwner + "; ");
        }
    }
}
