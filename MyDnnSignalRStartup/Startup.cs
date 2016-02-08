using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using MyDnn.SignalRStartup;
using Owin;
using System;


[assembly: OwinStartup(typeof(Startup))]
namespace MyDnn.SignalRStartup
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}