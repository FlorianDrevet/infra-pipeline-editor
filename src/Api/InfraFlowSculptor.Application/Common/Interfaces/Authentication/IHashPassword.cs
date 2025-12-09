namespace InfraFlowSculptor.Application.Common.Interfaces.Authentication;

public interface IHashPassword
{
    Tuple<string, string> GetHashedPassword(string password);
    string GetHashedPassword(string password, string salt);
}