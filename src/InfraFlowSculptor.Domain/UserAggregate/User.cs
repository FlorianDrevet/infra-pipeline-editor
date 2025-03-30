using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.UserAggregate;

public sealed class User : AggregateRoot<UserId>
{
    public string Email { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public Name Name { get; private set; } = null!;
    public string Salt { get; private set; } = null!;
    public string Role { get; set; } = null!;

    private User(UserId userId, string email, string password, Name name, string salt)
        : base(userId)
    {
        Email = email;
        Password = password;
        Name = name;
        Salt = salt;
        Role = "User";
    }

    public static User Create(string email, string password, Name name, string salt)
    {
        return new User(UserId.CreateUnique(), email, password, name, salt);
    }

    public User()
    {
    }

    public void ChangeEmail(string requestEmail)
    {
        Email = requestEmail;
    }
}