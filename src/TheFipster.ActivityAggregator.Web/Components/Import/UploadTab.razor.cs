using Microsoft.AspNetCore.Components;

namespace TheFipster.ActivityAggregator.Web.Components.Import;

public partial class UploadTab : ComponentBase
{
    private string uploadKey = Guid.NewGuid().ToString();

    private void OnUploadFinished()
    {
        uploadKey = Guid.NewGuid().ToString();
    }
}
