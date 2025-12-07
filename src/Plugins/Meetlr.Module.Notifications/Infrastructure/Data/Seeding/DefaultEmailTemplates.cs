using Meetlr.Domain.Enums;

namespace Meetlr.Module.Notifications.Infrastructure.Data.Seeding;

/// <summary>
/// Contains default HTML email templates for all template types
/// </summary>
public static class DefaultEmailTemplates
{
    public static Dictionary<EmailTemplateType, (string Subject, string HtmlBody, string PlainText, string[] Variables)> GetDefaults()
    {
        return new Dictionary<EmailTemplateType, (string, string, string, string[])>
        {
            [EmailTemplateType.EmailVerification] = (
                "Verify Your Email - {tenantName}",
                GetEmailVerificationHtml(),
                "Hi {userName},\n\nThank you for registering with {tenantName}. Your verification code is: {otpCode}\n\nThis code will expire in {expiryMinutes} minutes.\n\nIf you didn't request this, please ignore this email.",
                new[] { "userName", "otpCode", "tenantName", "expiryMinutes" }
            ),

            [EmailTemplateType.WelcomeEmail] = (
                "Welcome to {tenantName}!",
                GetWelcomeEmailHtml(),
                "Welcome {userName}!\n\nYour account is now active. You can log in at: {loginUrl}\n\nThank you for joining {tenantName}!",
                new[] { "userName", "tenantName", "loginUrl" }
            ),

            [EmailTemplateType.BookingConfirmationHost] = (
                "New Booking: {eventName} with {inviteeName}",
                GetBookingConfirmationHostHtml(),
                "Hi {hostName},\n\nYou have a new booking:\n\nEvent: {eventName}\nWith: {inviteeName}\nWhen: {bookingTime}\nLocation: {location}\n\nMeeting Link: {meetingLink}",
                new[] { "hostName", "inviteeName", "eventName", "bookingTime", "location", "meetingLink", "inviteeEmail", "inviteePhone" }
            ),

            [EmailTemplateType.BookingConfirmationInvitee] = (
                "Booking Confirmed: {eventName} with {hostName}",
                GetBookingConfirmationInviteeHtml(),
                "Hi {inviteeName},\n\nYour booking is confirmed:\n\nEvent: {eventName}\nWith: {hostName}\nWhen: {bookingTime}\nLocation: {location}\n\nMeeting Link: {meetingLink}\n\nTo reschedule: {rescheduleUrl}\nTo cancel: {cancellationUrl}",
                new[] { "inviteeName", "hostName", "eventName", "bookingTime", "location", "meetingLink", "hostEmail", "rescheduleUrl", "cancellationUrl" }
            ),

            [EmailTemplateType.BookingCancellationHost] = (
                "Booking Cancelled: {eventName} with {inviteeName}",
                GetBookingCancellationHostHtml(),
                "Hi {hostName},\n\nA booking has been cancelled:\n\nEvent: {eventName}\nWith: {inviteeName}\nOriginal Time: {bookingTime}\nReason: {cancellationReason}",
                new[] { "hostName", "inviteeName", "eventName", "bookingTime", "cancellationReason" }
            ),

            [EmailTemplateType.BookingCancellationInvitee] = (
                "Booking Cancelled: {eventName} with {hostName}",
                GetBookingCancellationInviteeHtml(),
                "Hi {inviteeName},\n\nYour booking has been cancelled:\n\nEvent: {eventName}\nWith: {hostName}\nOriginal Time: {bookingTime}\nReason: {cancellationReason}",
                new[] { "inviteeName", "hostName", "eventName", "bookingTime", "cancellationReason" }
            ),

            [EmailTemplateType.BookingReminder] = (
                "Reminder: {eventName} in 24 hours",
                GetBookingReminderHtml(),
                "Hi {inviteeName},\n\nThis is a reminder about your upcoming appointment:\n\nEvent: {eventName}\nWith: {hostName}\nWhen: {bookingTime}\nLocation: {location}\n\nMeeting Link: {meetingLink}",
                new[] { "inviteeName", "hostName", "eventName", "bookingTime", "location", "meetingLink", "timeUntil" }
            ),

            [EmailTemplateType.PasswordReset] = (
                "Reset Your Password - {tenantName}",
                GetPasswordResetHtml(),
                "Hi {userName},\n\nYou requested a password reset. Your reset code is: {otpCode}\n\nThis code will expire in {expiryMinutes} minutes.\n\nIf you didn't request this, please ignore this email.",
                new[] { "userName", "otpCode", "tenantName", "expiryMinutes" }
            ),

            [EmailTemplateType.BookingRescheduled] = (
                "Booking Rescheduled: {eventName} with {hostName}",
                GetBookingRescheduledHtml(),
                "Hi {inviteeName},\n\nYour booking has been rescheduled:\n\nEvent: {eventName}\nWith: {hostName}\n\nOld Time: {oldBookingTime}\nNew Time: {bookingTime}\n\nLocation: {location}",
                new[] { "inviteeName", "hostName", "eventName", "oldBookingTime", "bookingTime", "location", "meetingLink", "hostEmail", "rescheduleUrl", "cancellationUrl" }
            ),

            [EmailTemplateType.SlotInvitation] = (
                "You're Invited: {eventName} with {hostName}",
                GetSlotInvitationHtml(),
                "Hi {inviteeName},\n\n{hostName} has invited you to book a meeting.\n\nEvent: {eventName}\nReserved Time: {slotTime}\n\nThis invitation expires in {expirationHours} hours.\n\nBook now: {bookingUrl}",
                new[] { "inviteeName", "hostName", "eventName", "slotTime", "duration", "expirationHours", "expiresAt", "bookingUrl", "hostEmail" }
            )
        };
    }

