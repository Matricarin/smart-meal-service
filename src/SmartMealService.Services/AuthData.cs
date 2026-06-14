namespace SmartMealService.Services;

public sealed class AuthData
{
    public string UserName { get; }
    public string Password { get; }

    public AuthData(string? password, string? userName)
    {
        Password = password ?? throw new ArgumentNullException(nameof(password));
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
    }

    public override string ToString()
    {
        return $"{UserName}:{Password}";
    }

    private bool Equals(AuthData other)
    {
        return UserName == other.UserName && Password == other.Password;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is AuthData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UserName, Password);
    }
}