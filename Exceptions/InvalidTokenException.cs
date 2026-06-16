namespace StoreManagement.Exceptions;

public class InvalidTokenException : Exception
{
    public InvalidTokenException(string message) : base(message)
    {
    }

    public static InvalidTokenException Invalid()
    {
        return new InvalidTokenException("Token không hợp lệ");
    }

    public static InvalidTokenException Expired()
    {
        return new InvalidTokenException("Token đã hết hạn");
    }

    public static InvalidTokenException AlreadyUsed()
    {
        return new InvalidTokenException("Token đã được sử dụng");
    }
}