    private static string GetEmailVerificationHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
        <tr>
            <td align='center' style='padding: 40px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 20px 60px rgba(0,0,0,0.3);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 50px 20px;'>
                            <h1 style='color: white; margin: 0 0 10px; font-size: 32px; font-weight: 700;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.9); margin: 0; font-size: 18px;'>Verify Your Email</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 50px 40px;'>
                            <p style='margin: 0 0 20px; font-size: 16px; line-height: 1.6; color: #333;'>Hi <strong>{userName}</strong>,</p>
                            <p style='margin: 0 0 30px; font-size: 16px; line-height: 1.6; color: #333;'>Thank you for joining <strong>Meetlr</strong>! Please use the verification code below to confirm your email address:</p>
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin: 40px 0;'>
                                <tr>
                                    <td align='center' style='background: linear-gradient(135deg, #f0f0ff 0%, #f5f3ff 100%); padding: 35px; border-radius: 12px; border: 2px solid #e0e7ff;'>
                                        <div style='font-size: 42px; font-weight: 700; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; letter-spacing: 10px; font-family: monospace;'>{otpCode}</div>
                                    </td>
                                </tr>
                            </table>
                            <p style='margin: 0 0 10px; font-size: 14px; color: #666; text-align: center;'>This code will expire in <strong>{expiryMinutes} minutes</strong>.</p>
                            <p style='margin: 0; font-size: 14px; color: #666; text-align: center;'>If you didn't request this, please ignore this email.</p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' style='background: #f8f9fa; padding: 30px; border-top: 1px solid #e0e0e0;'>
                            <p style='margin: 0 0 10px; font-size: 14px; color: #666; font-weight: 600;'>Smart scheduling made simple</p>
                            <p style='margin: 0; font-size: 12px; color: #999;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetWelcomeEmailHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
        <tr>
            <td align='center' style='padding: 40px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 20px 60px rgba(0,0,0,0.3);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 50px 20px;'>
                            <h1 style='color: white; margin: 0 0 10px; font-size: 32px; font-weight: 700;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.9); margin: 0; font-size: 18px;'>Welcome!</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 50px 40px;'>
                            <p style='margin: 0 0 20px; font-size: 16px; line-height: 1.6; color: #333;'>Hi <strong>{userName}</strong>,</p>
                            <p style='margin: 0 0 20px; font-size: 16px; line-height: 1.6; color: #333;'>Welcome to <strong>Meetlr</strong>! Your account is now active and ready to use.</p>
                            <p style='margin: 0 0 30px; font-size: 16px; line-height: 1.6; color: #333;'>Start scheduling meetings with ease and take control of your time.</p>
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin: 30px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{loginUrl}' style='display: inline-block; padding: 16px 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 12px; font-weight: 600; font-size: 16px; box-shadow: 0 4px 12px rgba(102,126,234,0.4);'>Login to Your Account</a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' style='background: #f8f9fa; padding: 30px; border-top: 1px solid #e0e0e0;'>
                            <p style='margin: 0 0 10px; font-size: 14px; color: #666; font-weight: 600;'>Smart scheduling made simple</p>
                            <p style='margin: 0; font-size: 12px; color: #999;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetBookingConfirmationHostHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
        <tr>
            <td align='center' style='padding: 50px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#667eea' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 50px 30px;'>
                            <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                                <tr>
                                    <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                        <span style='color: #667eea; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>New Booking</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 55px 45px;'>
                            <p style='margin: 0 0 10px; font-size: 18px; color: #1f2937;'>Hi <strong style='color: #111827;'>{hostName}</strong>,</p>
                            <p style='margin: 0 0 40px; font-size: 16px; line-height: 1.5; color: #6b7280;'>You have a new booking!</p>

