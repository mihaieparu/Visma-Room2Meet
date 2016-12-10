using Microsoft.Owin;
using Owin;
using System.Data.Entity;

[assembly: OwinStartupAttribute(typeof(Visma_Room2Meet.Startup))]
namespace Visma_Room2Meet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer<room2meet_dbEntities>(null);
            ConfigureAuth(app);
        }
    }
}
