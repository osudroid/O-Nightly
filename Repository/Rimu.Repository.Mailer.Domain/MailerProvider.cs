using LamLibAllOver.ErrorHandling;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MimeKit;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Mailer.Adapter.Interface;

namespace Rimu.Repository.Mailer.Domain;

public class MailerProvider: IMailerProvider {
  private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IEnvDb _envDb;
    
    public MailerProvider(IEnvDb envDb) => _envDb = envDb;

    public ResultNone MainSendResetEmail(long userId, string username, string email, string token) {
        try {
            
            var domain = _envDb.Domain_Name;
            var emailNoReplayUsername = _envDb.Email_NoReplay;
            var emailNoReplay = _envDb.Email_NoReplay;

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
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultNone.Err;
        }
    }

    public ResultNone MainSendPatreonVerifyLinkToken(string username, string email, Guid token) {
        try {
            var domain = _envDb.Domain_Name;
            var emailNoReplayUsername = _envDb.Email_NoReplayUsername;
            var emailNoReplay = _envDb.Email_NoReplay;

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
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultNone.Err;
        }
    }

    public ResultNone MainSendDropAccountVerifyLinkToken(string username, string email, string token) {
        try {
            var domain = _envDb.Domain_Name;
            var emailNoReplayUsername = _envDb.Email_NoReplayUsername;
            var emailNoReplay = _envDb.Email_NoReplay;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailNoReplayUsername, emailNoReplay));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = "OsuDroid Remove Account Verify Token";
            message.Body = new TextPart(TextFormat.Text) {
                Text = @$"
Someone has requested to delete your osu!droid Account.

This Session Expires in : 5 min's
If this was you, click the link below.
https://{domain}/api2/profile/drop-account?token={token}

If this was not you, ignore this Email.

Note:
Your Account can't be recovered, if you delete your Account!
_________________________________________________
This e-mail is sent by the system, do not reply.
"
            };

            Send(message);
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultNone.Err;
        }
    }

    public ResultNone MainSendSignupEmail(string username, string email, string token) {
        try {
            var domain = _envDb.Domain_Name;
            var emailNoReplayUsername = _envDb.Email_NoReplayUsername;
            var emailNoReplay = _envDb.Email_NoReplay;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailNoReplayUsername, emailNoReplay));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = "OsuDroid New Account";
            // TODO Write MainSendSignupEmail TEXT
            message.Body = new TextPart(TextFormat.Text) {
                Text = @$"
New Signup in OsuDroid 
token = {token}
username = {username}
email = {email}
domain = {domain}
"
            };

            Send(message);
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultNone.Err;
        }
    }

    private void Send(MimeMessage message) {
        using var client = new SmtpClient();
        
        client.Connect(_envDb.Email_NoReplaySmtpAddress, 587);
        client.Authenticate(_envDb.Email_NoReplay, _envDb.Email_NoReplayPassword);
        client.Send(message);
        client.Disconnect(true);
    }
}