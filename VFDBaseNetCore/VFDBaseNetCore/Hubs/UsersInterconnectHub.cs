using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Services.UsersActivity;
using MTF.Areas.Logging.Data;
using MTF.Areas.Logging.Models;

// To work with clients outside a Hub - use injection in the constructor
// IHubContext<UsersInterconnectHub> hc - in this particular case
// then hc.Clients - and any method required
namespace MTF.Hubs
{
    public class UsersInterconnectHub : Hub
    {
        private UserManager<CommonUser> _userManager { get; set; }
        private LogDB_Context _logDB { get; set;}

        private UAStore _UAStore { get; set; }

        public UsersInterconnectHub (UserManager<CommonUser> userManager,
                                     LogDB_Context logDB
                                    )
            : base()
        {
            _userManager = userManager;
            _logDB = logDB;

            _UAStore = GlobalParameters._UAStore;
        }
      
        // Page Actualization Technology
        // Page States Store
        private static object _locker = new object();
        private static Dictionary<string, PageState> _store = new Dictionary<string, PageState>();
        private static Dictionary<string, string> _connsToPages = new Dictionary<string, string>();

        // Use to collect data during connection establishing
        // use to user accounting
        public override async Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                // Add User to the currently loggedin pool if already not in it
                await _UAStore.EntryActualized(
                    (await _userManager.GetUserAsync(Context.User)).Id
                    );
            }
            