                            <!-- Details Card -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #f8f9ff 0%, #f0f4ff 100%); border-radius: 16px; border: 1px solid #e5e7ff; margin-bottom: 35px;'>
                                <tr>
                                    <td style='padding: 35px 30px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='padding: 0 0 22px 0;'>
                                                    <p style='margin: 0 0 6px; font-size: 13px; color: #6366f1; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Event</p>
                                                    <p style='margin: 0; font-size: 20px; color: #1e1b4b; font-weight: 700; line-height: 1.3;'>{eventName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(99, 102, 241, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #6366f1; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Attendee</p>
                                                    <p style='margin: 0; font-size: 17px; color: #312e81; font-weight: 600;'>{inviteeName}</p>
                                                    <p style='margin: 4px 0 0; font-size: 14px; color: #6b7280;'>{inviteeEmail}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(99, 102, 241, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #6366f1; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>When</p>
                                                    <p style='margin: 0; font-size: 17px; color: #312e81; font-weight: 600;'>{bookingTime}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(99, 102, 241, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #6366f1; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Location</p>
                                                    <p style='margin: 0; font-size: 17px; color: #312e81; font-weight: 600;'>{location}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- CTA Button -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                <tr>
                                    <td align='center' style='padding: 10px 0;'>
                                        <a href='{meetingLink}' style='display: inline-block; padding: 18px 50px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 14px; font-weight: 600; font-size: 17px; box-shadow: 0 6px 20px rgba(102, 126, 234, 0.35); letter-spacing: 0.3px;'>Join Meeting</a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetBookingConfirmationInviteeHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
        <tr>
            <td align='center' style='padding: 50px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#667eea' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 50px 30px;'>
                            <!-- Logo -->
                            <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                                <tr>
                                    <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                        <span style='color: #667eea; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>Booking Confirmed</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 55px 45px;'>
                            <p style='margin: 0 0 10px; font-size: 18px; color: #1f2937;'>Hi <strong style='color: #111827;'>{inviteeName}</strong>,</p>
                            <p style='margin: 0 0 40px; font-size: 16px; line-height: 1.5; color: #6b7280;'>Your booking is confirmed. We look forward to meeting with you.</p>

                            <!-- Details Card -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #f8f9ff 0%, #f0f4ff 100%); border-radius: 16px; border: 1px solid #e5e7ff; margin-bottom: 35px;'>
                                <tr>
                                    <td style='padding: 35px 30px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='padding: 0 0 22px 0;'>
                                                    <p style='margin: 0 0 6px; font-size: 13px; color: #6366f1; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Event</p>
                                                    <p style='margin: 0; font-size: 20px; color: #1e1b4b; font-weight: 700; line-height: 1.3;'>{eventName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(99, 102, 241, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #6366f1; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>With</p>
                                                    <p style='margin: 0; font-size: 17px; color: #312e81; font-weight: 600;'>{hostName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(99, 102, 241, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #6366f1; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>When</p>
                                                    <p style='margin: 0; font-size: 17px; color: #312e81; font-weight: 600;'>{bookingTime}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(99, 102, 241, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #6366f1; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Location</p>
                                                    <p style='margin: 0; font-size: 17px; color: #312e81; font-weight: 600;'>{location}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- CTA Buttons -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                <tr>
                                    <td align='center' style='padding: 10px 0;'>
                                        <a href='{meetingLink}' style='display: inline-block; padding: 18px 50px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 14px; font-weight: 600; font-size: 17px; box-shadow: 0 6px 20px rgba(102, 126, 234, 0.35); letter-spacing: 0.3px;'>Join Meeting</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Action Links -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin-top: 25px;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{rescheduleUrl}' style='display: inline-block; margin: 0 10px; padding: 12px 24px; background-color: #f3f4f6; color: #374151; text-decoration: none; border-radius: 8px; font-weight: 500; font-size: 14px; border: 1px solid #e5e7eb;'>Reschedule</a>
                                        <a href='{cancellationUrl}' style='display: inline-block; margin: 0 10px; padding: 12px 24px; background-color: #fef2f2; color: #dc2626; text-decoration: none; border-radius: 8px; font-weight: 500; font-size: 14px; border: 1px solid #fecaca;'>Cancel Booking</a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0 0 12px; font-size: 14px; color: #6b7280;'>Questions? Contact <a href='mailto:{hostEmail}' style='color: #667eea; text-decoration: none; font-weight: 600;'>{hostEmail}</a></p>
                            <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetBookingCancellationHostHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
        <tr>
            <td align='center' style='padding: 50px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#dc2626' style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 50px 30px;'>
                            <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                                <tr>
                                    <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                        <span style='color: #dc2626; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>Booking Cancelled</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 55px 45px;'>
                            <p style='margin: 0 0 10px; font-size: 18px; color: #1f2937;'>Hi <strong style='color: #111827;'>{hostName}</strong>,</p>
                            <p style='margin: 0 0 40px; font-size: 16px; line-height: 1.5; color: #6b7280;'>A booking has been cancelled.</p>

                            <!-- Details Card -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%); border-radius: 16px; border: 1px solid #fecaca; margin-bottom: 35px;'>
                                <tr>
                                    <td style='padding: 35px 30px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='padding: 0 0 22px 0;'>
                                                    <p style='margin: 0 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Event</p>
                                                    <p style='margin: 0; font-size: 20px; color: #7f1d1d; font-weight: 700; line-height: 1.3;'>{eventName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(220, 38, 38, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Attendee</p>
                                                    <p style='margin: 0; font-size: 17px; color: #991b1b; font-weight: 600;'>{inviteeName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(220, 38, 38, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Original Time</p>
                                                    <p style='margin: 0; font-size: 17px; color: #991b1b; font-weight: 600;'>{bookingTime}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(220, 38, 38, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Reason</p>
                                                    <p style='margin: 0; font-size: 17px; color: #991b1b; font-weight: 600;'>{cancellationReason}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetBookingCancellationInviteeHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
        <tr>
            <td align='center' style='padding: 50px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#dc2626' style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 50px 30px;'>
                            <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                                <tr>
                                    <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                        <span style='color: #dc2626; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>Booking Cancelled</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 55px 45px;'>
                            <p style='margin: 0 0 10px; font-size: 18px; color: #1f2937;'>Hi <strong style='color: #111827;'>{inviteeName}</strong>,</p>
                            <p style='margin: 0 0 40px; font-size: 16px; line-height: 1.5; color: #6b7280;'>Your booking has been cancelled.</p>

                            <!-- Details Card -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%); border-radius: 16px; border: 1px solid #fecaca; margin-bottom: 35px;'>
                                <tr>
                                    <td style='padding: 35px 30px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='padding: 0 0 22px 0;'>
                                                    <p style='margin: 0 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Event</p>
                                                    <p style='margin: 0; font-size: 20px; color: #7f1d1d; font-weight: 700; line-height: 1.3;'>{eventName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(220, 38, 38, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>With</p>
                                                    <p style='margin: 0; font-size: 17px; color: #991b1b; font-weight: 600;'>{hostName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(220, 38, 38, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Original Time</p>
                                                    <p style='margin: 0; font-size: 17px; color: #991b1b; font-weight: 600;'>{bookingTime}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(220, 38, 38, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Reason</p>
                                                    <p style='margin: 0; font-size: 17px; color: #991b1b; font-weight: 600;'>{cancellationReason}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <p style='margin: 0; font-size: 14px; color: #6b7280; text-align: center;'>If you need to reschedule, please visit the booking page again.</p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetBookingReminderHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
        <tr>
            <td align='center' style='padding: 50px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#f59e0b' style='background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); padding: 50px 30px;'>
                            <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                                <tr>
                                    <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                        <span style='color: #f59e0b; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>Upcoming Meeting Reminder</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 55px 45px;'>
                            <!-- Alert Banner -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%); border-radius: 12px; border: 1px solid #fcd34d; margin-bottom: 30px;'>
                                <tr>
                                    <td style='padding: 20px 25px;'>
                                        <p style='margin: 0; font-size: 16px; color: #92400e; font-weight: 600; text-align: center;'>Your meeting is coming up {timeUntil}</p>
                                    </td>
                                </tr>
                            </table>

                            <p style='margin: 0 0 10px; font-size: 18px; color: #1f2937;'>Hi <strong style='color: #111827;'>{inviteeName}</strong>,</p>
                            <p style='margin: 0 0 40px; font-size: 16px; line-height: 1.5; color: #6b7280;'>This is a reminder about your upcoming appointment.</p>

                            <!-- Details Card -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%); border-radius: 16px; border: 1px solid #fde68a; margin-bottom: 35px;'>
                                <tr>
                                    <td style='padding: 35px 30px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='padding: 0 0 22px 0;'>
                                                    <p style='margin: 0 0 6px; font-size: 13px; color: #d97706; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Event</p>
                                                    <p style='margin: 0; font-size: 20px; color: #78350f; font-weight: 700; line-height: 1.3;'>{eventName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(217, 119, 6, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #d97706; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>With</p>
                                                    <p style='margin: 0; font-size: 17px; color: #92400e; font-weight: 600;'>{hostName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(217, 119, 6, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #d97706; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>When</p>
                                                    <p style='margin: 0; font-size: 17px; color: #92400e; font-weight: 600;'>{bookingTime}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(217, 119, 6, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #d97706; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Location</p>
                                                    <p style='margin: 0; font-size: 17px; color: #92400e; font-weight: 600;'>{location}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- CTA Button -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                <tr>
                                    <td align='center' style='padding: 10px 0;'>
                                        <a href='{meetingLink}' style='display: inline-block; padding: 18px 50px; background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); color: #ffffff; text-decoration: none; border-radius: 14px; font-weight: 600; font-size: 17px; box-shadow: 0 6px 20px rgba(245, 158, 11, 0.35); letter-spacing: 0.3px;'>Join Meeting</a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetPasswordResetHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
        <tr>
            <td align='center' style='padding: 50px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#6366f1' style='background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); padding: 50px 30px;'>
                            <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                                <tr>
                                    <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                        <span style='color: #6366f1; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>Password Reset</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 55px 45px;'>
                            <p style='margin: 0 0 10px; font-size: 18px; color: #1f2937;'>Hi <strong style='color: #111827;'>{userName}</strong>,</p>
                            <p style='margin: 0 0 30px; font-size: 16px; line-height: 1.5; color: #6b7280;'>You requested a password reset. Use the code below to reset your password:</p>

                            <!-- OTP Code Card -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin: 40px 0;'>
                                <tr>
                                    <td align='center' style='background: linear-gradient(135deg, #f0f0ff 0%, #f5f3ff 100%); padding: 35px; border-radius: 12px; border: 2px solid #e0e7ff;'>
                                        <div style='font-size: 42px; font-weight: 700; background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; letter-spacing: 10px; font-family: monospace;'>{otpCode}</div>
                                    </td>
                                </tr>
                            </table>

                            <p style='margin: 0 0 10px; font-size: 14px; color: #6b7280; text-align: center;'>This code will expire in <strong style='color: #111827;'>{expiryMinutes} minutes</strong>.</p>
                            <p style='margin: 0; font-size: 14px; color: #6b7280; text-align: center;'>If you didn't request this, please ignore this email.</p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetBookingRescheduledHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
        <tr>
            <td align='center' style='padding: 50px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#3b82f6' style='background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%); padding: 50px 30px;'>
                            <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                                <tr>
                                    <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                        <span style='color: #3b82f6; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>Booking Rescheduled</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 55px 45px;'>
                            <p style='margin: 0 0 10px; font-size: 18px; color: #1f2937;'>Hi <strong style='color: #111827;'>{inviteeName}</strong>,</p>
                            <p style='margin: 0 0 40px; font-size: 16px; line-height: 1.5; color: #6b7280;'>Your booking has been rescheduled to a new time.</p>

                            <!-- Time Change Card -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%); border-radius: 16px; border: 1px solid #bfdbfe; margin-bottom: 35px;'>
                                <tr>
                                    <td style='padding: 35px 30px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='padding: 0 0 22px 0;'>
                                                    <p style='margin: 0 0 6px; font-size: 13px; color: #3b82f6; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Event</p>
                                                    <p style='margin: 0; font-size: 20px; color: #1e3a8a; font-weight: 700; line-height: 1.3;'>{eventName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(59, 130, 246, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #3b82f6; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>With</p>
                                                    <p style='margin: 0; font-size: 17px; color: #1e40af; font-weight: 600;'>{hostName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(59, 130, 246, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #dc2626; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Previous Time</p>
                                                    <p style='margin: 0; font-size: 17px; color: #991b1b; font-weight: 600; text-decoration: line-through;'>{oldBookingTime}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(59, 130, 246, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #16a34a; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>New Time</p>
                                                    <p style='margin: 0; font-size: 17px; color: #166534; font-weight: 600;'>{bookingTime}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(59, 130, 246, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #3b82f6; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Location</p>
                                                    <p style='margin: 0; font-size: 17px; color: #1e40af; font-weight: 600;'>{location}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- CTA Button -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                <tr>
                                    <td align='center' style='padding: 10px 0;'>
                                        <a href='{meetingLink}' style='display: inline-block; padding: 18px 50px; background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%); color: #ffffff; text-decoration: none; border-radius: 14px; font-weight: 600; font-size: 17px; box-shadow: 0 6px 20px rgba(59, 130, 246, 0.35); letter-spacing: 0.3px;'>Join Meeting</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Action Links -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin-top: 25px;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{rescheduleUrl}' style='display: inline-block; margin: 0 10px; padding: 12px 24px; background-color: #f3f4f6; color: #374151; text-decoration: none; border-radius: 8px; font-weight: 500; font-size: 14px; border: 1px solid #e5e7eb;'>Reschedule Again</a>
                                        <a href='{cancellationUrl}' style='display: inline-block; margin: 0 10px; padding: 12px 24px; background-color: #fef2f2; color: #dc2626; text-decoration: none; border-radius: 8px; font-weight: 500; font-size: 14px; border: 1px solid #fecaca;'>Cancel Booking</a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0 0 12px; font-size: 14px; color: #6b7280;'>Questions? Contact <a href='mailto:{hostEmail}' style='color: #3b82f6; text-decoration: none; font-weight: 600;'>{hostEmail}</a></p>
                            <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetSlotInvitationHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
        <tr>
            <td align='center' style='padding: 50px 20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#10b981' style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 50px 30px;'>
                            <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                                <tr>
                                    <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                        <span style='color: #10b981; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>Meetlr</h1>
                            <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>You're Invited!</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 55px 45px;'>
                            <p style='margin: 0 0 10px; font-size: 18px; color: #1f2937;'>Hi <strong style='color: #111827;'>{inviteeName}</strong>,</p>
                            <p style='margin: 0 0 40px; font-size: 16px; line-height: 1.5; color: #6b7280;'><strong style='color: #111827;'>{hostName}</strong> has reserved a time slot just for you and invites you to book a meeting.</p>

                            <!-- Details Card -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%); border-radius: 16px; border: 1px solid #a7f3d0; margin-bottom: 35px;'>
                                <tr>
                                    <td style='padding: 35px 30px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='padding: 0 0 22px 0;'>
                                                    <p style='margin: 0 0 6px; font-size: 13px; color: #059669; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Event</p>
                                                    <p style='margin: 0; font-size: 20px; color: #064e3b; font-weight: 700; line-height: 1.3;'>{eventName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(5, 150, 105, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #059669; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Reserved Time</p>
                                                    <p style='margin: 0; font-size: 17px; color: #047857; font-weight: 600;'>{slotTime}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0 0 22px 0; border-top: 1px solid rgba(5, 150, 105, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #059669; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Duration</p>
                                                    <p style='margin: 0; font-size: 17px; color: #047857; font-weight: 600;'>{duration} minutes</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 0; border-top: 1px solid rgba(5, 150, 105, 0.12);'>
                                                    <p style='margin: 18px 0 6px; font-size: 13px; color: #059669; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Host</p>
                                                    <p style='margin: 0; font-size: 17px; color: #047857; font-weight: 600;'>{hostName}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Expiration Warning -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%); border-radius: 12px; border: 1px solid #fcd34d; margin-bottom: 35px;'>
                                <tr>
                                    <td style='padding: 20px 25px;'>
                                        <p style='margin: 0; font-size: 15px; color: #92400e; font-weight: 600; text-align: center;'>This invitation expires in <strong>{expirationHours} hours</strong></p>
                                        <p style='margin: 8px 0 0; font-size: 13px; color: #a16207; text-align: center;'>Valid until: {expiresAt}</p>
                                    </td>
                                </tr>
                            </table>

                            <!-- CTA Button -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                <tr>
                                    <td align='center' style='padding: 10px 0;'>
                                        <a href='{bookingUrl}' style='display: inline-block; padding: 18px 50px; background-color: #10b981; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: #ffffff; text-decoration: none; border-radius: 14px; font-weight: 600; font-size: 17px; box-shadow: 0 6px 20px rgba(16, 185, 129, 0.35); letter-spacing: 0.3px;'>Book This Slot</a>
                                    </td>
                                </tr>
                            </table>

                            <p style='margin: 30px 0 0; font-size: 14px; color: #6b7280; text-align: center;'>This time slot has been reserved exclusively for you. Click the button above to confirm your booking.</p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0 0 12px; font-size: 14px; color: #6b7280;'>Questions? Contact <a href='mailto:{hostEmail}' style='color: #10b981; text-decoration: none; font-weight: 600;'>{hostEmail}</a></p>
                            <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© 2025 Meetlr. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
