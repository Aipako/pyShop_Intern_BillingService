using Billing.Models;

namespace Billing.DataBase
{
    public static class Users
    {
        private static List<UserProfileModel> _users;

        //CREATE TABLE users
        //(
        //    name string PRIMARY KEY,
        //    rating INT
        //);

        public static void Init(List<UserProfileModel> users)
        {
            _users = new List<UserProfileModel>(users);
        }

        public static async Task<List<UserProfileModel>> GetUsersList()
        {
            return _users;
            //SELECT * FROM users
        }

        public static async Task<UserProfileModel?> GetUserByName(string userName)
        {
            return _users.Find(x => x.Name == userName);
            //SELECT * FROM users
            //WHERE name = @userName
        }

    }
}
