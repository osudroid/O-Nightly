using System.Linq.Expressions;
using FastEndpoints;
using FluentValidation;

namespace Rimu.Web.Gen2.Rule;

public static class DefaultRule {
    public static AbstractValidator<T> UseRuleUsername<T>(this AbstractValidator<T> self, Expression<Func<T, string>> expression) where T : notnull {
        self.RuleFor(expression)
            .MinimumLength(1)
            .WithMessage("Minimum length is 1.")
            .MaximumLength(25)
            .WithMessage("Maximum length is 25.");
        ;
        
        return self;
    }
    
    public static AbstractValidator<T> UseRulePassword<T>(this AbstractValidator<T> self, Expression<Func<T, string>> expression) where T : notnull {
        self.RuleFor(expression)
            .MinimumLength(2)
            .WithMessage("Minimum length is 2.")
            .MaximumLength(128)
            .WithMessage("Maximum length is 128.");
        ;
        
        return self;
    }
    
    public static AbstractValidator<T> UseRuleEmail<T>(this AbstractValidator<T> self, Expression<Func<T, string>> expression) where T : notnull {
        self.RuleFor(expression)
            .MinimumLength(3)
            .WithMessage("Minimum length is 3.")
            .Must(RegexValidation.ValidateEmail)
            .WithMessage("Email format is invalid.");
            ;
            
            return self;
    }
    
    public static AbstractValidator<T> UseRuleBase64<T>(this AbstractValidator<T> self, Expression<Func<T, string>> expression) where T : notnull {
        self.RuleFor(expression)
            .MinimumLength(1)
            .WithMessage("Minimum length is 1.")
            .Must(RegexValidation.ValidateBase64)
            .WithMessage("Invalid base64 encoded string.");
        ;

        return self;
    }
    
    public static AbstractValidator<T> UseRuleHex<T>(this AbstractValidator<T> self, Expression<Func<T, string>> expression) where T : notnull {
        self.RuleFor(expression)
            .MinimumLength(1)
            .WithMessage("Minimum length is 1.")
            .Must(RegexValidation.ValidateHex)
            .WithMessage("Invalid hex encoded string.");
        ;

        return self;
    }
}