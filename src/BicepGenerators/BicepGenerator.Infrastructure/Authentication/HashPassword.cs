using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using BicepGenerator.Application.Common.Interfaces.Authentication;

namespace BicepGenerator.Infrastructure.Authentication;

public class HashPassword : IHashPassword
{
    public Tuple<string, string> GetHashedPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        return new Tuple<string, string>(Convert.ToBase64String(salt), hashed.ToString());
    }

    public string GetHashedPassword(string password, string salt)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: Convert.FromBase64String(salt),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
    }
}