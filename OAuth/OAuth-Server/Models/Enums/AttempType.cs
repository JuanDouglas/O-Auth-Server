namespace OAuth.Server.Models.Enums
{
    public enum AttempType : uint
    {
        FirstStepAttemp,
        UserNotFound,
        IncorrectPassword,
        SecondStepAttemp,
        IPNotEqual
    }
}