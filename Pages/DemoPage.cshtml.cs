using awt-pj-ws23-24-mobile-streaming-1.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace awt-pj-ws23-24-mobile-streaming-1.Pages;

public class DemoPageModel : PageModel
{
    private readonly ILogger<DemoPageModel> _logger;

    public DemoPageModel(ILogger<DemoPageModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        var mes = new Measurement();
    }
}
