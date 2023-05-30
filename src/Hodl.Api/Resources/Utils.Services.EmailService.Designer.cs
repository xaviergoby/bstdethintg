﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hodl.Api.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Utils_Services_EmailService {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Utils_Services_EmailService() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Hodl.Api.Resources.Utils.Services.EmailService", typeof(Utils_Services_EmailService).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirm your email with code.
        /// </summary>
        internal static string ConfirmationEmailText {
            get {
                return ResourceManager.GetString("ConfirmationEmailText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Growity email confirmation.
        /// </summary>
        internal static string ConfirmationEmailTitle {
            get {
                return ResourceManager.GetString("ConfirmationEmailTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Disable 2FA with code.
        /// </summary>
        internal static string Disable2FaEmailText {
            get {
                return ResourceManager.GetString("Disable2FaEmailText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Growity disable 2FA Code.
        /// </summary>
        internal static string Disable2FaEmailTitle {
            get {
                return ResourceManager.GetString("Disable2FaEmailTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reset your password with code.
        /// </summary>
        internal static string ResetPasswordEmailText {
            get {
                return ResourceManager.GetString("ResetPasswordEmailText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Growity reset password.
        /// </summary>
        internal static string ResetPasswordEmailTitle {
            get {
                return ResourceManager.GetString("ResetPasswordEmailTitle", resourceCulture);
            }
        }
    }
}
