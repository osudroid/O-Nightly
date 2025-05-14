using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Mailer.Adapter.Interface;

public interface IMailerProvider {
    public ResultNone MainSendResetEmail(long userId, string username, string email, string token);

    public ResultNone MainSendPatreonVerifyLinkToken(string username, string email, Guid token);

    public ResultNone MainSendDropAccountVerifyLinkToken(string username, string email, string token);
    public ResultNone MainSendSignupEmail(string username, string email, string token);
}