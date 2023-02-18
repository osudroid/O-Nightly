namespace OsuDroid.Lib.Validate;

public abstract class ValidateAll {
    public enum EErrBy {
        None,
        Email,
        Passwd,
        Username,
        NewPasswd,
        NewEmail,
        OldEmail,
        NewUsername,
        OldUsername,
        OldPasswd,
        Else
    }

    public virtual ResultErr<EErrBy> AnyValidate() {
        var work = true;
        if (IValidateEmail.Type.IsInstanceOfType(this)) {
            work = IValidateEmail.ValidateEmail((IValidateEmail)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.Email);
        }

        if (IValidateNewEmail.Type.IsInstanceOfType(this)) {
            work = IValidateNewEmail.ValidateNewEmail((IValidateNewEmail)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.NewEmail);
        }

        if (IValidateNewPasswd.Type.IsInstanceOfType(this)) {
            work = IValidateNewPasswd.ValidateNewPasswd((IValidateNewPasswd)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.NewPasswd);
        }

        if (IValidateNewUsername.Type.IsInstanceOfType(this)) {
            work = IValidateNewUsername.ValidateNewUsername((IValidateNewUsername)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.NewUsername);
        }

        if (IValidateOldEmail.Type.IsInstanceOfType(this)) {
            work = IValidateOldEmail.ValidateOldEmail((IValidateOldEmail)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.OldEmail);
        }

        if (IValidateOldPasswd.Type.IsInstanceOfType(this)) {
            work = IValidateOldPasswd.ValidateOldPasswd((IValidateOldPasswd)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.OldPasswd);
        }

        if (IValidateOldUsername.Type.IsInstanceOfType(this)) {
            work = IValidateOldUsername.ValidateOldUsername((IValidateOldUsername)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.OldUsername);
        }

        if (IValidatePasswd.Type.IsInstanceOfType(this)) {
            work = IValidatePasswd.ValidatePasswd((IValidatePasswd)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.Passwd);
        }

        if (IValidateUsername.Type.IsInstanceOfType(this)) {
            work = IValidateUsername.ValidateUsername((IValidateUsername)this);
            if (work == false)
                return ResultErr<EErrBy>.Err(EErrBy.Username);
        }

        return ResultErr<EErrBy>.Ok();
    }
}