            await Task.CompletedTask;
        }
        
        // Use to clean any data assotiated with connection
        // including page release
        // use to user accounting
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Context.User.Identity.IsAuthenticated)
            {

                // Compromize User in the currently loggedin pool
                await _UAStore.EntryComprimized((await _userManager.GetUserAsync(Context.User)).Id);

                string pg = string.Empty;
                Queue<reqQElem> ul = null;

                lock (_locker)
                {
                    if (_connsToPages.TryGetValue(Context.ConnectionId, out pg))
                    {
                        ul = cleanConnectionFromPage(pg, Context.ConnectionId);
                    }
                }

                await SendToUsers(ul, pg);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // The operation called from clients
        public class ClientOperationParams
        {
            public string operation { get; set; }
            public string pageIdent { get; set; }
            public string msgWhenPageAvailable { get; set; }
        }
        public async Task<object> ClientOperation (ClientOperationParams prms
                                                  )
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                try
                {
                    return await putOperation(new Actor
                                                        {
                                                            connection = Context.ConnectionId,
                                                            userId = (await _userManager.GetUserAsync(Context.User)).Id,
                                                            pageIdent = prms.pageIdent,
                                                            avlMsg = prms.msgWhenPageAvailable
                                                        },
                                              prms.operation
                                            );
                }
                catch
                {
                    //return await Task.FromResult("rejected Unknown server error");
                    return await Task.FromResult(new { res = "rejected", msg = "Unknown server error" });
                }
            }
            else
            {
                return await Task.FromResult(new { res = "rejected", msg = "User is not logged in" });
            }
        }
        // Work with requests
        //
        enum  reqRes
        {
            catched,
            queued,
            rejected,
            underReview
        }

        private async Task<object> putOperation(Actor a, string operation)
        {
            reqRes reqState = reqRes.underReview;
            string resMsg = "";

            Queue<reqQElem> ul = null;
            string pg;
            
            lock (_locker)
            {
                if (_connsToPages.TryGetValue(a.connection, out pg))
                {
                    // connection already in the revers cache
                    if (a.pageIdent!=pg) // absolutely strange, page changed..
                    {
                        ul = cleanConnectionFromPage(pg,a.connection);
                        _connsToPages[a.connection] = a.pageIdent;

                        resMsg = "Page changed for connection";
                        reqState = reqRes.rejected;
                    }
                }
                else
                {
                    // adding connection into the revers cache
                    _connsToPages.Add(a.connection, a.pageIdent);
                }

                if (
                    reqState == reqRes.underReview &&
                    _store.TryGetValue(a.pageIdent, out PageState el)
                   )
                {
                    // we have state record, may be empty, for the page
                    switch (operation)
                    {
                        case "{":
                            if (el.owner != null)
                            {
                                // add new { in a queue
                                var qel = new reqQElem(a);
                                if (!el.requests.Contains(qel)) // check if this User is alreadyu in the queue 
                                {
                                    el.requests.Enqueue(qel);
                                }
                                reqState = reqRes.queued;
                            }
                            else
                            {
                                el.owner = a;
                                reqState = reqRes.catched;
                            }
                            break;
                        case "}":
                            ul = cleanConnectionFromPage(a.pageIdent, Context.ConnectionId);

                            reqState = reqRes.catched;
                            break;
                        default:
                            reqState = reqRes.rejected;
                            resMsg = "Illegal Page Actualization operation";
                            break;
                    }
                }
                else
                {
                    // we have not any state record for the page
                    switch (operation)
                    {
                        case "{":
                            // create new page state
                            _store.Add(a.pageIdent, new PageState(a));
                            reqState = reqRes.catched;
                            break;
                        case "}":
                            // Nothing to do, strange situation
                            reqState = reqRes.rejected;
                            resMsg = "} without {";
                            break;
                        default:
                            reqState = reqRes.rejected;
                            resMsg = "Illegal Page Actualization operation";
                            break;
                    }
                }
            } // lock end

            await SendToUsers(ul, pg);

            return await Task.FromResult(new { res = reqState.ToString(), msg = resMsg });
        }

        // Procedure to remove died connection from all caches
        private Queue<reqQElem> cleanConnectionFromPage(string page,
                                                        string connection)
        {
            // should clean all references to the connection and
            // return a list of all waiting users
            Queue<reqQElem> q = null;

           lock (_locker)
            {
                _connsToPages.Remove(connection);

                if (_store.TryGetValue(page, out PageState el))
                {
                    if (el.owner.connection == connection)
                    {
                        q = el.requests;
                        _store.Remove(page);
                    }
                }
            }

            return q;
        }

        // Just to send msg to users from list
        protected async Task SendToUsers(Queue<reqQElem> ul, string pg)
        {
            if (ul != null)
            {
                foreach (var cu in ul)
                {
                    // Notify all users from queue that page is available
                    await Clients.User(cu.userId).SendAsync("PageIsAvailable",
                                                            new { pageIdent = pg, 
                                                                  msg = cu.avlMsg }
                                                            );
                }
            }
            await Task.CompletedTask;
        }

        // Simple Common Chat for SignIned Users
        public class SendMessagePrms
        {
            public string message { get; set; }
        }
        public async Task SendMessage(SendMessagePrms prms)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (
                (Context.User.Identity.IsAuthenticated) &&
                (user != null)
               )
            {
                var chtHElem = new ChatHistory();
                chtHElem.dateIn = DateTime.Now;
                chtHElem.uId = user.Id;
                chtHElem.msg = prms.message;
                chtHElem.machineName = Environment.MachineName;
                chtHElem.appIdent = GlobalParameters.AppIdent;

                await _logDB.chatHistory.AddAsync(chtHElem);
                await _logDB.SaveChangesAsync();

                await Clients.All.SendAsync("ReceiveMessage", new { user = user.FullName, message = prms.message });
                // Clients.User(userId).SendAsync send a msg to all connections from user
                // may be useful for notification (!)
            }
        }

        // Claims Registration
        public class SendClaimPrms
        {
            public string message { get; set; }
        }
        public async Task SendClaim(SendClaimPrms prms)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (
                (Context.User.Identity.IsAuthenticated) &&
                (user != null)
               )
            {
                var claim = new ClaimsLog();
                claim.author = user.Email;
                claim.claimText = prms.message;

                await _logDB.claimsLog.AddAsync(claim);
                await _logDB.SaveChangesAsync();
            }
        }
    }
}