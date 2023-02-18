using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace OsuDroid.Utils;

public static class SendEmail {
    public static void MainSendResetEmail(long userId, string username, string email, string token) {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(Env.NoReplayEmailUsername, Env.NoReplayEmail));
        message.To.Add(new MailboxAddress(username, email));
        message.Subject = "OsuDroid Reset Password";
        message.Body = new TextPart(TextFormat.Text) {
            Text = @$"

[osu!droid] Reset Password
 
Someone has requested a password reset from your '{username}' osu!droid Account.

        This Session Expires in : 15 min's
        If this was you, click the link below.
        https://{Env.Domain}/reset/passwd/{userId}/{token}

        If this wasn't you, reset your password https://home.cyberpit.de/reset/passwd/{userId}/{token}
        _________________________________________________
            This e-mail is sent by the system, do not reply.
"
        };

        Send(message);
    }

    public static void MainSendPatreonVerifyLinkToken(string username, string email, Guid token) {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(Env.NoReplayEmailUsername, Env.NoReplayEmail));
        message.To.Add(new MailboxAddress(username, email));
        message.Subject = "OsuDroid Patreon Verify Token";
        message.Body = new TextPart(TextFormat.Text) {
            Text = @$"
Someone has requested to add your Patreon account to your osu!droid Account.

This Session Expires in : 5 min's
If this was you, click the link below.
https://{Env.Domain}/profile/accept/patreonemail/token/{token}

_________________________________________________
This e-mail is sent by the system, do not reply.
"
        };

        Send(message);
    }

    public static void MainSendDropAccountVerifyLinkToken(string username, string email, Guid token) {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(Env.NoReplayEmailUsername, Env.NoReplayEmail));
        message.To.Add(new MailboxAddress(username, email));
        message.Subject = "OsuDroid Remove Account Verify Token";
        message.Body = new TextPart(TextFormat.Text) {
            Text = @$"
Someone has requested to delete your osu!droid Account.

This Session Expires in : 5 min's
If this was you, click the link below.
https://{Env.Domain}/deletion/{token}

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
        using var client = new SmtpClient();
        client.Connect(Env.NoReplayEmailSmtpAddress, 587);
        client.Authenticate(Env.NoReplayEmail, Env.NoReplayEmailPasswd);
        client.Send(message);
        client.Disconnect(true);
    }
}