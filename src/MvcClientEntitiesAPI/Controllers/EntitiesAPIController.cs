using Microsoft.AspNetCore.Mvc;

namespace MvcClientEntitiesAPI.Controllers
{
    public class EntitiesAPIController : Controller
    {
        // 
        // GET: /EntitiesAPI/

        public IActionResult Index()
        {
            return View();
        }

        // 
        // GET: /EntitiesAPI/Refresh/ 
        public async System.Threading.Tasks.Task<IActionResult> RefreshAsync(string refToken)
        {
            var client = new MvcClient.EntitiesAPI.Client();
            await client.Refresh(refToken);

            ViewData["AccessToken"] = client.accessToken;
            ViewData["ApiPayload"] = client.apiPayload;
            ViewData["ApiResponse"] = client.apiResp;
            ViewData["ApiSummary"] = client.apiSummary;
            ViewData["NewRefToken"] = client.newRefToken;
            ViewData["RefToken"] = refToken;

            return View();
        }
    }
}
