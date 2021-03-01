using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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