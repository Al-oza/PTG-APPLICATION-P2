﻿using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using PTGApplication.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PTGApplication
{
    public class EmailService : IIdentityMessageService
    {
        private JArray BuildMessage(IdentityMessage message)
        {
            return new JArray{
                new JObject {
                    { "From", new JObject {
                        { "Email", Properties.SharedResources.Email },
                        { "Name", Properties.SharedResources.RegistrationSender }
                    } },
                    { "To", new JArray { new JObject { { "Email", message.Destination } } } },
                    { "Subject", message.Subject },
                    { "TextPart", message.Body },
                    { "HTMLPart", message.Body },
                    { "CustomID", "Registration Email" }
                }
            };
        }
        private async Task ConfigureMailjet(IdentityMessage message)
        {
            var client = new MailjetClient(
                Environment.GetEnvironmentVariable(Properties.SharedResources.MailJetApiKey1),
                Environment.GetEnvironmentVariable(Properties.SharedResources.MailJetApiKey2))
            { Version = ApiVersion.V3_1 };

            var request = new MailjetRequest { Resource = Send.Resource }
            .Property(Send.Messages, BuildMessage(message));
            var response = await client.PostAsync(request);

            if (!response.IsSuccessStatusCode)
            { await Task.FromResult(0); }
        }
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            // return Task.FromResult(0);
            return ConfigureMailjet(message);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager which is used in this application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.  
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager)
        { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
