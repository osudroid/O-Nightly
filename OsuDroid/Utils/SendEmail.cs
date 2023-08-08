using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace OsuDroid.Utils;

public static class SendEmail {
    public static ResultErr<string> MainSendResetEmail(long userId, string username, string email, string token) {
        try
        {
            var domain = Setting.Domain_Name!.Value;
            var emailNoReplayUsername = Setting.Email_NoReplayUsername!.Value;
            var emailNoReplay = Setting.Email_NoReplay!.Value;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailNoReplayUsername, emailNoReplay));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = "OsuDroid Reset Password";
            message.Body = new TextPart(TextFormat.Text) {
                Text = @$"

[osu!droid] Reset Password
 
Someone has requested a password reset from your '{username}' osu!droid Account.

        This Session Expires in : 15 min's
        If this was you, click the link below.
        https://{domain}/reset/passwd/{userId}/{token}

        If this wasn't you, reset your password https://home.cyberpit.de/reset/passwd/{userId}/{token}
        _________________________________________________
            This e-mail is sent by the system, do not reply.
"
            };

            Send(message);
            return ResultErr<string>.Ok();
        }
        catch (Exception e)
        {
            return ResultErr<string>.Err(e.ToString());
        }
    }

    public static void MainSendPatreonVerifyLinkToken(string username, string email, Guid token) {
        var domain = Setting.Domain_Name!.Value;
        var emailNoReplayUsername = Setting.Email_NoReplayUsername!.Value;
        var emailNoReplay = Setting.Email_NoReplay!.Value;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(emailNoReplayUsername, emailNoReplay));
        message.To.Add(new MailboxAddress(username, email));
        message.Subject = "OsuDroid Patreon Verify Token";
        message.Body = new TextPart(TextFormat.Text) {
            Text = @$"
Someone has requested to add your Patreon account to your osu!droid Account.

This Session Expires in : 5 min's
If this was you, click the link below.
https://{domain}/profile/accept/patreonemail/token/{token}

_________________________________________________
This e-mail is sent by the system, do not reply.
"
        };

        Send(message);
    }

    public static void MainSendDropAccountVerifyLinkToken(string username, string email, Guid token) {
        var domain = Setting.Domain_Name!.Value;
        var emailNoReplayUsername = Setting.Email_NoReplayUsername!.Value;
        var emailNoReplay = Setting.Email_NoReplay!.Value;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(emailNoReplayUsername, emailNoReplay));
        message.To.Add(new MailboxAddress(username, email));
        message.Subject = "OsuDroid Remove Account Verify Token";
        message.Body = new TextPart(TextFormat.Text) {
            Text = @$"
Someone has requested to delete your osu!droid Account.

This Session Expires in : 5 min's
If this was you, click the link below.
https://{domain}/deletion/{token}

If this was not you, ignore this Email.

Note:
Your Account can't be recovered, if you delete your Account!
_________________________________________________
This e-mail is sent by the system, do not reply.
"
        };

        Send(message);
    }

    private static void Send(MimeMessage message) {
        var domain = Setting.Domain_Name!.Value;
        var emailNoReplayUsername = Setting.Email_NoReplayUsername!.Value;
        var emailNoReplay = Setting.Email_NoReplay!.Value;

        using var client = new SmtpClient();
        client.Connect(Setting.Email_NoReplaySmtpAddress!.Value, 587);
        client.Authenticate(Setting.Email_NoReplay!.Value, Setting.Email_NoReplayPassword!.Value);
        client.Send(message);
        client.Disconnect(true);
    }
}