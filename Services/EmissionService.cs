using Billing.DataBase;
using Billing.Models;

namespace Billing.Services
{
    public class EmissionService
    {
        private class UserEmissionData
        {
            public long roundCoins;
            public double fraction;
            public double lowBoundCoef;

            public UserEmissionData(double rawCoins)
            {
                lowBoundCoef = Math.Floor(rawCoins);
                fraction = rawCoins - lowBoundCoef;
                roundCoins = 1;
            }

        }

        private List<UserProfileModel> _users;
        private Dictionary<string, UserEmissionData> _userEmissions;
        private long _coinsAmount;

        public EmissionService(List<UserProfileModel> users, long amount)
        {
            _users = users;
            _coinsAmount = amount;
            _userEmissions = new Dictionary<string, UserEmissionData>();
        }

        public async Task<bool> MakeTransaction()
        {
            long sumRating = 0;
            foreach (UserProfileModel user in _users)
            {
                sumRating += user.Rating;
                
            }

            //выдадим по монетке каждому пользователю
            _coinsAmount -= _users.Count;

            foreach (UserProfileModel user in _users)
            {
                //понизим коэффициенты
                var rawCoins = (((double)user.Rating / (double)sumRating) * (double)_coinsAmount) - 1d;
                _userEmissions.Add(user.Name, new UserEmissionData(rawCoins));
            }
            
            //теперь выдадим монеты округлённые вниз по значению

            var userEmissionsList = _userEmissions.ToList();
            userEmissionsList.Sort(CompareUserEmissionsByCoef);
             foreach(var  userEmission in userEmissionsList)
            {
                if (userEmission.Value.lowBoundCoef > 0)
                {
                    userEmission.Value.roundCoins += (long)userEmission.Value.lowBoundCoef;
                    _coinsAmount -= (long)userEmission.Value.lowBoundCoef;
                }
                else
                    break;
            }

            if (_coinsAmount < 0)
                return false;
            if(_coinsAmount == 0)
            {
                try
                {
                    await StartTransaction(userEmissionsList);
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }


            userEmissionsList.Sort(CompareUserEmissionsByFraction);

            int index = 0;
            while(_coinsAmount > 0)
            {
                if (userEmissionsList[index].Value.lowBoundCoef > 0)
                {
                    userEmissionsList[index].Value.roundCoins++;
                    _coinsAmount--;
                    index++;
                }
                else
                    index = 0;
            }

            try
            {
                await StartTransaction(userEmissionsList);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;

        }

        private static async Task StartTransaction(List<KeyValuePair<string, UserEmissionData>> userEmissionsList)
        {
            foreach(var userEmission in userEmissionsList)
            {
                await Coins.AddCoinsToUser(userEmission.Key, userEmission.Value.roundCoins);
            }
        }

        private static int CompareUserEmissionsByCoef(KeyValuePair<string, UserEmissionData> first, KeyValuePair<string, UserEmissionData> second)
        {
            if (first.Value.lowBoundCoef < second.Value.lowBoundCoef)
            {
                return 1;
            }
            else
            {
                if (first.Value.lowBoundCoef > second.Value.lowBoundCoef)
                { 
                    return -1;
                }
                else
                    return 0;
            }
        }

        private static int CompareUserEmissionsByFraction(KeyValuePair<string, UserEmissionData> first, KeyValuePair<string, UserEmissionData> second)
        {
            if (first.Value.fraction < second.Value.fraction && second.Value.lowBoundCoef > 0)
            {
                return 1;
            }
            else
            {
                if (first.Value.fraction > second.Value.fraction && first.Value.lowBoundCoef > 0)
                {
                    return -1;
                }
                else
                    return 0;
            }
        }
    }
}
