namespace Meetlr.Application.Interfaces;

public interface ITimeZoneService
{
    DateTime ConvertToTimeZone(DateTime dateTime, string fromTimeZone, string toTimeZone);
    DateTime ConvertToUtc(DateTime dateTime, string fromTimeZone);
    DateTime ConvertFromUtc(DateTime dateTime, string toTimeZone);
    string DetectTimeZoneFromIp(string ipAddress);
    List<string> GetAllTimeZones();
}
