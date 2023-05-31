using Billing.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.RegularExpressions;
using System.Text;

namespace Billing.DataBase
{
    public static class Coins
    {
        private static List<CoinModel> _coins;
        
        //CREATE TABLE coins
        //(
        //    id INT PRIMARY KEY,
        //    user STRING FOREIGN KEY,
        //    history string
        //);

        public static void Init()
        {
            _coins = new List<CoinModel>();
        }

        public static async Task<CoinModel?> GetCoinById(long id)
        {
            return _coins.Find(x => x.Id == id);
            
            //SELECT id, user, history FROM coins
            //WHERE id = @id
        }

        public static async Task<List<CoinModel>> GetUserCoins(string userName)
        {
            return _coins.FindAll(x => x.User == userName);

            //SELECT id, user, history FROM coins
            //WHERE user = @user
        }

        public static async Task<long> GetUserCoinsAmount(string userName)
        {
            return _coins.Count(x => x.User == userName);

            //SELECT COUNT(id) FROM coins
            //WHERE user = @user
        }

        public static async Task<CoinModel?> GetCoinWithLongestHistory()
        {
            //return _coins.MaxBy(async x => await CoinHistoryLength(x.History.ToString()));
            CoinModel? responseCoin = null;
            foreach(var coin in _coins)
            {
                if (responseCoin == null)
                    responseCoin = coin;
                else
                {
                    if(await CoinHistoryLength(responseCoin.History.ToString()) < await CoinHistoryLength(coin.History.ToString()))
                    {
                        responseCoin = coin;
                    }
                }
            }
            return responseCoin;
            //SELECT id, user, history FROM coins
            //WHERE LEN(history) - LEN(REPLACE(history, ';', '')) =
            //( SELECT MAX(LEN(history) - LEN(REPLACE(history, ';', ''))) FROM coins )
        }

        public static async Task AddCoinsToUser(string userName, long amount)
        {
            List<CoinModel> coins = CoinModel.CoinGenerator.GenerateCoin(userName, amount);
            foreach(var coin in coins)
            {
                _coins.Add(coin);
            }
            //INSERT INTO coins (id, user, history)
            //VALUES (@id, @userName, @history)
            
        }

        public static async Task<bool> MoveCoins(string srcUser, string dstUser, long amount)
        {
            

            if ((int)amount < 0)
                throw new OverflowException();

            List<CoinModel> coinsToMove = _coins.FindAll(x => x.User == srcUser).GetRange(0, (int)amount);

            foreach(var coin in coinsToMove)
            {
                coin.User = dstUser;
                coin.AddHistory(dstUser);
            }

            return true; 

            //Данный SQL запрос должен быть одной транзакцией

            //UPDATE coins
            //SET user = @dstUser, history = history + @dstUser + '; '
            //WHERE id IN (
            //    SELECT id FROM (
            //        SELECT id FROM coins 
            //        WHERE user = @srcUser  
            //        LIMIT @amount
            //    ) tmp
            //)

            //Значение false возвращается в случае rollbackа бд
        }

        private static async Task<int> CoinHistoryLength(string history)
        {
            return Regex.Matches(history.ToString(), "; ").Count;
        }
    }
}
