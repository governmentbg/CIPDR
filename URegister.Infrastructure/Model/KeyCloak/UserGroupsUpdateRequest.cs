namespace URegister.Infrastructure.Model.KeyCloak
{
    public class UserGroupsUpdateRequest
    {
        public string UserId { get; set; }
        public string GroupId { get; set; }
        public List<string> Groups { get; set; } = new List<string>();
    }
}
