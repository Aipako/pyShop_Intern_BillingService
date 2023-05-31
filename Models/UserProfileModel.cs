namespace Billing.Models
{
    public class UserProfileModel
    {
        
        public UserProfileModel(string name, long rating)
        {
            Name = name;
            Rating = rating;
        }

        public string Name { get; init; }

        public long Rating { get; private set; }
    }
}
