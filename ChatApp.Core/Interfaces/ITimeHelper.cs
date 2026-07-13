using System;

namespace ChatApp.Core.Interfaces
{
    public interface ITimeHelper
    {
        DateTime GetIstTime();
        DateTime ConvertToIst(DateTime utcTime);
    }
}
