using System.Reflection;
using LamLibAllOver;
using NLog;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Security.Adapter.Interface;

namespace Rimu.Repository.Security.Domain;

public class SecurityProvider: ISecurityProvider {
    private static ISecurity? _security;
    private static ISecurityPhp? _securityPhp;
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public ISecurity Security => SecurityProvider.GetSecurity();
    public ISecurityPhp SecurityPhp => SecurityProvider.GetSecurityPhp();
    
    protected internal static ISecurity GetSecurity() {
        if (_security is not null) return _security;
        try {
            var osuDroidSecurityDll = Dependency.Adapter.Injection.GlobalServiceProvider
                                          .GetEnvJson()
                                          .OSUDROID_SECURITY_DLL;

            
            Logger.Info("TryLoad Load Extra Security DLL");
            
            var path = PathProcess() + osuDroidSecurityDll;
            var assembly = Assembly.LoadFile(path);
            _security = (ISecurity)Activator.CreateInstance(assembly.ExportedTypes.FirstOrDefault()!)!;
            Logger.Info("Extra Security DLL Loaded");
            return _security;
        }
        catch (Exception e) {
            Logger.Warn(e, "Failed to load security dll");
            Logger.Info("Use Default Security");
            _security = new PassSecurity();
            return _security;
        }
    }
    
    protected internal static ISecurityPhp GetSecurityPhp() {
        if (_security is not null) return _securityPhp;
        try {
            var osuDroidSecurityDll = Dependency.Adapter.Injection.GlobalServiceProvider
                                                .GetEnvJson()
                                                .OSUDROID_SECURITY_DLL;

            
            Logger.Info("TryLoad Load Extra Security DLL");
            
            var path = PathProcess() + osuDroidSecurityDll;
            var assembly = Assembly.LoadFile(path);
            _security = (ISecurity)Activator.CreateInstance(assembly.ExportedTypes.FirstOrDefault()!)!;
            Logger.Info("Extra Security DLL Loaded");
            return _securityPhp;
        }
        catch (Exception e) {
            Logger.Warn(e, "Failed to load security dll");
            Logger.Info("Use Default Security");
            _security = new PassSecurity();
            return _securityPhp;
        }
    }
    
    private static string PathProcess() { 
        return System.Environment.ProcessPath!.Remove(System.Environment.ProcessPath.LastIndexOf('/') + 1);
    }
}