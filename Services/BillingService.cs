using Grpc.Core;
using Billing.Models;
using Billing.DataBase;

namespace Billing.Services
{
    public class BillingService : Billing.BillingBase
    {
        private readonly Response _OKresponse = new Response()
        {
            Status = Response.Types.Status.Ok,
            Comment = string.Empty
        };

        public override async Task ListUsers(None Request, IServerStreamWriter<UserProfile> userStream, ServerCallContext context)
        {
            List<UserProfileModel> users = await Users.GetUsersList();
            foreach(UserProfileModel user in users)
            {
                UserProfile profile = new UserProfile()
                {
                    Name = user.Name,
                    Amount = await Coins.GetUserCoinsAmount(user.Name)
                };
                await userStream.WriteAsync(profile);
            }
        }

        public override async Task<Response> CoinsEmission(EmissionAmount amount, ServerCallContext context)
        {
            List<UserProfileModel> users = await Users.GetUsersList();
            if(amount.Amount < users.LongCount())
            {
                return new Response()
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Amount of coins less than Users count"
                };
            }

            EmissionService newEmission = new EmissionService(users, amount.Amount);
            bool result = await newEmission.MakeTransaction();
            if(result)
            {
                return _OKresponse;
            }
            else
            {
                return new Response()
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Database transaction failed"
                };
            }   
        }

        public override async Task<Response> MoveCoins(MoveCoinsTransaction coinsTransaction, ServerCallContext context)
        {
            if (await Users.GetUserByName(coinsTransaction.SrcUser) == null)
            {
                return new Response()
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Source user does not exist"
                };
            }

            if (await Users.GetUserByName(coinsTransaction.DstUser) == null)
            {
                return new Response()
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Destination user does not exist"
                };
            }

            if (await Coins.GetUserCoinsAmount(coinsTransaction.SrcUser) < coinsTransaction.Amount)
            {
                return new Response()
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Source user don't have enough coins"
                };
            }

            try
            {
                if (await Coins.MoveCoins(coinsTransaction.SrcUser, coinsTransaction.DstUser, coinsTransaction.Amount))
                {
                    return _OKresponse;
                }
                else
                {
                    return new Response()
                    {
                        Status = Response.Types.Status.Failed,
                        Comment = "Database transaction failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Status = Response.Types.Status.Unspecified,
                    Comment = "Unknown error while database transaction"
                };
            }

        }

        public override async Task<Coin> LongestHistoryCoin(None none,ServerCallContext context)
        {
            CoinModel? coin = await Coins.GetCoinWithLongestHistory();
            if(coin == null) 
            {
                throw new NotImplementedException();

                //return new Coin { Id = -1, History = "" };
            }

            Coin responseCoin = new Coin
            {
                Id = coin.Id,
                History = coin.History.ToString()
            };

            return responseCoin;
        }
    }
}
