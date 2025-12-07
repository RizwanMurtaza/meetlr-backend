using Meetlr.Domain.Exceptions.Base;

namespace Meetlr.Domain.Exceptions.Http;

public class BadRequestException : ApplicationExceptionBase
{
    public BadRequestException(ApplicationArea area, int code, string message)
        : base(HttpStatusCode.BadRequest, area, code, message)
    {
    }

    public new BadRequestException WithDetail(string key, object? value)
    {
        Details[key] = value;
        return this;
    }
}
