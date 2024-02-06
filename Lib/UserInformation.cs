using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace BTRReportProcesser.Lib
{
        public class UserInformation
        {
            public String DisplayUserName;
            public String First;
            public String Last;
            public User CurrentUser;
            public Task Init { get; private set; }

            public UserInformation()
            {
                Init = GetCurrentUser();
                this.DisplayUserName = "";
                this.First = "";
                this.Last = "";
                this.CurrentUser = null;
            }

            private async Task GetCurrentUser()
            {
                var hold = await User.FindAllAsync();
                this.CurrentUser = hold[0];
                IReadOnlyList<User> users = await User.FindAllAsync();

                var current = users.Where(p => p.AuthenticationStatus == UserAuthenticationStatus.LocallyAuthenticated &&
                                            p.Type == UserType.LocalUser).FirstOrDefault();

                // user may have username
                var data = await current.GetPropertyAsync(KnownUserProperties.AccountName);
                DisplayUserName = (string)data;

                // user might not have this either
                First = (string)await current.GetPropertyAsync(KnownUserProperties.FirstName);
                Last = (string)await current.GetPropertyAsync(KnownUserProperties.LastName);

            }
        }
}
