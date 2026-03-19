namespace Auth.Domain.Enums;

public enum AuthErrorCode
{
    Unknown = 0,
    InvalidRequest = 1000,
    InvalidEmail = 1001,
    InvalidPassword = 1002,
    InvalidFullName = 1003,
    UserAlreadyExists = 2001,
    InvalidCredentials = 2002,
    PersistenceFailure = 3001
}
