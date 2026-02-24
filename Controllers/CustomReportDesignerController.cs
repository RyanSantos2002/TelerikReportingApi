using Microsoft.AspNetCore.Mvc;
using Telerik.Reporting.Services;
using Telerik.WebReportDesigner.Services;
using Telerik.WebReportDesigner.Services.Controllers;

namespace TelerikReportingApi.Controllers
{
    [Route("api/reportdesigner")]
    [ApiController] // Mantenha isso agora
    public class CustomReportDesignerController : ReportDesignerControllerBase
    {
        public CustomReportDesignerController(
            IReportDesignerServiceConfiguration reportDesignerServiceConfiguration, 
            IReportServiceConfiguration reportServiceConfiguration)
            : base(reportDesignerServiceConfiguration, reportServiceConfiguration)
        {
        }

    }
}