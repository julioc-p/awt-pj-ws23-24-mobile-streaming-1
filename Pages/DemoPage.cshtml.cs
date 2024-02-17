using awt_pj_ws23_24_mobile_streaming_1.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace awt_pj_ws23_24_mobile_streaming_1.Pages;

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
